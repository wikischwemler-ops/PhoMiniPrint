namespace PhoMiniPrint.Core;

/// <summary>Ecke eines Elements, an der mit der Maus gezogen wird (F-05).</summary>
public enum Ecke
{
    ObenLinks,
    ObenRechts,
    UntenLinks,
    UntenRechts,
}

/// <summary>
/// Verschieben und Größenändern von Elementen mit der Maus oder über Eingabefelder (F-05, FR-019).
/// Das Seitenverhältnis bleibt immer erhalten, Elemente bleiben im Druckbereich,
/// alle Werte liegen auf ganzen Druckerpunkten.
/// </summary>
public static class Bearbeitung
{
    /// <summary>Kleinste Kantenlänge eines Elements in mm.</summary>
    public const double MindestgroesseMm = 2.0;

    /// <summary>Setzt die linke obere Ecke; das Element bleibt vollständig im Druckbereich.</summary>
    public static void Verschieben(Element element, double x, double y, Etikett etikett)
    {
        ArgumentNullException.ThrowIfNull(element);
        ArgumentNullException.ThrowIfNull(etikett);
        var (bx, by, bb, bh) = etikett.Druckbereich;
        element.X = Begrenzen(x, bx, bx + bb - element.Breite);
        element.Y = Begrenzen(y, by, by + bh - element.Hoehe);
    }

    /// <summary>
    /// Zieht die angegebene Ecke zum Punkt (x, y) in mm. Die gegenüberliegende Ecke bleibt fest,
    /// das Seitenverhältnis <paramref name="verhaeltnis"/> (Breite ÷ Höhe) bleibt erhalten.
    /// </summary>
    public static void GroesseAendern(Element element, Ecke ecke, double x, double y, Etikett etikett, double? verhaeltnis = null)
    {
        ArgumentNullException.ThrowIfNull(element);
        ArgumentNullException.ThrowIfNull(etikett);
        var v = verhaeltnis ?? (element.Breite / element.Hoehe);
        var links = ecke is Ecke.ObenLinks or Ecke.UntenLinks;
        var oben = ecke is Ecke.ObenLinks or Ecke.ObenRechts;

        var festX = links ? element.Rechts : element.X;
        var festY = oben ? element.Unten : element.Y;

        var b = Math.Abs(x - festX);
        var h = Math.Abs(y - festY);
        if (b / v > h)
        {
            h = b / v;
        }
        else
        {
            b = h * v;
        }

        var (bx, by, bb, bh) = etikett.Druckbereich;
        var maxB = links ? festX - bx : bx + bb - festX;
        var maxH = oben ? festY - by : by + bh - festY;
        (b, h) = Einpassen(b, h, v, maxB, maxH);

        element.Breite = b;
        element.Hoehe = h;
        element.X = Layout.AufPunktRunden(links ? festX - b : festX);
        element.Y = Layout.AufPunktRunden(oben ? festY - h : festY);
    }

    /// <summary>
    /// Setzt Breite oder Höhe exakt (Eingabefeld); der andere Wert folgt dem Seitenverhältnis.
    /// Die linke obere Ecke bleibt, solange das Element in den Druckbereich passt.
    /// </summary>
    public static void GroesseSetzen(Element element, double? breite, double? hoehe, Etikett etikett)
    {
        ArgumentNullException.ThrowIfNull(element);
        ArgumentNullException.ThrowIfNull(etikett);
        var v = element.Breite / element.Hoehe;
        double b, h;
        if (breite is { } wb)
        {
            b = wb;
            h = wb / v;
        }
        else if (hoehe is { } wh)
        {
            h = wh;
            b = wh * v;
        }
        else
        {
            return;
        }

        var (_, _, bb, bh) = etikett.Druckbereich;
        (b, h) = Einpassen(b, h, v, bb, bh);
        element.Breite = b;
        element.Hoehe = h;
        Verschieben(element, element.X, element.Y, etikett);
    }

    /// <summary>Begrenzt die Größe auf den verfügbaren Platz und die Mindestgröße, abgerundet auf Druckerpunkte.</summary>
    private static (double B, double H) Einpassen(double b, double h, double v, double maxB, double maxH)
    {
        if (b > maxB)
        {
            b = maxB;
            h = b / v;
        }

        if (h > maxH)
        {
            h = maxH;
            b = h * v;
        }

        var kleinste = Math.Min(b, h);
        if (kleinste < MindestgroesseMm && kleinste > 0)
        {
            var faktor = Math.Min(MindestgroesseMm / kleinste, Math.Min(maxB / b, maxH / h));
            b *= faktor;
            h *= faktor;
        }

        return (Abrunden(b), Abrunden(h));
    }

    private static double Abrunden(double mm) =>
        Math.Max(Layout.PunktMm, Math.Floor((mm / Layout.PunktMm) + 1e-6) * Layout.PunktMm);

    private static double Begrenzen(double wert, double min, double max) =>
        Layout.AufPunktRunden(Math.Clamp(wert, min, Math.Max(min, max)));
}
