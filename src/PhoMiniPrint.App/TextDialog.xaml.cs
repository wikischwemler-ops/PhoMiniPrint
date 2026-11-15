using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PhoMiniPrint.Core;

namespace PhoMiniPrint.App;

/// <summary>Text mit Symbolen eingeben und formatieren (F-06, FR-009, FR-010).</summary>
public partial class TextDialog : Window
{
    private static readonly double[] Groessen = [6, 8, 10, 12, 14, 16, 18, 20, 24, 28, 32, 36, 48, 60, 72, 96];
    private bool _bereit;

    public TextDialog(TextElement vorlage)
    {
        ArgumentNullException.ThrowIfNull(vorlage);
        InitializeComponent();

        var schriften = Fonts.SystemFontFamilies
            .Select(f => f.Source)
            .Where(n => !n.StartsWith('@'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.CurrentCultureIgnoreCase)
            .ToList();
        SchriftAuswahl.ItemsSource = schriften;
        SchriftAuswahl.SelectedItem = schriften.FirstOrDefault(n => string.Equals(n, vorlage.Schriftart, StringComparison.OrdinalIgnoreCase)) ?? "Segoe UI";

        GroesseAuswahl.ItemsSource = Groessen.Select(g => g.ToString(CultureInfo.CurrentCulture));
        GroesseAuswahl.Text = vorlage.GroessePt.ToString("0.#", CultureInfo.CurrentCulture);
        FettKnopf.IsChecked = vorlage.Fett;
        Links.IsChecked = vorlage.Ausrichtung == Textausrichtung.Links;
        Mittig.IsChecked = vorlage.Ausrichtung == Textausrichtung.Mittig;
        Rechts.IsChecked = vorlage.Ausrichtung == Textausrichtung.Rechts;
        Textfeld.Text = vorlage.Text;
        SymbolListe.ItemsSource = Zeichenpruefung.Symbole;
        Title = string.IsNullOrEmpty(vorlage.Text) ? "Text einfügen" : "Text bearbeiten";

        _bereit = true;
        Pruefen();
        Loaded += (_, _) =>
        {
            Textfeld.Focus();
            Textfeld.CaretIndex = Textfeld.Text.Length;
        };
    }

    /// <summary>Eingegebener Text (nach „Übernehmen“).</summary>
    public string Text => Textfeld.Text;

    public string Schriftart => SchriftAuswahl.SelectedItem as string ?? "Segoe UI";

    public bool Fett => FettKnopf.IsChecked == true;

    public Textausrichtung Ausrichtung =>
        Mittig.IsChecked == true ? Textausrichtung.Mittig : Rechts.IsChecked == true ? Textausrichtung.Rechts : Textausrichtung.Links;

    public double GroessePt { get; private set; } = 14;

    private void Einstellung_Geaendert(object sender, RoutedEventArgs e) => Pruefen();

    private void Pruefen()
    {
        if (!_bereit)
        {
            return;
        }

        Textfeld.FontFamily = new FontFamily(Schriftart);
        Textfeld.FontWeight = Fett ? FontWeights.Bold : FontWeights.Normal;
        Textfeld.TextAlignment = Ausrichtung switch
        {
            Textausrichtung.Mittig => TextAlignment.Center,
            Textausrichtung.Rechts => TextAlignment.Right,
            _ => TextAlignment.Left,
        };

        var groesseOk = double.TryParse(GroesseAuswahl.Text.Replace('.', ','), NumberStyles.Float, CultureInfo.GetCultureInfo("de-DE"), out var pt)
                        && pt is >= 4 and <= 400;
        if (groesseOk)
        {
            GroessePt = pt;
            HoeheText.Text = $"Großbuchstaben ca. {pt * Renderer.MmProPt * 0.7:0.0} mm hoch";
        }
        else
        {
            HoeheText.Text = "Schriftgröße 4 bis 400 pt";
        }

        var fehlend = Zeichenpruefung.NichtDruckbar(Text, Schriftart);
        WarnText.Text = string.Join(Environment.NewLine, fehlend.Select(z => $"Das Zeichen ‚{z}‘ kann nicht gedruckt werden."));
        WarnText.Visibility = fehlend.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        OkKnopf.IsEnabled = groesseOk && fehlend.Count == 0 && !string.IsNullOrWhiteSpace(Text);
    }

    private void Symbol_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Content: string symbol })
        {
            var start = Textfeld.SelectionStart;
            Textfeld.SelectedText = symbol;
            Textfeld.SelectionLength = 0;
            Textfeld.CaretIndex = start + symbol.Length;
            Textfeld.Focus();
        }
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (OkKnopf.IsEnabled)
        {
            DialogResult = true;
        }
    }
}
