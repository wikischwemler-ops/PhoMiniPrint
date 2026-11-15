namespace PhoMiniPrint.Core;

/// <summary>Verbindungszustand zum T02 (F-02).</summary>
public enum Verbindungszustand
{
    NichtVerbunden,
    Verbindet,
    Bereit,
    Druckt,
}

/// <summary>Papierzustand. Der T02 unterscheidet „kein Papier“ und „Deckel offen“ nicht (M1, E-2).</summary>
public enum Papierzustand
{
    Unbekannt,
    Ok,
    KeinPapier,
}

/// <summary>Momentaufnahme des Druckerstatus für die Statusleiste (F-02, FR-003, FR-020, FR-021).</summary>
public sealed record Druckerstatus(
    Verbindungszustand Zustand,
    string? Name,
    string? ComPort,
    int? AkkuProzent,
    Papierzustand Papier,
    bool Ueberhitzt,
    int? VerbindungsqualitaetProzent)
{
    public static Druckerstatus Unverbunden { get; } =
        new(Verbindungszustand.NichtVerbunden, null, null, null, Papierzustand.Unbekannt, false, null);

    /// <summary>Darf jetzt gedruckt werden (kein gemeldetes Papierproblem, nicht überhitzt)?</summary>
    public bool DruckErlaubt => Papier != Papierzustand.KeinPapier && !Ueberhitzt;
}

/// <summary>
/// Glättet den Akkustand (E-2): Beim Drucken bricht die Spannung ein und der T02 meldet kurzzeitig
/// deutlich weniger (M1: 34 % direkt nach dem Druck, eine Minute später 63 %).
/// Werte kurz nach einem Druck werden verworfen; angezeigt wird der höchste Wert der letzten Minuten,
/// damit kurze Einbrüche nicht durchschlagen, echtes Entladen aber nach dem Zeitfenster sichtbar wird.
/// </summary>
public sealed class Akkuglaettung
{
    /// <summary>So lange nach Druckende wird der Akkuwert ignoriert.</summary>
    public static readonly TimeSpan Erholzeit = TimeSpan.FromSeconds(90);

    /// <summary>Zeitfenster für den Höchstwert.</summary>
    public static readonly TimeSpan Fenster = TimeSpan.FromMinutes(3);

    private readonly List<(DateTime Zeit, int Wert)> _werte = [];
    private DateTime _letztesDruckende = DateTime.MinValue;

    /// <summary>Geglätteter Wert; null, solange keiner vorliegt.</summary>
    public int? Wert { get; private set; }

    public void DruckBeendet(DateTime zeitpunkt) => _letztesDruckende = zeitpunkt;

    public void Hinzufuegen(int prozent, DateTime zeitpunkt)
    {
        prozent = Math.Clamp(prozent, 0, 100);
        var kurzNachDruck = zeitpunkt - _letztesDruckende < Erholzeit;
        if (kurzNachDruck && Wert is not null)
        {
            return;
        }

        _werte.Add((zeitpunkt, prozent));
        _werte.RemoveAll(w => zeitpunkt - w.Zeit > Fenster);
        Wert = _werte.Max(w => w.Wert);
    }
}

/// <summary>
/// Verbindungsqualität (E-1) aus den letzten Statusabfragen: Ausfälle zählen 0 %,
/// Antworten bis 150 ms 100 %, langsamere linear abnehmend bis 20 % bei 1,5 s.
/// </summary>
public sealed class Verbindungsqualitaet
{
    public const int AnzahlAbfragen = 10;
    private static readonly TimeSpan Schnell = TimeSpan.FromMilliseconds(150);
    private static readonly TimeSpan Langsam = TimeSpan.FromMilliseconds(1500);

    private readonly Queue<double> _bewertungen = new();

    /// <summary>Qualität 0–100 %; null ohne Messung.</summary>
    public int? Prozent => _bewertungen.Count == 0 ? null : (int)Math.Round(_bewertungen.Average());

    public void Erfolg(TimeSpan antwortzeit) => Hinzufuegen(Bewerten(antwortzeit));

    public void Ausfall() => Hinzufuegen(0);

    public void Zuruecksetzen() => _bewertungen.Clear();

    public static double Bewerten(TimeSpan antwortzeit)
    {
        if (antwortzeit <= Schnell)
        {
            return 100;
        }

        if (antwortzeit >= Langsam)
        {
            return 20;
        }

        var anteil = (antwortzeit - Schnell) / (Langsam - Schnell);
        return 100 - (80 * anteil);
    }

    private void Hinzufuegen(double bewertung)
    {
        _bewertungen.Enqueue(bewertung);
        while (_bewertungen.Count > AnzahlAbfragen)
        {
            _bewertungen.Dequeue();
        }
    }
}
