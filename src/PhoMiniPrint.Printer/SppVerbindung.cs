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
            lock (_sperre)
            {
                _rohdaten.AddRange(puffer.AsSpan(0, gelesen));
                Meldungen.AddRange(_leser.Hinzufuegen(puffer.AsSpan(0, gelesen)));
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
