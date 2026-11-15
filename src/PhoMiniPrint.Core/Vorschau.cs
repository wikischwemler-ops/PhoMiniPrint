using System.Windows.Media;
using System.Windows.Media.Imaging;
using PhoMiniPrint.Printer;

namespace PhoMiniPrint.Core;

/// <summary>
/// Wandelt das Druckraster zurück in die Lage der Bandansicht (F-07: Ansicht zeigt das Schwarz-Weiß-Ergebnis).
/// Ein Bildpunkt entspricht einem Druckerpunkt (1/8 mm); x = Länge, y = bedruckbare Höhe von oben.
/// </summary>
public static class Vorschau
{
    public static BitmapSource Ansicht(Rasterbild raster)
    {
        ArgumentNullException.ThrowIfNull(raster);
        var breite = raster.Hoehe;
        var hoehe = raster.Breite;
        var pixel = new byte[breite * hoehe];
        for (var y = 0; y < hoehe; y++)
        {
            var spalte = raster.Breite - 1 - y;
            for (var x = 0; x < breite; x++)
            {
                pixel[(y * breite) + x] = raster[spalte, x] ? (byte)0x1C : (byte)0xFF;
            }
        }

        var bild = BitmapSource.Create(breite, hoehe, 96, 96, PixelFormats.Gray8, null, pixel, breite);
        bild.Freeze();
        return bild;
    }
}
