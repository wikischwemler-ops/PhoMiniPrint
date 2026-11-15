using PhoMiniPrint.Printer;

namespace PhoMiniPrint.Core;

/// <summary>Testseite und Berechnung der Kalibrierfaktoren (F-09, FR-012).</summary>
public static class Kalibrierrechnung
{
    /// <summary>Sollmaß des Testrahmens (Außenkante) in mm.</summary>
    public const double SollLaengeMm = 80;

    /// <summary>Sollmaß des Testrahmens quer zum Band in mm.</summary>
    public const double SollHoeheMm = 40;

    /// <summary>Freiraum vor und nach dem Rahmen in mm.</summary>
    public const double VorlaufMm = 2;

    /// <summary>Länge der Testseite in mm (ohne Nachlauf).</summary>
    public const double TestseiteLaengeMm = SollLaengeMm + (2 * VorlaufMm);

    /// <summary>Größte erlaubte Abweichung eines Faktors von 1,0.</summary>
    public const double MaxAbweichung = 0.10;

    public const string MeldungUnplausibel = "Bitte nachmessen. Der Wert weicht mehr als 10 % ab.";

    public static bool IstPlausibel(double faktor) => Math.Abs(faktor - 1.0) <= MaxAbweichung + 1e-9;

    /// <summary>
    /// Neue Faktoren aus den gemessenen Außenmaßen. Die Testseite wurde mit den bisherigen Faktoren gedruckt,
    /// daher: neu = bisher × Soll ÷ Ist. Liefert eine deutsche Fehlermeldung, wenn ein Messwert unplausibel ist.
    /// </summary>
    public static (Kalibrierung? Ergebnis, string? Fehler) Berechnen(Kalibrierung bisher, double istLaengeMm, double istHoeheMm)
    {
        ArgumentNullException.ThrowIfNull(bisher);
        if (istLaengeMm <= 0 || istHoeheMm <= 0)
        {
            return (null, "Bitte beide Maße in mm eingeben.");
        }

        var korrekturL = SollLaengeMm / istLaengeMm;
        var korrekturH = SollHoeheMm / istHoeheMm;
        if (!IstPlausibel(korrekturL) || !IstPlausibel(korrekturH))
        {
            return (null, MeldungUnplausibel);
        }

        var neu = new Kalibrierung(Math.Round(bisher.FaktorLaenge * korrekturL, 4), Math.Round(bisher.FaktorHoehe * korrekturH, 4));
        if (!IstPlausibel(neu.FaktorLaenge) || !IstPlausibel(neu.FaktorHoehe))
        {
            return (null, MeldungUnplausibel);
        }

        return (neu, null);
    }

    /// <summary>
    /// Testseite: Rahmen 80 × 40 mm (Strich 2 Punkte, außen gemessen) mit Teilstrichen alle 10 mm
    /// und der Aufschrift der Sollmaße; mit den angegebenen Faktoren gerastert.
    /// </summary>
    public static Rasterbild Testseite(Kalibrierung kalibrierung)
    {
        ArgumentNullException.ThrowIfNull(kalibrierung);
        var etikett = new Etikett { Laenge = TestseiteLaengeMm, Raender = new Raender(0, 0, Etikett.UnbedruckbarMm, Etikett.UnbedruckbarMm) };
        var text = new TextElement { Text = "80 × 40 mm\naußen messen", GroessePt = 16, Fett = true, Ausrichtung = Textausrichtung.Mittig };
        Renderer.TextBemessen(text);
        text.X = Layout.AufPunktRunden((TestseiteLaengeMm - text.Breite) / 2);
        text.Y = Layout.AufPunktRunden((Etikett.BandhoeheMm - text.Hoehe) / 2);
        etikett.Elemente.Add(text);

        var bild = Renderer.Rastern(etikett, 50, kalibrierung);

        var hoehe = Layout.MmZuPunkten(SollHoeheMm, kalibrierung.FaktorHoehe);
        var laenge = Layout.MmZuPunkten(SollLaengeMm, kalibrierung.FaktorLaenge);
        var spalte0 = (T02Geraet.DruckbreitePunkte - hoehe) / 2;
        var zeile0 = Layout.MmZuPunkten(VorlaufMm, kalibrierung.FaktorLaenge);
        const int strich = 2;
        bild.Rahmen(spalte0, zeile0, hoehe, laenge, strich);

        // Teilstriche alle 10 mm an der oberen Innenkante (hohe Spalten), 5 mm lang.
        var teil = Layout.MmZuPunkten(5, kalibrierung.FaktorHoehe);
        for (var mm = 10; mm < SollLaengeMm; mm += 10)
        {
            var zeile = zeile0 + Layout.MmZuPunkten(mm, kalibrierung.FaktorLaenge) - 1;
            bild.Rechteck(spalte0 + hoehe - strich - teil, zeile, teil, 2);
        }

        return bild;
    }
}
