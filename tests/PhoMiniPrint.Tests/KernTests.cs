using System.Windows.Media;
using System.Windows.Media.Imaging;
using PhoMiniPrint.Core;
using PhoMiniPrint.Printer;

namespace PhoMiniPrint.Tests;

public class KernTests
{
    private static BitmapSource Einfarbig(int breite, int hoehe, Color farbe)
    {
        var pixel = new byte[breite * hoehe * 4];
        for (var i = 0; i < pixel.Length; i += 4)
        {
            pixel[i] = farbe.B;
            pixel[i + 1] = farbe.G;
            pixel[i + 2] = farbe.R;
            pixel[i + 3] = 255;
        }

        var bild = BitmapSource.Create(breite, hoehe, 96, 96, PixelFormats.Bgra32, null, pixel, breite * 4);
        bild.Freeze();
        return bild;
    }

    private static Etikett EtikettMit(double laenge, Raender raender, params Element[] elemente)
    {
        var e = new Etikett { Laenge = laenge, Raender = raender };
        e.Elemente.AddRange(elemente);
        return e;
    }

    private static (int MinZeile, int MaxZeile, int MinSpalte, int MaxSpalte, int Anzahl) Schwarzbereich(Rasterbild r)
    {
        int minZ = int.MaxValue, maxZ = -1, minS = int.MaxValue, maxS = -1, n = 0;
        for (var z = 0; z < r.Hoehe; z++)
        {
            for (var s = 0; s < r.Breite; s++)
            {
                if (!r[s, z])
                {
                    continue;
                }

                n++;
                minZ = Math.Min(minZ, z);
                maxZ = Math.Max(maxZ, z);
                minS = Math.Min(minS, s);
                maxS = Math.Max(maxS, s);
            }
        }

        return (minZ, maxZ, minS, maxS, n);
    }

    [Theory]
    [InlineData(40.0, 1.0, 320)]
    [InlineData(2.0, 1.0, 16)]
    [InlineData(48.0, 1.0, 384)]
    [InlineData(80.0, 1.01, 646)]
    public void Millimeter_werden_in_Druckerpunkte_umgerechnet(double mm, double faktor, int punkte)
    {
        Assert.Equal(punkte, Layout.MmZuPunkten(mm, faktor));
    }

    [Fact]
    public void Bild_wird_maximal_und_unverzerrt_eingepasst()
    {
        var etikett = EtikettMit(80, Raender.Standard);
        var bild = new BildElement(Einfarbig(200, 100, Colors.Black));

        Layout.MaximalEinpassen(etikett, bild);

        // Druckbereich 76 × 46 mm; Verhältnis 2:1 → Breite 76 mm, Höhe 38 mm, mittig.
        Assert.Equal(76, bild.Breite, 3);
        Assert.Equal(38, bild.Hoehe, 3);
        Assert.Equal(2, bild.X, 3);
        Assert.Equal(6, bild.Y, 3);
        Assert.Empty(Layout.Pruefen(EtikettMit(80, Raender.Standard, bild)));
    }

    [Fact]
    public void Drehen_um_90_Grad_tauscht_das_Seitenverhaeltnis_ohne_Verzerrung()
    {
        var etikett = EtikettMit(80, Raender.Standard);
        var bild = new BildElement(Einfarbig(200, 100, Colors.Black));
        Layout.MaximalEinpassen(etikett, bild);

        Layout.Drehen(bild, Drehung.Grad90);

        Assert.Equal(Drehung.Grad90, bild.Drehung);
        Assert.Equal(0.5, bild.Breite / bild.Hoehe, 2);
        Assert.Equal(38, bild.Hoehe, 3);
        Assert.Equal(40, bild.X + (bild.Breite / 2), 1);
    }

    [Fact]
    public void Zu_grosse_Raender_und_ueberstehende_Elemente_werden_gemeldet()
    {
        var bild = new BildElement(Einfarbig(10, 10, Colors.Black)) { X = 0, Y = 0, Breite = 10, Hoehe = 10 };
        var etikett = EtikettMit(10, new Raender(6, 6, 0.5, 2), bild);

        var fehler = Layout.Pruefen(etikett);

        Assert.Contains(fehler, f => f.Contains("links und rechts"));
        Assert.Contains(fehler, f => f.Contains("mindestens"));
        Assert.Contains(fehler, f => f.Contains("ragt über den Rand"));
    }

    [Fact]
    public void Schwelle_entscheidet_ob_Grau_schwarz_wird()
    {
        Assert.False(Renderer.IstSchwarz(128, 128, 128, 255 * 0.40));
        Assert.True(Renderer.IstSchwarz(128, 128, 128, 255 * 0.60));
        Assert.False(Renderer.IstSchwarz(255, 255, 255, 255));
    }

    [Fact]
    public void Quadrat_40_mm_wird_exakt_320_Punkte_gross_gerastert()
    {
        var bild = new BildElement(Einfarbig(50, 50, Colors.Black)) { X = 0, Y = 5, Breite = 40, Hoehe = 40 };
        var etikett = EtikettMit(40, new Raender(0, 0, 1, 1), bild);

        var raster = Renderer.Rastern(etikett, 50);

        Assert.Equal(384, raster.Breite);
        Assert.Equal(320, raster.Hoehe);
        var (minZ, maxZ, minS, maxS, _) = Schwarzbereich(raster);
        Assert.Equal(0, minZ);
        Assert.Equal(319, maxZ);
        Assert.InRange(maxS - minS + 1, 319, 321);
    }

    [Fact]
    public void Element_oben_links_in_der_Ansicht_liegt_am_Anfang_und_an_hohen_Spalten()
    {
        // 5 × 5 mm am Anfang des Etiketts, direkt unter dem oberen nicht bedruckbaren Streifen.
        var bild = new BildElement(Einfarbig(10, 10, Colors.Black)) { X = 0, Y = Etikett.UnbedruckbarMm, Breite = 5, Hoehe = 5 };
        var etikett = EtikettMit(20, new Raender(0, 0, 1, 1), bild);

        var raster = Renderer.Rastern(etikett, 50);

        var (minZ, maxZ, minS, maxS, _) = Schwarzbereich(raster);
        Assert.Equal(0, minZ);
        Assert.InRange(maxZ, 38, 40);
        Assert.Equal(383, maxS);
        Assert.InRange(minS, 343, 345);
    }

    [Fact]
    public void Graues_Bild_haengt_vom_Regler_Staerke_fuer_Schwarz_ab()
    {
        var bild = new BildElement(Einfarbig(10, 10, Color.FromRgb(128, 128, 128))) { X = 2, Y = 5, Breite = 10, Hoehe = 10 };
        var etikett = EtikettMit(20, new Raender(1, 1, 1, 1), bild);

        Assert.Equal(0, Schwarzbereich(Renderer.Rastern(etikett, 40)).Anzahl);
        Assert.InRange(Schwarzbereich(Renderer.Rastern(etikett, 60)).Anzahl, 79 * 79, 81 * 81);
    }

    [Fact]
    public void Kalibrierfaktor_verlaengert_das_Raster()
    {
        var etikett = EtikettMit(80, Raender.Standard);

        var raster = Renderer.Rastern(etikett, 50, new Kalibrierung(1.01, 1.0));

        Assert.Equal(646, raster.Hoehe);
    }

    [Fact]
    public void Text_mit_Symbolen_wird_bemessen_und_gedruckt()
    {
        var text = new TextElement { Text = "Gewürze ° € ✓ ★ ♥ →", GroessePt = 14, X = 2, Y = 5 };
        Renderer.TextBemessen(text);
        var etikett = EtikettMit(80, Raender.Standard, text);

        var raster = Renderer.Rastern(etikett, 50);

        Assert.InRange(text.Hoehe, 4, 8);
        Assert.True(text.Breite > 20);
        Assert.True(Schwarzbereich(raster).Anzahl > 200);
        Assert.Empty(Layout.Pruefen(etikett));
    }

    [Fact]
    public void Bemessener_Text_bricht_nicht_um()
    {
        var text = new TextElement { Text = "Haltbar bis 12/26", GroessePt = 12, X = 2, Y = 5 };
        Renderer.TextBemessen(text);
        var einzeilig = text.Hoehe;
        var etikett = EtikettMit(80, Raender.Standard, text);

        var (minZ, maxZ, minS, maxS, _) = Schwarzbereich(Renderer.Rastern(etikett, 50));

        // Einzeilig: die schwarze Fläche ist nicht höher als eine Zeile.
        Assert.True((maxS - minS + 1) <= Layout.MmZuPunkten(einzeilig), "Der Text wurde umgebrochen.");
        Assert.True(maxZ > minZ);
    }

    [Theory]
    [InlineData("😀")]
    [InlineData("👍")]
    [InlineData("☎")]
    [InlineData("⚠")]
    public void Emojis_und_Symbole_werden_gedruckt(string zeichen)
    {
        var text = new TextElement { Text = zeichen, GroessePt = 28, X = 2, Y = 5 };
        Renderer.TextBemessen(text);
        var etikett = EtikettMit(40, Raender.Standard, text);

        var raster = Renderer.Rastern(etikett, 50);

        Assert.True(Schwarzbereich(raster).Anzahl > 100, $"„{zeichen}“ erzeugt kaum schwarze Punkte.");
    }

    [Fact]
    public void Druckvorbereitung_liefert_Auftrag_mit_Dichtestufe_und_Nachlauf()
    {
        var bild = new BildElement(Einfarbig(20, 20, Colors.Black));
        var etikett = EtikettMit(40, Raender.Standard);
        Layout.MaximalEinpassen(etikett, bild);
        etikett.Elemente.Add(bild);

        var daten = Druckvorbereitung.Erstellen(etikett, new Druckeinstellungen(50, 80, Kalibrierung.Keine));

        Assert.Equal(320, daten.Bild.Hoehe);
        Assert.Equal(T02Befehle.Vorlaufzeilen + 320 + T02Befehle.Nachlaufzeilen, daten.Zeilen);
        Assert.Equal(T02Befehle.Druckdichte(4), daten.Auftrag[2..10]);
    }

    [Fact]
    public void Druckvorbereitung_lehnt_ungueltiges_Etikett_ab()
    {
        var etikett = EtikettMit(3, Raender.Standard);

        var fehler = Assert.Throws<InvalidOperationException>(() => Druckvorbereitung.Erstellen(etikett, Druckeinstellungen.Standard));
        Assert.Contains("links und rechts", fehler.Message);
    }
}
