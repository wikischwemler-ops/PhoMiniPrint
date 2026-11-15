using PhoMiniPrint.Printer;

namespace PhoMiniPrint.Core;

/// <summary>Maßrechnung, Einpassen und Prüfungen (FR-006, FR-008, FR-016, FR-017, FR-019, BR-001, BR-002).</summary>
public static class Layout
{
    /// <summary>Kleinste Einheit: ein Druckerpunkt = 1/8 mm.</summary>
    public const double PunktMm = 1.0 / T02Geraet.PunkteProMm;

    /// <summary>mm → Druckerpunkte, gerundet (BR-002).</summary>
    public static int MmZuPunkten(double mm, double faktor = 1.0) =>
        (int)Math.Round(mm * T02Geraet.PunkteProMm * faktor, MidpointRounding.AwayFromZero);

    /// <summary>Rundet einen mm-Wert auf ganze Druckerpunkte, damit Ansicht und Druck übereinstimmen.</summary>
    public static double AufPunktRunden(double mm) => Math.Round(mm / PunktMm) * PunktMm;

    /// <summary>Rundet auf den nächsten ganzen Druckerpunkt auf (z. B. Textbreite, damit nichts umbricht).</summary>
    public static double AufPunktAufrunden(double mm) => Math.Ceiling((mm / PunktMm) - 1e-9) * PunktMm;

    /// <summary>Seitenverhältnis des Elements auf dem Etikett nach der Drehung.</summary>
    public static double SeitenverhaeltnisGedreht(double verhaeltnis, Drehung drehung) =>
        drehung is Drehung.Grad90 or Drehung.Grad270 ? 1.0 / verhaeltnis : verhaeltnis;

    /// <summary>
    /// Seitenverhältnis Breite ÷ Höhe eines Elements vor der Drehung: bei Bildern aus den Pixeln,
    /// bei Text aus dem (bemessenen) Kasten.
    /// </summary>
    public static double Grundverhaeltnis(Element element)
    {
        ArgumentNullException.ThrowIfNull(element);
        if (element is BildElement bild)
        {
            return bild.Seitenverhaeltnis;
        }

        var quer = element.Drehung is Drehung.Grad90 or Drehung.Grad270;
        return quer ? element.Hoehe / element.Breite : element.Breite / element.Hoehe;
    }

    /// <summary>
    /// Passt ein Element so groß wie möglich und zentriert in den Druckbereich ein,
    /// ohne es zu verzerren (FR-019, Q-A2d). Text muss vorher bemessen sein.
    /// </summary>
    public static void MaximalEinpassen(Etikett etikett, Element bild)
    {
        ArgumentNullException.ThrowIfNull(etikett);
        ArgumentNullException.ThrowIfNull(bild);
        var (x, y, breite, hoehe) = etikett.Druckbereich;
        var v = SeitenverhaeltnisGedreht(Grundverhaeltnis(bild), bild.Drehung);

        var b = breite;
        var h = b / v;
        if (h > hoehe)
        {
            h = hoehe;
            b = h * v;
        }

        bild.Breite = AufPunktRunden(b);
        bild.Hoehe = AufPunktRunden(h);
        bild.X = AufPunktRunden(x + ((breite - bild.Breite) / 2));
        bild.Y = AufPunktRunden(y + ((hoehe - bild.Hoehe) / 2));
    }

    /// <summary>
    /// Dreht ein Element um seinen Mittelpunkt (FR-017).
    /// Ein Bild wird auf die vorgegebene Größe skaliert: Es füllt die bisherige Fläche so weit wie möglich,
    /// ohne verzerrt zu werden. Text behält seine Schriftgröße; Breite und Höhe tauschen bei 90°/270°.
    /// </summary>
    public static void Drehen(Element bild, Drehung neu)
    {
        ArgumentNullException.ThrowIfNull(bild);
        var mitteX = bild.X + (bild.Breite / 2);
        var mitteY = bild.Y + (bild.Hoehe / 2);
        var flaecheB = bild.Breite;
        var flaecheH = bild.Hoehe;
        var grund = Grundverhaeltnis(bild);
        var warQuer = bild.Drehung is Drehung.Grad90 or Drehung.Grad270;
        var wirdQuer = neu is Drehung.Grad90 or Drehung.Grad270;
        if (bild is not BildElement && warQuer != wirdQuer)
        {
            (flaecheB, flaecheH) = (flaecheH, flaecheB);
        }

        bild.Drehung = neu;
        var v = SeitenverhaeltnisGedreht(grund, neu);
        var b = flaecheB;
        var h = b / v;
        if (h > flaecheH)
        {
            h = flaecheH;
            b = h * v;
        }

        bild.Breite = AufPunktRunden(b);
        bild.Hoehe = AufPunktRunden(h);
        bild.X = AufPunktRunden(mitteX - (bild.Breite / 2));
        bild.Y = AufPunktRunden(mitteY - (bild.Hoehe / 2));
    }

    /// <summary>
    /// Prüft das Etikett vor dem Druck (FR-008, FR-016). Liefert deutsche Meldungen; leer = druckbar.
    /// </summary>
    public static IReadOnlyList<string> Pruefen(Etikett etikett)
    {
        ArgumentNullException.ThrowIfNull(etikett);
        var fehler = new List<string>();
        var r = etikett.Raender;

        if (etikett.Laenge <= 0)
        {
            fehler.Add("Die Länge muss größer als 0 mm sein.");
        }

        if (r.Links < 0 || r.Rechts < 0 || r.Oben < 0 || r.Unten < 0)
        {
            fehler.Add("Ränder dürfen nicht negativ sein.");
        }

        if (r.Links + r.Rechts >= etikett.Laenge)
        {
            fehler.Add("Die Ränder links und rechts sind größer als das Etikett.");
        }

        if (r.Oben + r.Unten >= Etikett.BandhoeheMm)
        {
            fehler.Add("Die Ränder oben und unten sind größer als das Band.");
        }

        if (r.Oben < Etikett.UnbedruckbarMm - 1e-9 || r.Unten < Etikett.UnbedruckbarMm - 1e-9)
        {
            fehler.Add($"Rand oben und unten muss mindestens {Etikett.UnbedruckbarMm:0.#} mm betragen – dort druckt der T02 nicht.");
        }

        var (x, y, breite, hoehe) = etikett.Druckbereich;
        for (var i = 0; i < etikett.Elemente.Count; i++)
        {
            var e = etikett.Elemente[i];
            const double toleranz = 1e-6;
            if (e.X < x - toleranz || e.Y < y - toleranz || e.Rechts > x + breite + toleranz || e.Unten > y + hoehe + toleranz)
            {
                fehler.Add($"Element {i + 1} ragt über den Rand.");
            }
        }

        return fehler;
    }
}
