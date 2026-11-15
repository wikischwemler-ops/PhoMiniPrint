using System.IO;
using PhoMiniPrint.Printer;

namespace PhoMiniPrint.Core;

/// <summary>Abschnitt eines Druckvorgangs für Fortschritt und Animation (FR-022).</summary>
public enum Druckphase
{
    Verbinden,
    Senden,
    Drucken,
    Fertig,
}

/// <summary>Fortschritt: Phase und Anteil 0–1 innerhalb der Phase.</summary>
public sealed record Druckfortschritt(Druckphase Phase, double Anteil);

/// <summary>Ergebnis eines Druckvorgangs mit deutscher Meldung.</summary>
public sealed record Druckergebnis(bool Erfolgreich, string Meldung);

/// <summary>
/// Sendet einen fertigen Druckauftrag an den gekoppelten T02 (F-08).
/// Blockiert; aus der Oberfläche in einem Hintergrund-Task aufrufen.
/// </summary>
public static class Druckdienst
{
    /// <summary>Kennung und Wert der Meldung „kein Papier“ (M1: 1A 06 88; auch bei offenem Deckel).</summary>
    private const byte KennungPapier = 0x06;
    private const byte WertKeinPapier = 0x88;

    /// <summary>Sucht den ersten gekoppelten T02 mit COM-Port; null, wenn keiner gefunden wurde.</summary>
    public static T02Fundstelle? DruckerSuchen()
    {
        try
        {
            return T02Suche.Suchen().FirstOrDefault(f => f.ComPort is not null);
        }
        catch (Exception ex) when (ex is System.Management.ManagementException or System.Runtime.InteropServices.COMException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    public static Druckergebnis Drucken(Druckdaten daten, IProgress<Druckfortschritt>? fortschritt = null)
    {
        ArgumentNullException.ThrowIfNull(daten);
        fortschritt?.Report(new Druckfortschritt(Druckphase.Verbinden, 0));

        var drucker = DruckerSuchen();
        if (drucker?.ComPort is not { } port)
        {
            return new Druckergebnis(false, "Kein gekoppelter T02 gefunden. Bitte den Drucker einschalten und in Windows koppeln.");
        }

        using var verbindung = new SppVerbindung(port);
        try
        {
            verbindung.Oeffnen();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or TimeoutException or InvalidOperationException)
        {
            return new Druckergebnis(false, $"Keine Verbindung zum {drucker.Name} ({port}). Ist der Drucker eingeschaltet?");
        }

        try
        {
            Thread.Sleep(300);
            var (meldungen, _) = verbindung.Abfragen(T02Befehle.AbfragePapier(), TimeSpan.FromMilliseconds(400));
            if (meldungen.Any(m => m.Kennung == KennungPapier && m.Wert == WertKeinPapier))
            {
                return new Druckergebnis(false, "Kein Papier oder Deckel offen.");
            }

            var ab = verbindung.AnzahlMeldungen;
            fortschritt?.Report(new Druckfortschritt(Druckphase.Senden, 0));
            verbindung.Senden(daten.Auftrag, new Fortschrittsweiterleitung(fortschritt));

            fortschritt?.Report(new Druckfortschritt(Druckphase.Drucken, 0));
            if (!verbindung.AufFertigmeldungWarten(ab, Druckfrist(daten.Zeilen)))
            {
                return new Druckergebnis(false, "Der Drucker hat das Druckende nicht gemeldet. Papierstau?");
            }

            fortschritt?.Report(new Druckfortschritt(Druckphase.Fertig, 1));
            return new Druckergebnis(true, $"Gedruckt: {daten.Bild.Hoehe / (double)T02Geraet.PunkteProMm:0.#} mm.");
        }
        catch (Exception ex) when (ex is IOException or TimeoutException or InvalidOperationException or UnauthorizedAccessException)
        {
            return new Druckergebnis(false, "Die Verbindung zum Drucker wurde unterbrochen.");
        }
    }

    /// <summary>Wartezeit auf „Druck fertig“: Druckzeit (Schätzung 0,11 s/mm) plus 10 s Zuschlag.</summary>
    public static TimeSpan Druckfrist(int zeilen) =>
        TimeSpan.FromSeconds((zeilen / (double)T02Geraet.PunkteProMm * 0.11) + 10);

    /// <summary>Wandelt den Sendefortschritt (0–1) in einen <see cref="Druckfortschritt"/> um.</summary>
    private sealed class Fortschrittsweiterleitung(IProgress<Druckfortschritt>? ziel) : IProgress<double>
    {
        public void Report(double value) => ziel?.Report(new Druckfortschritt(Druckphase.Senden, value));
    }
}
