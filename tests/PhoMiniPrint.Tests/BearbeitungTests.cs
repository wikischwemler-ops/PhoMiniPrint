using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PhoMiniPrint.Core;

namespace PhoMiniPrint.Tests;

public class BearbeitungTests
{
    private static BildElement Bild(int breite, int hoehe)
    {
        var pixel = new byte[breite * hoehe * 4];
        Array.Fill<byte>(pixel, 0);
        for (var i = 3; i < pixel.Length; i += 4)
        {
            pixel[i] = 255;
        }

        return new BildElement(BitmapSource.Create(breite, hoehe, 96, 96, PixelFormats.Bgra32, null, pixel, breite * 4));
    }

    private static Etikett Etikett80() => new() { Laenge = 80, Raender = Raender.Standard };

    [Fact]
    public void Verschieben_bleibt_im_Druckbereich()
    {
        var etikett = Etikett80();
        var bild = Bild(10, 10);
        bild.Breite = 10;
        bild.Hoehe = 10;

        Bearbeitung.Verschieben(bild, -5, 100, etikett);

        Assert.Equal(2, bild.X, 6);
        Assert.Equal(48 - 10, bild.Y, 6);
        Assert.Empty(Layout.Pruefen(new Etikett { Laenge = 80, Raender = Raender.Standard, Elemente = { bild } }));
    }

    [Fact]
    public void Ecke_ziehen_haelt_Seitenverhaeltnis_und_gegenueberliegende_Ecke()
    {
        var etikett = Etikett80();
        var bild = Bild(200, 100);
        bild.X = 10;
        bild.Y = 10;
        bild.Breite = 20;
        bild.Hoehe = 10;

        Bearbeitung.GroesseAendern(bild, Ecke.UntenRechts, 50, 12, etikett);

        Assert.Equal(10, bild.X, 6);
        Assert.Equal(10, bild.Y, 6);
        Assert.Equal(40, bild.Breite, 6);
        Assert.Equal(20, bild.Hoehe, 6);
    }

    [Fact]
    public void Ecke_oben_links_wird_am_Rand_begrenzt()
    {
        var etikett = Etikett80();
        var bild = Bild(100, 100);
        bild.X = 20;
        bild.Y = 20;
        bild.Breite = 10;
        bild.Hoehe = 10;

        Bearbeitung.GroesseAendern(bild, Ecke.ObenLinks, -100, -100, etikett);

        // Feste Ecke bei (30, 30); bis zum Rand oben (2 mm) sind es 28 mm.
        Assert.Equal(28, bild.Breite, 6);
        Assert.Equal(28, bild.Hoehe, 6);
        Assert.Equal(2, bild.X, 6);
        Assert.Equal(2, bild.Y, 6);
    }

    [Fact]
    public void Groesse_setzen_ist_exakt_und_unverzerrt()
    {
        var etikett = Etikett80();
        var bild = Bild(300, 200);
        Layout.MaximalEinpassen(etikett, bild);

        Bearbeitung.GroesseSetzen(bild, 30, null, etikett);

        Assert.Equal(30, bild.Breite, 6);
        Assert.Equal(20, bild.Hoehe, 6);
        Assert.Empty(Layout.Pruefen(new Etikett { Laenge = 80, Raender = Raender.Standard, Elemente = { bild } }));
    }

    [Fact]
    public void Zu_grosse_Eingabe_wird_auf_den_Druckbereich_begrenzt()
    {
        var etikett = Etikett80();
        var bild = Bild(100, 100);
        Layout.MaximalEinpassen(etikett, bild);

        Bearbeitung.GroesseSetzen(bild, null, 60, etikett);

        Assert.Equal(46, bild.Hoehe, 6);
        Assert.Equal(46, bild.Breite, 6);
    }

    [Fact]
    public void Vorschau_zeigt_das_Raster_in_der_Lage_der_Bandansicht()
    {
        // 5 × 5 mm oben links im bedruckbaren Bereich.
        var bild = Bild(10, 10);
        bild.X = 0;
        bild.Y = Etikett.UnbedruckbarMm;
        bild.Breite = 5;
        bild.Hoehe = 5;
        var etikett = new Etikett { Laenge = 20, Raender = new Raender(0, 0, 1, 1), Elemente = { bild } };

        var ansicht = Vorschau.Ansicht(Renderer.Rastern(etikett, 50));

        Assert.Equal(160, ansicht.PixelWidth);
        Assert.Equal(384, ansicht.PixelHeight);
        var pixel = new byte[160 * 384];
        ansicht.CopyPixels(pixel, 160, 0);
        Assert.True(pixel[(5 * 160) + 5] < 128, "Oben links muss schwarz sein.");
        Assert.True(pixel[(5 * 160) + 100] > 128, "Rechts daneben muss weiß sein.");
        Assert.True(pixel[(300 * 160) + 5] > 128, "Unten muss weiß sein.");
    }

    [Fact]
    public void Falsches_Format_wird_deutsch_gemeldet()
    {
        var fehler = Assert.Throws<BildFehler>(() => Bildlader.Laden("bild.gif"));
        Assert.Contains("PNG oder JPG", fehler.Message);
    }

    [Fact]
    public void Defekte_Datei_wird_deutsch_gemeldet()
    {
        var pfad = Path.Combine(Path.GetTempPath(), $"phominiprint-{Guid.NewGuid():N}.png");
        System.IO.File.WriteAllText(pfad, "kein Bild");
        try
        {
            var fehler = Assert.Throws<BildFehler>(() => Bildlader.Laden(pfad));
            Assert.Equal("Das Bild konnte nicht gelesen werden.", fehler.Message);
        }
        finally
        {
            System.IO.File.Delete(pfad);
        }
    }

    [Fact]
    public void Png_wird_geladen()
    {
        var pfad = Path.Combine(Path.GetTempPath(), $"phominiprint-{Guid.NewGuid():N}.png");
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(Bild(30, 20).Quelle));
        using (var datei = System.IO.File.Create(pfad))
        {
            encoder.Save(datei);
        }

        try
        {
            var bild = Bildlader.Laden(pfad);
            Assert.Equal(30, bild.PixelWidth);
            Assert.Equal(20, bild.PixelHeight);
            Assert.True(bild.IsFrozen);
        }
        finally
        {
            System.IO.File.Delete(pfad);
        }
    }

    [Fact]
    public void Druckfrist_waechst_mit_der_Laenge()
    {
        Assert.True(Druckdienst.Druckfrist(800) > Druckdienst.Druckfrist(80));
        Assert.True(Druckdienst.Druckfrist(0) >= TimeSpan.FromSeconds(10));
    }
}
