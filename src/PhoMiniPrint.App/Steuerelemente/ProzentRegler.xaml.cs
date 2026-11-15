using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PhoMiniPrint.Printer;

namespace PhoMiniPrint.App.Steuerelemente;

/// <summary>Art des Reglers: bestimmt die Zusatzanzeige unter der Spur.</summary>
public enum ReglerArt
{
    /// <summary>„Stärke für Schwarz“ mit Grauverlauf-Vorschau.</summary>
    Schwarz,

    /// <summary>„Druckdichte“ mit Anzeige der aktiven Stufe 1–4.</summary>
    Dichte,
}

/// <summary>Gestalteter Regler 0–100 % mit großer Zahl, Skala und Zusatzanzeige (FR-007).</summary>
public partial class ProzentRegler : UserControl
{
    public static readonly DependencyProperty WertProperty = DependencyProperty.Register(
        nameof(Wert), typeof(int), typeof(ProzentRegler),
        new FrameworkPropertyMetadata(50, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, _) => ((ProzentRegler)d).Anzeigen()));

    private bool _intern;

    public ProzentRegler()
    {
        InitializeComponent();
        SchwelleFlaeche.SizeChanged += (_, _) => Anzeigen();
        Loaded += (_, _) => Anzeigen();
    }

    /// <summary>Wert wurde vom Benutzer geändert.</summary>
    public event EventHandler? WertGeaendert;

    public int Wert
    {
        get => (int)GetValue(WertProperty);
        set => SetValue(WertProperty, Math.Clamp(value, 0, 100));
    }

    public string Titel
    {
        get => TitelText.Text;
        set => TitelText.Text = value;
    }

    public ReglerArt Art
    {
        get => GrauVorschau.Visibility == Visibility.Visible ? ReglerArt.Schwarz : ReglerArt.Dichte;
        set
        {
            GrauVorschau.Visibility = value == ReglerArt.Schwarz ? Visibility.Visible : Visibility.Collapsed;
            StufenAnzeige.Visibility = value == ReglerArt.Dichte ? Visibility.Visible : Visibility.Collapsed;
            Anzeigen();
        }
    }

    private void Regler_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_intern)
        {
            return;
        }

        Wert = (int)Math.Round(e.NewValue);
        WertGeaendert?.Invoke(this, EventArgs.Empty);
    }

    private void Anzeigen()
    {
        if (Regler is null)
        {
            return;
        }

        _intern = true;
        Regler.Value = Wert;
        _intern = false;
        ZahlText.Text = Wert.ToString(CultureInfo.CurrentCulture);

        // Grauverlauf schwarz → weiß: dunkler als die Schwelle wird schwarz, also der linke Anteil „Wert %“.
        SchwelleBalken.Width = SchwelleFlaeche.ActualWidth * Wert / 100.0;

        var aktiv = T02Befehle.DichtestufeAusProzent(Wert);
        Border[] stufen = [Stufe1, Stufe2, Stufe3, Stufe4];
        for (var i = 0; i < stufen.Length; i++)
        {
            var istAktiv = i + 1 == aktiv;
            stufen[i].Background = istAktiv ? (Brush)FindResource("Akzent") : (Brush)FindResource("FlaecheHell");
            ((TextBlock)stufen[i].Child).FontWeight = istAktiv ? FontWeights.Bold : FontWeights.Normal;
        }
    }
}

/// <summary>Skala 0/25/50/75/100 unter der Reglerspur.</summary>
public sealed class ReglerSkala : FrameworkElement
{
    protected override void OnRender(DrawingContext drawingContext)
    {
        ArgumentNullException.ThrowIfNull(drawingContext);
        var pinsel = (Brush)FindResource("TextGedimmt");
        var stift = new Pen(pinsel, 1);
        var dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        for (var wert = 0; wert <= 100; wert += 5)
        {
            var x = ActualWidth * wert / 100.0;
            var gross = wert % 25 == 0;
            drawingContext.DrawLine(stift, new Point(x, 0), new Point(x, gross ? 4 : 2));
            if (gross)
            {
                var text = new FormattedText(
                    wert.ToString(CultureInfo.CurrentCulture), CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"), 10, pinsel, dpi);
                drawingContext.DrawText(text, new Point(x - (text.Width / 2), 4));
            }
        }
    }
}
