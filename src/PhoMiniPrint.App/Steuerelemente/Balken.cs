using System.Windows;
using System.Windows.Media;

namespace PhoMiniPrint.App.Steuerelemente;

/// <summary>Kleine Balkenanzeige 0–100 % für Akku und Verbindungsqualität (FR-020, FR-021).</summary>
public sealed class Balken : FrameworkElement
{
    public static readonly DependencyProperty ProzentProperty = DependencyProperty.Register(
        nameof(Prozent), typeof(int?), typeof(Balken),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    private static readonly Brush Spur = Gefroren(Color.FromRgb(0x3A, 0x41, 0x4C));
    private static readonly Brush Gut = Gefroren(Color.FromRgb(0x3E, 0xCF, 0x8E));
    private static readonly Brush Mittel = Gefroren(Color.FromRgb(0xF2, 0x6B, 0x1D));
    private static readonly Brush Schlecht = Gefroren(Color.FromRgb(0xFF, 0x4D, 0x4D));

    public Balken()
    {
        Width = 46;
        Height = 8;
        SnapsToDevicePixels = true;
    }

    /// <summary>Wert 0–100; null = unbekannt (leere Spur).</summary>
    public int? Prozent
    {
        get => (int?)GetValue(ProzentProperty);
        set => SetValue(ProzentProperty, value);
    }

    /// <summary>Farbe nach Wert: ab 50 % grün, ab 20 % orange, darunter rot.</summary>
    public static Brush FarbeFuer(int prozent) => prozent >= 50 ? Gut : prozent >= 20 ? Mittel : Schlecht;

    protected override void OnRender(DrawingContext drawingContext)
    {
        ArgumentNullException.ThrowIfNull(drawingContext);
        var r = new Rect(RenderSize);
        var radius = r.Height / 2;
        drawingContext.DrawRoundedRectangle(Spur, null, r, radius, radius);
        if (Prozent is { } p && p > 0)
        {
            var breite = Math.Max(r.Height, r.Width * Math.Clamp(p, 0, 100) / 100.0);
            drawingContext.DrawRoundedRectangle(FarbeFuer(p), null, new Rect(0, 0, breite, r.Height), radius, radius);
        }
    }

    private static SolidColorBrush Gefroren(Color farbe)
    {
        var pinsel = new SolidColorBrush(farbe);
        pinsel.Freeze();
        return pinsel;
    }
}
