using System.IO;
using System.Text.Json;

namespace PhoMiniPrint.Core;

/// <summary>Dauerhaft gespeicherte Einstellungen (F-10, FR-007, FR-012, FR-016).</summary>
public sealed record Einstellungen
{
    public double Laenge { get; init; } = 40;
    public Raender Raender { get; init; } = Raender.Standard;
    public int StaerkeSchwarzProzent { get; init; } = 50;
    public int DruckdichteProzent { get; init; } = 50;
    public Kalibrierung Kalibrierung { get; init; } = Kalibrierung.Keine;

    /// <summary>Bluetooth-Adresse des zuletzt verwendeten Druckers.</summary>
    public string? LetzterDrucker { get; init; }

    public static Einstellungen Standard { get; } = new();

    /// <summary>True, wenn alle Werte in gültigen Grenzen liegen.</summary>
    public bool IstGueltig() =>
        Laenge is > 0 and <= 10000
        && Raender is { Links: >= 0, Rechts: >= 0, Oben: >= 0, Unten: >= 0 }
        && StaerkeSchwarzProzent is >= 0 and <= 100
        && DruckdichteProzent is >= 0 and <= 100
        && Kalibrierung is not null
        && Kalibrierrechnung.IstPlausibel(Kalibrierung.FaktorLaenge)
        && Kalibrierrechnung.IstPlausibel(Kalibrierung.FaktorHoehe);
}

/// <summary>Ergebnis des Ladens: Einstellungen und ob eine defekte Datei durch Standardwerte ersetzt wurde.</summary>
public sealed record Ladeergebnis(Einstellungen Einstellungen, bool Zurueckgesetzt);

/// <summary>Liest und schreibt <c>%APPDATA%\PhoMiniPrint\einstellungen.json</c> (F-10).</summary>
public sealed class Einstellungsspeicher(string pfad)
{
    private static readonly JsonSerializerOptions Optionen = new() { WriteIndented = true };

    /// <summary>Standardpfad im Benutzerprofil.</summary>
    public static string StandardPfad { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PhoMiniPrint", "einstellungen.json");

    public string Pfad { get; } = pfad;

    /// <summary>Fehlende Datei → Standardwerte; defekte oder ungültige Datei → Standardwerte und Hinweis.</summary>
    public Ladeergebnis Laden()
    {
        if (!File.Exists(Pfad))
        {
            return new Ladeergebnis(Einstellungen.Standard, false);
        }

        try
        {
            var einstellungen = JsonSerializer.Deserialize<Einstellungen>(File.ReadAllText(Pfad), Optionen);
            return einstellungen is not null && einstellungen.IstGueltig()
                ? new Ladeergebnis(einstellungen, false)
                : new Ladeergebnis(Einstellungen.Standard, true);
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return new Ladeergebnis(Einstellungen.Standard, true);
        }
    }

    /// <summary>Speichert über eine Zwischendatei, damit bei einem Absturz keine halbe Datei zurückbleibt.</summary>
    public bool Speichern(Einstellungen einstellungen)
    {
        ArgumentNullException.ThrowIfNull(einstellungen);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Pfad)!);
            var zwischen = Pfad + ".neu";
            File.WriteAllText(zwischen, JsonSerializer.Serialize(einstellungen, Optionen));
            File.Move(zwischen, Pfad, overwrite: true);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }
}
