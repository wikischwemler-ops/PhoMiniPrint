using System.IO.Ports;

namespace PhoMiniPrint.Printer;

/// <summary>
/// Bluetooth-SPP-Verbindung zum T02 über den virtuellen COM-Port von Windows.
/// Sendet in Paketen à 128 Byte mit 20 ms Pause (Werte aus dem TiMini-Print-Profil).
/// </summary>
public sealed class SppVerbindung : IDisposable
{
    public const int Paketgroesse = 128;
    public static readonly TimeSpan Paketpause = TimeSpan.FromMilliseconds(20);

    private readonly SerialPort _port;
    private readonly T02Antwortleser _leser = new();
    private readonly List<byte> _rohdaten = [];
    private readonly Lock _sperre = new();

    public SppVerbindung(string comPort)
    {
        _port = new SerialPort(comPort, 115200)
        {
            ReadTimeout = 500,
            WriteTimeout = 10000,
        };
        _port.DataReceived += (_, _) => Empfangen();
    }

    /// <summary>Alle seit dem Öffnen empfangenen Meldungen.</summary>
    public List<T02Meldung> Meldungen { get; } = [];

    /// <summary>
    /// Öffnet den Port. Direkt nach dem Schließen einer vorherigen Verbindung meldet Windows
    /// am T02 kurzzeitig eine Zeitüberschreitung (gemessen in M1) – daher bis zu 4 Versuche.
    /// </summary>
    public void Oeffnen(int versuche = 4)
    {
        for (var versuch = 1; ; versuch++)
        {
            try
            {
                _port.Open();
                return;
            }
            catch (IOException) when (versuch < versuche)
            {
                Thread.Sleep(TimeSpan.FromSeconds(3));
            }
        }
    }

    public void Senden(ReadOnlySpan<byte> daten, IProgress<double>? fortschritt = null)
    {
        for (var start = 0; start < daten.Length; start += Paketgroesse)
        {
            var laenge = Math.Min(Paketgroesse, daten.Length - start);
            _port.Write(daten.Slice(start, laenge).ToArray(), 0, laenge);
            fortschritt?.Report((double)(start + laenge) / daten.Length);
            Thread.Sleep(Paketpause);
        }
    }

    /// <summary>
    /// Wartet, bis der Sendepuffer leer ist und der Drucker die Zeilen gedruckt haben dürfte.
    /// Wird die Verbindung vorher geschlossen, fehlt das Ende des Ausdrucks (gemessen in M1).
    /// Druckzeit je Zeile: 0,125 mm × 0,11 s/mm (Schätzung aus TiMini-Print) plus Sicherheitszuschlag.
    /// </summary>
    public void AufDruckendeWarten(int zeilen)
    {
        var frist = DateTime.UtcNow.AddSeconds(30);
        while (_port.BytesToWrite > 0 && DateTime.UtcNow < frist)
        {
            Thread.Sleep(50);
        }

        var druckzeit = TimeSpan.FromSeconds((zeilen * 0.125 * 0.11) + 3);
        Thread.Sleep(druckzeit);
    }

    /// <summary>Kennung der Meldung „Druck fertig“ (1A 0F 0C), am T02 gemessen in M1.</summary>
    public const byte KennungDruckFertig = 0x0F;

    /// <summary>
    /// Wartet auf die Meldung „Druck fertig“, die nach dem Senden eingeht (FR-020, E-2).
    /// Liefert false, wenn sie innerhalb der Frist ausbleibt – Hinweis „Papierstau?“.
    /// </summary>
    /// <param name="abMeldung">Anzahl der Meldungen vor dem Senden; nur spätere zählen.</param>
    public bool AufFertigmeldungWarten(int abMeldung, TimeSpan frist)
    {
        var ende = DateTime.UtcNow + frist;
        while (DateTime.UtcNow < ende)
        {
            lock (_sperre)
            {
                if (Meldungen.Skip(abMeldung).Any(m => m.Kennung == KennungDruckFertig))
                {
                    return true;
                }
            }

            Thread.Sleep(100);
        }

        return false;
    }

    /// <summary>Anzahl der bisher empfangenen Meldungen (für <see cref="AufFertigmeldungWarten"/>).</summary>
    public int AnzahlMeldungen
    {
        get
        {
            lock (_sperre)
            {
                return Meldungen.Count;
            }
        }
    }

    /// <summary>Sendet eine Abfrage und wartet die angegebene Zeit auf Antworten.</summary>
    public (IReadOnlyList<T02Meldung> Meldungen, byte[] Roh) Abfragen(byte[] befehl, TimeSpan warten)
    {
        int vorherMeldungen, vorherRoh;
        lock (_sperre)
        {
            vorherMeldungen = Meldungen.Count;
            vorherRoh = _rohdaten.Count;
        }

        Senden(befehl);
        Thread.Sleep(warten);

        lock (_sperre)
        {
            return ([.. Meldungen.Skip(vorherMeldungen)], [.. _rohdaten.Skip(vorherRoh)]);
        }
    }

    /// <summary>
    /// Sendet eine Abfrage und wartet höchstens <paramref name="frist"/> auf die erste Antwort mit der Kennung.
    /// Liefert die Antwort (oder null) und die gemessene Antwortzeit.
    /// </summary>
    public (T02Meldung? Antwort, TimeSpan Dauer) AbfrageMitAntwort(byte[] befehl, byte kennung, TimeSpan frist)
    {
        var vorher = AnzahlMeldungen;
        var uhr = System.Diagnostics.Stopwatch.StartNew();
        Senden(befehl);
        while (uhr.Elapsed < frist)
        {
            lock (_sperre)
            {
                foreach (var m in Meldungen.Skip(vorher))
                {
                    if (m.Kennung == kennung)
                    {
                        return (m, uhr.Elapsed);
                    }
                }
            }

            Thread.Sleep(10);
        }

        return (null, uhr.Elapsed);
    }

    /// <summary>Wird bei jeder empfangenen Meldung ausgelöst (auf dem Empfangs-Thread).</summary>
    public event EventHandler<T02Meldung>? MeldungEmpfangen;

    public bool IstOffen => _port.IsOpen;

    private void Empfangen()
    {
        try
        {
            var anzahl = _port.BytesToRead;
            if (anzahl <= 0)
            {
                return;
            }

            var puffer = new byte[anzahl];
            var gelesen = _port.Read(puffer, 0, anzahl);
            IReadOnlyList<T02Meldung> neu;
            lock (_sperre)
            {
                _rohdaten.AddRange(puffer.AsSpan(0, gelesen));
                neu = _leser.Hinzufuegen(puffer.AsSpan(0, gelesen));
                Meldungen.AddRange(neu);
            }

            foreach (var meldung in neu)
            {
                MeldungEmpfangen?.Invoke(this, meldung);
            }
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or TimeoutException)
        {
            // Verbindung wurde geschlossen oder getrennt – wird beim nächsten Senden gemeldet.
        }
    }

    public void Dispose()
    {
        if (_port.IsOpen)
        {
            _port.Close();
        }

        _port.Dispose();
    }
}
