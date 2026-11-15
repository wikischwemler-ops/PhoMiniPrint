using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PhoMiniPrint.Core;

namespace PhoMiniPrint.App;

/// <summary>Was der Benutzer im Kalibrierdialog gewählt hat.</summary>
public enum KalibrierAktion
{
    Keine,
    TestseiteDrucken,
    Gespeichert,
    Zurueckgesetzt,
}

/// <summary>Kalibrierung über das Menü (F-09, FR-012, Q-A5b).</summary>
public partial class KalibrierDialog : Window
{
    private static readonly CultureInfo Deutsch = CultureInfo.GetCultureInfo("de-DE");
    private readonly Kalibrierung _bisher;
    private Kalibrierung? _neu;

    public KalibrierDialog(Kalibrierung bisher, bool druckenMoeglich)
    {
        ArgumentNullException.ThrowIfNull(bisher);
        InitializeComponent();
        _bisher = bisher;
        TestseiteKnopf.IsEnabled = druckenMoeglich;
        AktuellText.Text = $"Aktuelle Faktoren: Länge × {bisher.FaktorLaenge.ToString("0.0000", Deutsch)} · Höhe × {bisher.FaktorHoehe.ToString("0.0000", Deutsch)}";
        Berechnen();
        Loaded += (_, _) => LaengeFeld.Focus();
    }

    public KalibrierAktion Aktion { get; private set; }

    /// <summary>Neue Faktoren nach „Speichern“ oder „Zurücksetzen“.</summary>
    public Kalibrierung? Ergebnis { get; private set; }

    private void Feld_TextChanged(object sender, TextChangedEventArgs e) => Berechnen();

    private void Berechnen()
    {
        if (SpeichernKnopf is null)
        {
            return;
        }

        var okL = Lesen(LaengeFeld, out var laenge);
        var okH = Lesen(HoeheFeld, out var hoehe);
        _neu = null;
        if (!okL || !okH)
        {
            ErgebnisText.Text = "Bitte beide Maße in mm eingeben, z. B. 79,5.";
            ErgebnisText.Foreground = (Brush)FindResource("Fehler");
        }
        else
        {
            var (neu, fehler) = Kalibrierrechnung.Berechnen(_bisher, laenge, hoehe);
            _neu = neu;
            ErgebnisText.Text = neu is null
                ? fehler
                : $"Neue Faktoren: Länge × {neu.FaktorLaenge.ToString("0.0000", Deutsch)} · Höhe × {neu.FaktorHoehe.ToString("0.0000", Deutsch)}";
            ErgebnisText.Foreground = neu is null ? (Brush)FindResource("Fehler") : (Brush)FindResource("Text");
        }

        SpeichernKnopf.IsEnabled = _neu is not null;
    }

    private static bool Lesen(TextBox feld, out double wert) =>
        double.TryParse(feld.Text.Trim().Replace('.', ','), NumberStyles.Float, Deutsch, out wert) && wert > 0;

    private void Testseite_Click(object sender, RoutedEventArgs e)
    {
        Aktion = KalibrierAktion.TestseiteDrucken;
        DialogResult = true;
    }

    private void Speichern_Click(object sender, RoutedEventArgs e)
    {
        if (_neu is null)
        {
            return;
        }

        Aktion = KalibrierAktion.Gespeichert;
        Ergebnis = _neu;
        DialogResult = true;
    }

    private void Zuruecksetzen_Click(object sender, RoutedEventArgs e)
    {
        Aktion = KalibrierAktion.Zurueckgesetzt;
        Ergebnis = Kalibrierung.Keine;
        DialogResult = true;
    }
}
