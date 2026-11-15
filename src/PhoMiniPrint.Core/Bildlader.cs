using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhoMiniPrint.Core;

/// <summary>Fehler beim Laden eines Bildes; die Meldung ist für den Benutzer bestimmt.</summary>
public sealed class BildFehler(string meldung, Exception? ursache = null) : Exception(meldung, ursache);

/// <summary>Lädt PNG- und JPG-Bilder (F-04, FR-005).</summary>
public static class Bildlader
{
    /// <summary>Unterstützte Dateiendungen.</summary>
    public static IReadOnlyList<string> Endungen { get; } = [".png", ".jpg", ".jpeg"];

    /// <summary>Filter für den Dateidialog.</summary>
    public const string Dateifilter = "Bilder (*.png; *.jpg; *.jpeg)|*.png;*.jpg;*.jpeg";

    /// <summary>Größere Bilder werden verkleinert: 3000 Punkte reichen für 375 mm bei 8 Punkten/mm.</summary>
    public const int MaxKantePixel = 3000;

    public static bool IstUnterstuetzt(string pfad) =>
        Endungen.Contains(Path.GetExtension(pfad), StringComparer.OrdinalIgnoreCase);

    /// <summary>Lädt das Bild, dreht Fotos nach ihrer EXIF-Ausrichtung und friert es ein.</summary>
    public static BitmapSource Laden(string pfad)
    {
        ArgumentNullException.ThrowIfNull(pfad);
        if (!IstUnterstuetzt(pfad))
        {
            throw new BildFehler("Dieses Format wird nicht unterstützt. Bitte PNG oder JPG verwenden.");
        }

        try
        {
            using var datei = File.OpenRead(pfad);
            var decoder = BitmapDecoder.Create(datei, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            var frame = decoder.Frames[0];
            BitmapSource bild = frame;

            var drehung = ExifDrehung(frame);
            if (drehung != 0)
            {
                bild = new TransformedBitmap(bild, new RotateTransform(drehung));
            }

            var kante = Math.Max(bild.PixelWidth, bild.PixelHeight);
            if (kante > MaxKantePixel)
            {
                var faktor = (double)MaxKantePixel / kante;
                bild = new TransformedBitmap(bild, new ScaleTransform(faktor, faktor));
            }

            bild = new FormatConvertedBitmap(bild, PixelFormats.Bgra32, null, 0);
            var fertig = new WriteableBitmap(bild);
            fertig.Freeze();
            return fertig;
        }
        catch (Exception ex) when (ex is IOException or NotSupportedException or FileFormatException
                                       or ArgumentException or InvalidOperationException or OverflowException
                                       or UnauthorizedAccessException)
        {
            throw new BildFehler("Das Bild konnte nicht gelesen werden.", ex);
        }
    }

    /// <summary>Drehwinkel im Uhrzeigersinn laut EXIF-Ausrichtung (nur 1, 3, 6, 8; Spiegelungen werden ignoriert).</summary>
    private static int ExifDrehung(BitmapFrame frame)
    {
        try
        {
            if (frame.Metadata is BitmapMetadata daten && daten.GetQuery("System.Photo.Orientation") is ushort ausrichtung)
            {
                return ausrichtung switch
                {
                    3 => 180,
                    6 => 90,
                    8 => 270,
                    _ => 0,
                };
            }
        }
        catch (Exception ex) when (ex is NotSupportedException or InvalidOperationException or ArgumentException)
        {
            // Keine lesbaren Metadaten – Bild bleibt, wie es ist.
        }

        return 0;
    }
}
