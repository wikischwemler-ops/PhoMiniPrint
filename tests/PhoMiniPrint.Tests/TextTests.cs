using System.Windows.Media;
using System.Windows.Media.Imaging;
using PhoMiniPrint.Core;
using PhoMiniPrint.Printer;

namespace PhoMiniPrint.Tests;

public class TextTests
{
    /// <summary>Abnahmetext aus F-06 / M4.</summary>
    private const string Abnahmetext = "° ² € § ± ✓ ✗ ★ ♥ → ☎ ⚠ 😀 👍";

    private static int Schwarz(Rasterbild r)
    {
        var n = 0;
        for (var z = 0; z < r.Hoehe; z++)
        {
            for (var s = 0; s < r.Breite; s++)
            {
                if (r[s, z])
                {
                    n++;
                }
            }
        }

        return n;
    }

    [Fact]
    public void Abnahmetext_ist_vollstaendig_druckbar()
    {
        Assert.Empty(Zeichenpruefung.NichtDruckbar(Abnahmetext, "Segoe UI"));
        Assert.Empty(Zeichenpruefung.NichtDruckbar(Abnahmetext, "Arial"));
    }

    [Fact]
    public void Alle_Symbole_der_Auswahl_sind_druckbar()
    {
        Assert.Empty(Zeichenpruefung.NichtDruckbar(string.Concat(Zeichenpruefung.Symbole), "Segoe UI"));
    }

    [Fact]
    public void Zeichen_ohne_Schrift_wird_gemeldet()
    {
        // Privatnutzungsbereich: keine Schrift enthält dieses Zeichen.
        var fehlend = Zeichenpruefung.NichtDruckbar("A\U0010FFFDB", "Segoe UI");

        Assert.Equal(["\U0010FFFD"], fehlend);
    }

    [Fact]
    public void Abnahmetext_wird_jedes_Zeichen_gedruckt()
    {
        // Jedes Zeichen einzeln: Ein leeres Kästchen oder fehlendes Zeichen fiele durch wenige schwarze Punkte auf.
        foreach (var zeichen in Abnahmetext.Split(' '))
        {
            var text = new TextElement { Text = zeichen, GroessePt = 36, X = 2, Y = 3 };
            Renderer.TextBemessen(text);
            var etikett = new Etikett { Laenge = 40, Elemente = { text } };

            Assert.True(Schwarz(Renderer.Rastern(etikett, 50)) > 150, $"„{zeichen}“ erzeugt kaum schwarze Punkte.");
        }
    }

    [Fact]
    public void Groesserer_Kasten_vergroessert_die_Schrift_unverzerrt()
    {
        var etikett = new Etikett { Laenge = 80 };
        var text = new TextElement { Text = "Test ★", GroessePt = 12, X = 5, Y = 5 };
        Renderer.TextBemessen(text);
        var hoehe12 = text.Hoehe;
        etikett.Elemente.Add(text);
        var schwarz12 = Schwarz(Renderer.Rastern(etikett, 50));

        Bearbeitung.GroesseAendern(text, Ecke.UntenRechts, text.X + (text.Breite * 2), text.Y + (text.Hoehe * 2), etikett);
        Renderer.SchriftgroesseAusKasten(text);

        Assert.Equal(hoehe12 * 2, text.Hoehe, 0);
        Assert.InRange(text.GroessePt, 23, 25);
        Assert.InRange(Schwarz(Renderer.Rastern(etikett, 50)), schwarz12 * 3, schwarz12 * 5);
    }

    [Fact]
    public void Text_drehen_tauscht_Breite_und_Hoehe_und_behaelt_die_Schrift()
    {
        var text = new TextElement { Text = "Haltbar", GroessePt = 14, X = 20, Y = 10 };
        Renderer.TextBemessen(text);
        var (b, h) = (text.Breite, text.Hoehe);

        Layout.Drehen(text, Drehung.Grad90);
        Renderer.SchriftgroesseAusKasten(text);

        Assert.Equal(h, text.Breite, 1);
        Assert.Equal(b, text.Hoehe, 1);
        Assert.InRange(text.GroessePt, 13.5, 14.5);
        Assert.Empty(Layout.Pruefen(new Etikett { Laenge = 80, Elemente = { text } }));
    }

    [Fact]
    public void Ausrichtung_mittig_zentriert_kurze_Zeilen()
    {
        var links = new TextElement { Text = "WWWWWWWW\nI", GroessePt = 20, X = 2, Y = 3 };
        var mittig = new TextElement { Text = "WWWWWWWW\nI", GroessePt = 20, X = 2, Y = 3, Ausrichtung = Textausrichtung.Mittig };
        Renderer.TextBemessen(links);
        Renderer.TextBemessen(mittig);

        // Zweite Zeile („I“): bei links nahe am Anfang, bei mittig etwa in der Mitte der Länge.
        var anfangLinks = ErsteSchwarzeZeileUnten(Renderer.Rastern(new Etikett { Laenge = 60, Elemente = { links } }, 50), links);
        var anfangMittig = ErsteSchwarzeZeileUnten(Renderer.Rastern(new Etikett { Laenge = 60, Elemente = { mittig } }, 50), mittig);

        Assert.True(anfangMittig > anfangLinks + (Layout.MmZuPunkten(mittig.Breite) / 3), $"links {anfangLinks}, mittig {anfangMittig}");
    }

    [Fact]
    public void Bild_und_Text_auf_einem_Etikett()
    {
        var pixel = new byte[20 * 20 * 4];
        for (var i = 3; i < pixel.Length; i += 4)
        {
            pixel[i] = 255;
        }

        var bild = new BildElement(BitmapSource.Create(20, 20, 96, 96, PixelFormats.Bgra32, null, pixel, 80))
        {
            X = 2, Y = 2, Breite = 20, Hoehe = 20,
        };
        var text = new TextElement { Text = "Gewürz ★", GroessePt = 16, X = 25, Y = 10 };
        Renderer.TextBemessen(text);
        var etikett = new Etikett { Laenge = 80, Elemente = { bild, text } };

        var daten = Druckvorbereitung.Erstellen(etikett, Druckeinstellungen.Standard);

        Assert.True(Schwarz(daten.Bild) > (160 * 160) + 200);
    }

    /// <summary>Kleinste Druckzeile (Länge) mit schwarzen Punkten in der unteren Hälfte des Textkastens.</summary>
    private static int ErsteSchwarzeZeileUnten(Rasterbild r, TextElement t)
    {
        // Spalte 383 = obere Kante; untere Hälfte des Kastens = kleinere Spalten.
        var obenSpalte = r.Breite - 1 - Layout.MmZuPunkten(t.Y + (t.Hoehe / 2) - Etikett.UnbedruckbarMm);
        var untenSpalte = r.Breite - 1 - Layout.MmZuPunkten(t.Unten - Etikett.UnbedruckbarMm);
        for (var z = 0; z < r.Hoehe; z++)
        {
            for (var s = Math.Max(0, untenSpalte); s <= obenSpalte; s++)
            {
                if (r[s, z])
                {
                    return z;
                }
            }
        }

        return -1;
    }
}
