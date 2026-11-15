using PhoMiniPrint.Printer;

namespace PhoMiniPrint.Core;

/// <summary>Drehwinkel eines Elements (FR-017).</summary>
public enum Drehung
{
    Grad0 = 0,
    Grad90 = 90,
    Grad180 = 180,
    Grad270 = 270,
}

/// <summary>Ausrichtung eines Textes (FR-010).</summary>
public enum Textausrichtung
{
    Links,
    Mittig,
    Rechts,
}

/// <summary>
/// Ränder in mm (FR-016). Links/rechts beziehen sich auf die Länge (Anfang/Ende des Etiketts),
/// oben/unten auf die Höhe quer zum Band, jeweils gemessen ab der Papierkante.
/// </summary>
public sealed record Raender(double Links, double Rechts, double Oben, double Unten)
{
    /// <summary>Startwerte (Planungsannahme): je 2 mm.</summary>
    public static Raender Standard { get; } = new(2, 2, 2, 2);
}

/// <summary>Korrekturfaktoren aus der Kalibrierung (FR-012). 1,0 = keine Korrektur.</summary>
public sealed record Kalibrierung(double FaktorLaenge, double FaktorHoehe)
{
    public static Kalibrierung Keine { get; } = new(1.0, 1.0);
}

/// <summary>
/// Ein Element auf dem Etikett. Koordinaten in mm im Etikett-Raum:
/// X entlang des Bandes (0 = Anfang), Y quer zum Band (0 = obere Papierkante).
/// Breite/Hoehe beschreiben den Platz auf dem Etikett nach der Drehung.
/// </summary>
public abstract class Element
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Breite { get; set; }
    public double Hoehe { get; set; }
    public Drehung Drehung { get; set; } = Drehung.Grad0;

    public double Rechts => X + Breite;
    public double Unten => Y + Hoehe;
}

/// <summary>Bildelement (FR-004, FR-005).</summary>
public sealed class BildElement : Element
{
    public BildElement(System.Windows.Media.Imaging.BitmapSource quelle)
    {
        ArgumentNullException.ThrowIfNull(quelle);
        if (!quelle.IsFrozen && quelle.CanFreeze)
        {
            // Eingefroren, damit Rendern auch auf einem anderen Thread möglich ist.
            quelle.Freeze();
        }

        Quelle = quelle;
    }

    public System.Windows.Media.Imaging.BitmapSource Quelle { get; }

    /// <summary>Seitenverhältnis Breite ÷ Höhe des Bildes vor der Drehung.</summary>
    public double Seitenverhaeltnis => (double)Quelle.PixelWidth / Quelle.PixelHeight;
}

/// <summary>Textelement mit Symbolen (FR-009, FR-010).</summary>
public sealed class TextElement : Element
{
    public string Text { get; set; } = string.Empty;
    public string Schriftart { get; set; } = "Segoe UI";
    public double GroessePt { get; set; } = 14;
    public bool Fett { get; set; }
    public Textausrichtung Ausrichtung { get; set; } = Textausrichtung.Links;
}

/// <summary>Das Etikett: Länge, Ränder und Elemente (FR-006, FR-013, FR-016).</summary>
public sealed class Etikett
{
    /// <summary>Höhe des Papierbandes in mm (gemessen in M1).</summary>
    public const double BandhoeheMm = 50.0;

    /// <summary>Bedruckbare Höhe in mm (384 Punkte ÷ 8).</summary>
    public const double DruckhoeheMm = (double)T02Geraet.DruckbreitePunkte / T02Geraet.PunkteProMm;

    /// <summary>
    /// Nicht bedruckbarer Streifen oben und unten in mm. Annahme: Druckkopf mittig auf dem Band
    /// (in der Abnahme zu prüfen).
    /// </summary>
    public const double UnbedruckbarMm = (BandhoeheMm - DruckhoeheMm) / 2;

    public double Laenge { get; set; } = 40;
    public Raender Raender { get; set; } = Raender.Standard;
    public List<Element> Elemente { get; } = [];

    /// <summary>Bereich innerhalb der Ränder (Etikett-Raum, mm).</summary>
    public (double X, double Y, double Breite, double Hoehe) Druckbereich =>
        (Raender.Links, Raender.Oben, Laenge - Raender.Links - Raender.Rechts, BandhoeheMm - Raender.Oben - Raender.Unten);
}
