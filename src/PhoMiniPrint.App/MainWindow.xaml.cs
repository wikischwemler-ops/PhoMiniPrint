using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using PhoMiniPrint.App.Steuerelemente;
using PhoMiniPrint.Core;

namespace PhoMiniPrint.App;

/// <summary>Hauptfenster: Bandansicht in der Mitte, Einstellungen rechts, Status unten (M3).</summary>
public partial class MainWindow : Window
{
    private static readonly CultureInfo Deutsch = CultureInfo.GetCultureInfo("de-DE");

    private readonly Etikett _etikett = new();
    private bool _aktualisierungGeplant;
    private bool _druckt;
    private bool _felderSetzen;

    public MainWindow()
    {
        InitializeComponent();
        Title = $"PhoMiniPrint {Programmversion.Von(typeof(MainWindow).Assembly)}";

        Ansicht.Etikett = _etikett;
        Ansicht.Geaendert += (_, _) => AktualisierenPlanen();
        Ansicht.AuswahlGeaendert += (_, _) => ElementFelderAnzeigen();
        Ansicht.EntfernenGewuenscht += (_, _) => Entfernen_Click(this, new RoutedEventArgs());
        StaerkeRegler.WertGeaendert += (_, _) => AktualisierenPlanen();

        foreach (var feld in new[] { LaengeFeld, RandLinksFeld, RandRechtsFeld, RandObenFeld, RandUntenFeld })
        {
            FeldEinrichten(feld, EtikettUebernehmen);
        }

        FeldEinrichten(BreiteFeld, () => ElementGroesseUebernehmen(breite: true));
        FeldEinrichten(HoeheFeld, () => ElementGroesseUebernehmen(breite: false));

        Animation.Ruhe();
        EtikettFelderAnzeigen();
        Aktualisieren();
        Loaded += async (_, _) =>
        {
            // „Öffnen mit“ / Startparameter: erstes Argument als Bild laden.
            var argumente = Environment.GetCommandLineArgs();
            if (argumente.Length > 1 && Bildlader.IstUnterstuetzt(argumente[1]))
            {
                BildLaden(argumente[1]);
            }

            await DruckerSuchenAsync();
        };
    }

    // ---------- Etikett und Felder ----------

    private static void FeldEinrichten(TextBox feld, Action uebernehmen)
    {
        feld.LostKeyboardFocus += (_, _) => uebernehmen();
        feld.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter)
            {
                uebernehmen();
                feld.SelectAll();
            }
        };
        feld.GotKeyboardFocus += (_, _) => feld.SelectAll();
    }

    /// <summary>Liest eine Zahl in mm; Komma und Punkt sind erlaubt.</summary>
    private static bool ZahlLesen(TextBox feld, out double wert)
    {
        var text = feld.Text.Trim().Replace("mm", string.Empty, StringComparison.OrdinalIgnoreCase).Trim().Replace('.', ',');
        var ok = double.TryParse(text, NumberStyles.Float, Deutsch, out wert) && wert >= 0 && wert <= 10000;
        feld.BorderBrush = ok ? (Brush)Application.Current.FindResource("Linie") : (Brush)Application.Current.FindResource("Fehler");
        return ok;
    }

    private static string Zahl(double mm) => mm.ToString("0.###", Deutsch);

    private void EtikettFelderAnzeigen()
    {
        _felderSetzen = true;
        LaengeFeld.Text = Zahl(_etikett.Laenge);
        RandLinksFeld.Text = Zahl(_etikett.Raender.Links);
        RandRechtsFeld.Text = Zahl(_etikett.Raender.Rechts);
        RandObenFeld.Text = Zahl(_etikett.Raender.Oben);
        RandUntenFeld.Text = Zahl(_etikett.Raender.Unten);
        _felderSetzen = false;
    }

    private void EtikettUebernehmen()
    {
        if (_felderSetzen)
        {
            return;
        }

        var ok = ZahlLesen(LaengeFeld, out var laenge) & ZahlLesen(RandLinksFeld, out var links)
            & ZahlLesen(RandRechtsFeld, out var rechts) & ZahlLesen(RandObenFeld, out var oben) & ZahlLesen(RandUntenFeld, out var unten);
        if (!ok)
        {
            Aktualisieren();
            return;
        }

        _etikett.Laenge = Layout.AufPunktRunden(laenge);
        _etikett.Raender = new Raender(Layout.AufPunktRunden(links), Layout.AufPunktRunden(rechts), Layout.AufPunktRunden(oben), Layout.AufPunktRunden(unten));
        EtikettFelderAnzeigen();
        Ansicht.InvalidateVisual();
        Aktualisieren();
    }

    private void ElementFelderAnzeigen()
    {
        var e = Ansicht.Auswahl;
        ElementKarte.IsEnabled = e is not null;
        MenueEinpassen.IsEnabled = e is BildElement;
        MenueEntfernen.IsEnabled = e is not null;
        _felderSetzen = true;
        BreiteFeld.Text = e is null ? string.Empty : Zahl(e.Breite);
        HoeheFeld.Text = e is null ? string.Empty : Zahl(e.Hoehe);
        _felderSetzen = false;
        ElementTitel.Text = e switch
        {
            BildElement => "BILD",
            TextElement => "TEXT",
            _ => "ELEMENT (keins ausgewählt)",
        };
        Drehen0.IsChecked = e?.Drehung == Drehung.Grad0;
        Drehen90.IsChecked = e?.Drehung == Drehung.Grad90;
        Drehen180.IsChecked = e?.Drehung == Drehung.Grad180;
        Drehen270.IsChecked = e?.Drehung == Drehung.Grad270;
    }

    private void ElementGroesseUebernehmen(bool breite)
    {
        if (_felderSetzen || Ansicht.Auswahl is not { } element)
        {
            return;
        }

        var feld = breite ? BreiteFeld : HoeheFeld;
        if (ZahlLesen(feld, out var wert) && wert > 0)
        {
            Bearbeitung.GroesseSetzen(element, breite ? wert : null, breite ? null : wert, _etikett);
        }

        ElementFelderAnzeigen();
        Ansicht.InvalidateVisual();
        Aktualisieren();
    }

    // ---------- Vorschau und Prüfung ----------

    /// <summary>Fasst schnelle Änderungen (Ziehen mit der Maus) zu einer Aktualisierung zusammen.</summary>
    private void AktualisierenPlanen()
    {
        if (_aktualisierungGeplant)
        {
            return;
        }

        _aktualisierungGeplant = true;
        Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
        {
            _aktualisierungGeplant = false;
            Aktualisieren();
        });
    }

    private void Aktualisieren()
    {
        var fehler = Layout.Pruefen(_etikett);
        FehlerText.Text = string.Join(Environment.NewLine, fehler);
        FehlerText.Visibility = fehler.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        DruckenKnopf.IsEnabled = fehler.Count == 0 && _etikett.Elemente.Count > 0 && !_druckt;
        MenueDrucken.IsEnabled = DruckenKnopf.IsEnabled;

        Ansicht.VorschauSetzen(_etikett.Laenge > 0 ? Vorschau.Ansicht(Renderer.Rastern(_etikett, StaerkeRegler.Wert)) : null);
        if (!_felderSetzen && Ansicht.Auswahl is { } e && !BreiteFeld.IsKeyboardFocused && !HoeheFeld.IsKeyboardFocused)
        {
            _felderSetzen = true;
            BreiteFeld.Text = Zahl(e.Breite);
            HoeheFeld.Text = Zahl(e.Hoehe);
            _felderSetzen = false;
        }

        IllustrationZeigen();
    }

    private void IllustrationZeigen()
    {
        var leer = _etikett.Elemente.Count == 0;
        Illustration.Visibility = leer || _druckt ? Visibility.Visible : Visibility.Collapsed;
        StartKnopf.Visibility = _druckt ? Visibility.Collapsed : Visibility.Visible;
        if (!_druckt)
        {
            IllustrationTitel.Text = "Bild oder Text einfügen";
            IllustrationText.Text = "PNG oder JPG hierher ziehen oder über „Bild einfügen“ öffnen";
        }
    }

    // ---------- Bild einfügen ----------

    private void BildEinfuegen_Click(object sender, RoutedEventArgs e)
    {
        if (_druckt)
        {
            return;
        }

        var dialog = new OpenFileDialog { Title = "Bild einfügen", Filter = Bildlader.Dateifilter };
        if (dialog.ShowDialog(this) == true)
        {
            BildLaden(dialog.FileName);
        }
    }

    private void Datei_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = !_druckt && e.Data.GetData(DataFormats.FileDrop) is string[] { Length: > 0 } dateien && Bildlader.IstUnterstuetzt(dateien[0])
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void Datei_Drop(object sender, DragEventArgs e)
    {
        if (!_druckt && e.Data.GetData(DataFormats.FileDrop) is string[] { Length: > 0 } dateien)
        {
            BildLaden(dateien[0]);
        }
    }

    private void BildLaden(string pfad)
    {
        try
        {
            var bild = new BildElement(Bildlader.Laden(pfad));
            Layout.MaximalEinpassen(_etikett, bild);
            _etikett.Elemente.Add(bild);
            Ansicht.Auswahl = bild;
            Ansicht.Focus();
            Aktualisieren();
        }
        catch (BildFehler ex)
        {
            MessageBox.Show(this, ex.Message, "Bild einfügen", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // ---------- Element bearbeiten ----------

    private void Drehen_Click(object sender, RoutedEventArgs e)
    {
        if (Ansicht.Auswahl is BildElement bild && sender is FrameworkElement { Tag: string winkel })
        {
            Layout.Drehen(bild, (Drehung)int.Parse(winkel, CultureInfo.InvariantCulture));
            Bearbeitung.Verschieben(bild, bild.X, bild.Y, _etikett);
            ElementFelderAnzeigen();
            Ansicht.InvalidateVisual();
            Aktualisieren();
        }
    }

    private void Einpassen_Click(object sender, RoutedEventArgs e)
    {
        if (Ansicht.Auswahl is BildElement bild)
        {
            Layout.MaximalEinpassen(_etikett, bild);
            ElementFelderAnzeigen();
            Ansicht.InvalidateVisual();
            Aktualisieren();
        }
    }

    private void Entfernen_Click(object sender, RoutedEventArgs e)
    {
        if (Ansicht.Auswahl is { } element && !_druckt)
        {
            _etikett.Elemente.Remove(element);
            Ansicht.Auswahl = _etikett.Elemente.LastOrDefault();
            Ansicht.InvalidateVisual();
            Aktualisieren();
        }
    }

    private void Beenden_Click(object sender, RoutedEventArgs e) => Close();

    // ---------- Drucker ----------

    private async void DruckerSuchen_Click(object sender, RoutedEventArgs e) => await DruckerSuchenAsync();

    private async Task DruckerSuchenAsync()
    {
        StatusText.Text = "Drucker: wird gesucht …";
        var drucker = await Task.Run(Druckdienst.DruckerSuchen);
        if (drucker is null)
        {
            StatusSetzen("Drucker: kein gekoppelter T02 gefunden", "Fehler");
        }
        else
        {
            StatusSetzen($"Drucker: {drucker.Name} gekoppelt ({drucker.ComPort})", "Erfolg");
        }
    }

    private void StatusSetzen(string text, string farbe)
    {
        StatusText.Text = text;
        DruckerPunkt.Fill = (Brush)FindResource(farbe);
    }

    private async void Drucken_Click(object sender, RoutedEventArgs e)
    {
        if (_druckt || !DruckenKnopf.IsEnabled)
        {
            return;
        }

        Druckdaten daten;
        try
        {
            daten = Druckvorbereitung.Erstellen(_etikett, new Druckeinstellungen(StaerkeRegler.Wert, DichteRegler.Wert, Kalibrierung.Keine));
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(this, ex.Message, "Drucken", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _druckt = true;
        Aktualisieren();
        Animation.Starten();
        IllustrationTitel.Text = "Verbinde mit dem Drucker …";
        IllustrationText.Text = $"{Zahl(_etikett.Laenge)} mm · Druckdichte Stufe {Printer.T02Befehle.DichtestufeAusProzent(DichteRegler.Wert)}";

        var fortschritt = new Progress<Druckfortschritt>(FortschrittAnzeigen);
        var ergebnis = await Task.Run(() => Druckdienst.Drucken(daten, fortschritt));

        Animation.Beenden(ergebnis.Erfolgreich);
        IllustrationTitel.Text = ergebnis.Erfolgreich ? "Fertig" : "Nicht gedruckt";
        IllustrationText.Text = ergebnis.Meldung;
        StatusSetzen($"Drucker: {ergebnis.Meldung}", ergebnis.Erfolgreich ? "Erfolg" : "Fehler");

        await Task.Delay(ergebnis.Erfolgreich ? 1500 : 3500);
        _druckt = false;
        Animation.Ruhe();
        Aktualisieren();
    }

    private void FortschrittAnzeigen(Druckfortschritt f)
    {
        switch (f.Phase)
        {
            case Druckphase.Senden:
                IllustrationTitel.Text = $"Übertrage … {f.Anteil * 100:0} %";
                Animation.Uebertragung(f.Anteil);
                break;
            case Druckphase.Drucken:
                IllustrationTitel.Text = "Drucke …";
                Animation.Drucken();
                break;
        }
    }
}
