using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PhoMiniPrint.Printer;

namespace PhoMiniPrint.Core;

/// <summary>
/// Rendert ein Etikett in Druckerpunkte (F-08). Dieselbe Zeichenroutine dient später der Bandansicht,
/// damit Ansicht und Ausdruck übereinstimmen (ARCHITECTURE V-04).
/// </summary>
public static class Renderer
{
    /// <summary>Millimeter je typografischem Punkt.</summary>
    public const double MmProPt = 25.4 / 72.0;

    /// <summary>
    /// Erzeugt das Rasterbild für den T02: 384 Punkte quer zum Band, eine Zeile je 1/8 mm Länge.
    /// Ausrichtung (gemessen in M1): Zeile 0 = Anfang des Etiketts; Spalte 0 = untere Bandkante
    /// in der Bandansicht. Dadurch erscheint der Ausdruck seitenrichtig, wenn der Anfang links liegt.
    /// </summary>
    /// <param name="staerkeSchwarzProzent">Regler „Stärke für Schwarz“ 0–100 % (FR-007).</param>
    public static Rasterbild Rastern(Etikett etikett, int staerkeSchwarzProzent, Kalibrierung? kalibrierung = null)
    {
        ArgumentNullException.ThrowIfNull(etikett);
        ArgumentOutOfRangeException.ThrowIfLessThan(staerkeSchwarzProzent, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(staerkeSchwarzProzent, 100);
        kalibrierung ??= Kalibrierung.Keine;

        return StaAusfuehren(() =>
        {
            var zeilen = Math.Max(1, Layout.MmZuPunkten(etikett.Laenge, kalibrierung.FaktorLaenge));
            var spalten = T02Geraet.DruckbreitePunkte;

            // Etikett-Raum (mm) → Bitmap: x = Länge, y = bedruckbare Höhe.
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, zeilen, spalten));
                dc.PushTransform(new ScaleTransform(
                    T02Geraet.PunkteProMm * kalibrierung.FaktorLaenge,
                    T02Geraet.PunkteProMm * kalibrierung.FaktorHoehe));
                dc.PushTransform(new TranslateTransform(0, -Etikett.UnbedruckbarMm));
                Zeichnen(dc, etikett);
                dc.Pop();
                dc.Pop();
            }

            var bitmap = new RenderTargetBitmap(zeilen, spalten, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            var pixel = new byte[zeilen * spalten * 4];
            bitmap.CopyPixels(pixel, zeilen * 4, 0);

            var schwelle = 255.0 * staerkeSchwarzProzent / 100.0;
            var raster = new Rasterbild(spalten, zeilen);
            for (var y = 0; y < spalten; y++)
            {
                for (var x = 0; x < zeilen; x++)
                {
                    var i = ((y * zeilen) + x) * 4;
                    if (IstSchwarz(pixel[i + 2], pixel[i + 1], pixel[i], schwelle))
                    {
                        raster[spalten - 1 - y, x] = true;
                    }
                }
            }

            return raster;
        });
    }

    /// <summary>Helligkeit 0–255 (ITU-R BT.601); schwarz, wenn dunkler als die Schwelle.</summary>
    public static bool IstSchwarz(byte rot, byte gruen, byte blau, double schwelle) =>
        ((0.299 * rot) + (0.587 * gruen) + (0.114 * blau)) < schwelle;

    /// <summary>Zeichnet alle Elemente im Etikett-Raum (mm). Wird auch für die Bandansicht verwendet.</summary>
    public static void Zeichnen(DrawingContext dc, Etikett etikett)
    {
        ArgumentNullException.ThrowIfNull(dc);
        ArgumentNullException.ThrowIfNull(etikett);
        foreach (var element in etikett.Elemente)
        {
            // Lokales Kästchen vor der Drehung; bei 90°/270° sind Breite und Höhe vertauscht.
            var quer = element.Drehung is Drehung.Grad90 or Drehung.Grad270;
            var b0 = quer ? element.Hoehe : element.Breite;
            var h0 = quer ? element.Breite : element.Hoehe;

            dc.PushTransform(new TranslateTransform(element.X + (element.Breite / 2), element.Y + (element.Hoehe / 2)));
            dc.PushTransform(new RotateTransform((int)element.Drehung));
            var kasten = new Rect(-b0 / 2, -h0 / 2, b0, h0);

            switch (element)
            {
                case BildElement bild:
                    dc.DrawImage(bild.Quelle, kasten);
                    break;
                case TextElement text:
                    var ft = TextFormatieren(text, b0);
                    dc.DrawText(ft, kasten.TopLeft);
                    break;
            }

            dc.Pop();
            dc.Pop();
        }
    }

    /// <summary>Bemisst einen Text und setzt Breite/Höhe des Elements (in mm, auf Druckerpunkte gerundet).</summary>
    public static void TextBemessen(TextElement text)
    {
        ArgumentNullException.ThrowIfNull(text);
        StaAusfuehren(() =>
        {
            var ft = TextFormatieren(text, null);
            // Aufrunden: eine minimal zu schmale Breite würde beim Zeichnen einen Zeilenumbruch auslösen.
            var b = Layout.AufPunktAufrunden(Math.Max(Layout.PunktMm, ft.WidthIncludingTrailingWhitespace));
            var h = Layout.AufPunktAufrunden(Math.Max(Layout.PunktMm, ft.Height));
            var quer = text.Drehung is Drehung.Grad90 or Drehung.Grad270;
            text.Breite = quer ? h : b;
            text.Hoehe = quer ? b : h;
            return 0;
        });
    }

    private static FormattedText TextFormatieren(TextElement text, double? breite)
    {
        var typeface = new Typeface(
            new FontFamily(text.Schriftart),
            FontStyles.Normal,
            text.Fett ? FontWeights.Bold : FontWeights.Normal,
            FontStretches.Normal);
        var ft = new FormattedText(
            text.Text,
            CultureInfo.GetCultureInfo("de-DE"),
            FlowDirection.LeftToRight,
            typeface,
            text.GroessePt * MmProPt,
            Brushes.Black,
            1.0)
        {
            TextAlignment = text.Ausrichtung switch
            {
                Textausrichtung.Mittig => TextAlignment.Center,
                Textausrichtung.Rechts => TextAlignment.Right,
                _ => TextAlignment.Left,
            },
        };
        if (breite is > 0)
        {
            ft.MaxTextWidth = breite.Value;
        }

        return ft;
    }

    /// <summary>WPF-Grafik braucht einen STA-Thread; außerhalb der Oberfläche wird einer gestartet.</summary>
    private static T StaAusfuehren<T>(Func<T> arbeit)
    {
        if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
        {
            return arbeit();
        }

        T? ergebnis = default;
        Exception? fehler = null;
        var thread = new Thread(() =>
        {
            try
            {
                ergebnis = arbeit();
            }
            catch (Exception ex)
            {
                fehler = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (fehler is not null)
        {
            throw new InvalidOperationException("Rendern fehlgeschlagen.", fehler);
        }

        return ergebnis!;
    }
}
