using System.IO;
using PhoMiniPrint.Printer;

namespace PhoMiniPrint.Core;

/// <summary>
/// Hält die Verbindung zum T02 offen, fragt alle 10 s Akku und Papier ab (F-01, F-02) und druckt
/// über dieselbe Verbindung. Während eines Drucks ruhen die Abfragen; Meldungen des Druckers
/// werden aber immer ausgewertet. Nach drei Ausfällen in Folge wird getrennt und neu verbunden.
/// </summary>
public sealed class Druckerverbindung : IDisposable
{
    public static readonly TimeSpan Abfrageabstand = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan Antwortfrist = TimeSpan.FromSeconds(2);
    private const int MaxAusfaelle = 3;

    private const byte KennungUeberhitzt = 0x03;
    private const byte KennungAkku = 0x04;
    private const byte KennungPapier = 0x06;

    private readonly SemaphoreSlim _zugriff = new(1, 1);
    private readonly CancellationTokenSource _ende = new();
    private readonly Akkuglaettung _akku = new();
    private readonly Verbindungsqualitaet _qualitaet = new();
    private readonly Lock _statusSperre = new();
    private readonly Func<string?> _bevorzugteAdresse;

    private SppVerbindung? _verbindung;
    private T02Fundstelle? _drucker;
    private Druckerstatus _status = Druckerstatus.Unverbunden;
    private int _ausfaelle;
    private Task? _schleife;

    /// <param name="bevorzugteAdresse">Liefert die Adresse des zuletzt verwendeten Druckers (F-10).</param>
    public Druckerverbindung(Func<string?> bevorzugteAdresse)
    {
        _bevorzugteAdresse = bevorzugteAdresse;
    }

    /// <summary>Neuer Status; wird auf einem Hintergrund-Thread ausgelöst.</summary>
    public event EventHandler<Druckerstatus>? StatusGeaendert;

    /// <summary>Ein Drucker wurde gefunden und verbunden (für „zuletzt verwendet“).</summary>
    public event EventHandler<T02Fundstelle>? Verbunden;

    public Druckerstatus Status
    {
        get
        {
            lock (_statusSperre)
            {
                return _status;
            }
        }
    }

    /// <summary>Startet Verbindungsaufbau und regelmäßige Abfrage im Hintergrund.</summary>
    public void Starten() => _schleife ??= Task.Run(() => SchleifeAsync(_ende.Token));

    /// <summary>Sofort neu suchen und verbinden (Menü „Drucker suchen“).</summary>
    public async Task NeuVerbindenAsync()
    {
        await _zugriff.WaitAsync().ConfigureAwait(false);
        try
        {
            Trennen();
            Abfragen();
        }
        finally
        {
            _zugriff.Release();
        }
    }

    /// <summary>Druckt über die offene Verbindung; verbindet vorher, falls nötig.</summary>
    public async Task<Druckergebnis> DruckenAsync(Druckdaten daten, IProgress<Druckfortschritt>? fortschritt)
    {
        ArgumentNullException.ThrowIfNull(daten);
        await _zugriff.WaitAsync().ConfigureAwait(false);
        try
        {
            return await Task.Run(() =>
            {
                fortschritt?.Report(new Druckfortschritt(Druckphase.Verbinden, 0));
                if (_verbindung is null && !Verbinden())
                {
                    return new Druckergebnis(false, _drucker is null
                        ? "Kein gekoppelter T02 gefunden. Bitte den Drucker einschalten und in Windows koppeln."
                        : $"Keine Verbindung zum {_drucker.Name} ({_drucker.ComPort}). Ist der Drucker eingeschaltet?");
                }

                StatusAendern(s => s with { Zustand = Verbindungszustand.Druckt });
                var ergebnis = Druckdienst.AufVerbindungDrucken(_verbindung!, daten, fortschritt);
                _akku.DruckBeendet(DateTime.UtcNow);
                if (!_verbindung!.IstOffen)
                {
                    Trennen();
                }
                else
                {
                    StatusAendern(s => s with { Zustand = Verbindungszustand.Bereit });
                }

                return ergebnis;
            }).ConfigureAwait(false);
        }
        finally
        {
            _zugriff.Release();
        }
    }

    private async Task SchleifeAsync(CancellationToken abbruch)
    {
        using var takt = new PeriodicTimer(Abfrageabstand);
        do
        {
            if (await _zugriff.WaitAsync(0, abbruch).ConfigureAwait(false))
            {
                try
                {
                    Abfragen();
                }
                finally
                {
                    _zugriff.Release();
                }
            }
        }
        while (await takt.WaitForNextTickAsync(abbruch).ConfigureAwait(false));
    }

    /// <summary>Eine Abfragerunde: ggf. verbinden, dann Akku und Papier abfragen.</summary>
    private void Abfragen()
    {
        if (_verbindung is null && !Verbinden())
        {
            return;
        }

        var akku = Abfrage(T02Befehle.AbfrageAkku(), KennungAkku);
        var papier = Abfrage(T02Befehle.AbfragePapier(), KennungPapier);
        if (akku is null && papier is null && _ausfaelle >= MaxAusfaelle)
        {
            Trennen();
            return;
        }

        StatusAendern(s => s with { Zustand = Verbindungszustand.Bereit, VerbindungsqualitaetProzent = _qualitaet.Prozent });
    }

    private T02Meldung? Abfrage(byte[] befehl, byte kennung)
    {
        try
        {
            var (antwort, dauer) = _verbindung!.AbfrageMitAntwort(befehl, kennung, Antwortfrist);
            if (antwort is null)
            {
                _qualitaet.Ausfall();
                _ausfaelle++;
                return null;
            }

            _qualitaet.Erfolg(dauer);
            _ausfaelle = 0;
            return antwort;
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or TimeoutException or UnauthorizedAccessException)
        {
            _qualitaet.Ausfall();
            _ausfaelle = MaxAusfaelle;
            return null;
        }
    }

    private bool Verbinden()
    {
        StatusAendern(_ => Druckerstatus.Unverbunden with { Zustand = Verbindungszustand.Verbindet });
        var drucker = Druckdienst.DruckerSuchen(_bevorzugteAdresse());
        _drucker = drucker;
        if (drucker?.ComPort is not { } port)
        {
            StatusAendern(_ => Druckerstatus.Unverbunden);
            return false;
        }

        var verbindung = new SppVerbindung(port);
        try
        {
            verbindung.Oeffnen(versuche: 2);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or TimeoutException or InvalidOperationException)
        {
            verbindung.Dispose();
            StatusAendern(_ => Druckerstatus.Unverbunden with { Name = drucker.Name, ComPort = port });
            return false;
        }

        verbindung.MeldungEmpfangen += (_, m) => MeldungAuswerten(m);
        _verbindung = verbindung;
        _ausfaelle = 0;
        _qualitaet.Zuruecksetzen();
        StatusAendern(s => s with { Zustand = Verbindungszustand.Bereit, Name = drucker.Name, ComPort = port });
        Verbunden?.Invoke(this, drucker);
        Thread.Sleep(300);
        return true;
    }

    private void Trennen()
    {
        _verbindung?.Dispose();
        _verbindung = null;
        _qualitaet.Zuruecksetzen();
        StatusAendern(s => Druckerstatus.Unverbunden with { Name = s.Name, ComPort = s.ComPort });
    }

    /// <summary>Wertet jede Meldung sofort aus, auch spontane während des Drucks.</summary>
    private void MeldungAuswerten(T02Meldung m)
    {
        switch (m.Kennung)
        {
            case KennungAkku when m.Wert <= 100:
                _akku.Hinzufuegen(m.Wert, DateTime.UtcNow);
                StatusAendern(s => s with { AkkuProzent = _akku.Wert });
                break;
            case KennungPapier:
                StatusAendern(s => s with { Papier = m.Wert == 0x88 ? Papierzustand.KeinPapier : Papierzustand.Ok });
                break;
            case KennungUeberhitzt:
                StatusAendern(s => s with { Ueberhitzt = m.Wert == 0xA9 });
                break;
        }
    }

    private void StatusAendern(Func<Druckerstatus, Druckerstatus> aenderung)
    {
        Druckerstatus neu;
        lock (_statusSperre)
        {
            neu = aenderung(_status);
            if (neu == _status)
            {
                return;
            }

            _status = neu;
        }

        StatusGeaendert?.Invoke(this, neu);
    }

    public void Dispose()
    {
        _ende.Cancel();
        try
        {
            _schleife?.Wait(TimeSpan.FromSeconds(3));
        }
        catch (AggregateException)
        {
            // Abbruch der Schleife ist erwartet.
        }

        _verbindung?.Dispose();
        _ende.Dispose();
        _zugriff.Dispose();
    }
}
