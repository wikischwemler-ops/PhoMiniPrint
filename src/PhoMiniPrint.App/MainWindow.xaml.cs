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

    /// <summary>Zuletzt verwendete Formatierung als Vorlage für den nächsten Text.</summary>
    private TextElement _textVorlage = new();

    private readonly Einstellungsspeicher _speicher = new(Einstellungsspeicher.StandardPfad);
    private Einstellungen _einstellungen = Einstellungen.Standard;
    private bool _einstellungenZurueckgesetzt;

    private readonly Druckerverbindung _drucker;
    private Druckerstatus _druckerstatus = Druckerstatus.Unverbunden;

    public MainWindow()
    {
        InitializeComponent();
        Title = $"PhoMiniPrint {Programmversion.Von(typeof(MainWindow).Assembly)}";

        Ansicht.Etikett = _etikett;
        Ansicht.Geaendert += (_, _) => AktualisierenPlanen();
        Ansicht.AuswahlGeaendert += (_, _) => ElementFelderAnzeigen();
        Ansicht.EntfernenGewuenscht += (_, _) => Entfernen_Click(this, new RoutedEventArgs());
        Ansicht.BearbeitenGewuenscht += (_, _) => TextBearbeiten_Click(this, new RoutedEventArgs());
        Ansicht.GroesseGeaendert += (_, _) => SchriftNachfuehren(Ansicht.Auswahl);
        StaerkeRegler.WertGeaendert += (_, _) =>
        {
            AktualisierenPlanen();
            EinstellungenSpeichern();
        };
        DichteRegler.WertGeaendert += (_, _) => EinstellungenSpeichern();
        EinstellungenLaden();

        _drucker = new Druckerverbindung(() => _einstellungen.LetzterDrucker);
        _drucker.StatusGeaendert += (_, status) => Dispatcher.BeginInvoke(() => DruckerstatusAnzeigen(status));
        _drucker.Verbunden += (_, fund) => Dispatcher.BeginInvoke(() => LetztenDruckerMerken(fund));
        Closed += (_, _) => _drucker.Dispose();

        foreach (var feld in new[] { LaengeFeld, RandLinksFeld, RandRechtsFeld, RandObenFeld, RandUntenFeld })
        {
            FeldEinrichten(feld, EtikettUebernehmen);
        }

        FeldEinrichten(BreiteFeld, () => ElementGroesseUebernehmen(breite: true));
        FeldEinrichten(HoeheFeld, () => ElementGroesseUebernehmen(breite: false));

        Animation.Ruhe();
        EtikettFelderAnzeigen();
        ElementFelderAnzeigen();
        Aktualisieren();
        Loaded += (_, _) =>
        {
            // „Öffnen mit“ / Startparameter: erstes Argument als Bild laden.
            var argumente = Environment.GetCommandLineArgs();
            if (argumente.Length > 1 && Bildlader.IstUnterstuetzt(argumente[1]))
            {
                BildLaden(argumente[1]);
            }

            if (_einstellungenZurueckgesetzt)
            {
                MessageBox.Show(this, "Die gespeicherten Einstellungen waren nicht lesbar. Einstellungen wurden zurückgesetzt.",
                    "Einstellungen", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            _drucker.Starten();
        };
    }

    // ---------- Einstellungen (F-10) ----------

    private void EinstellungenLaden()
    {
        var (einstellungen, zurueckgesetzt) = _speicher.Laden();
        _einstellungen = einstellungen;
        _einstellungenZurueckgesetzt = zurueckgesetzt;
        _etikett.Laenge = einstellungen.Laenge;
        _etikett.Raender = einstellungen.Raender;
        StaerkeRegler.Wert = einstellungen.StaerkeSchwarzProzent;
        DichteRegler.Wert = einstellungen.DruckdichteProzent;
    }

    /// <summary>Speichert nach jeder Änderung von Länge, Rändern, Reglern, Kalibrierung oder Drucker.</summary>
    private void EinstellungenSpeichern()
    {
        _einstellungen = _einstellungen with
        {
            Laenge = _etikett.Laenge,
            Raender = _etikett.Raender,
            StaerkeSchwarzProzent = StaerkeRegler.Wert,
            DruckdichteProzent = DichteRegler.Wert,
        };
        if (_einstellungen.IstGueltig() && !_speicher.Speichern(_einstellungen))
        {
            StatusSetzen("Einstellungen konnten nicht gespeichert werden.", "Fehler");
        }
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
        EinstellungenSpeichern();
    }

    private void ElementFelderAnzeigen()
    {
        var e = Ansicht.Auswahl;
        ElementKarte.IsEnabled = e is not null;
        MenueEinpassen.IsEnabled = e is not null;
        MenueTextBearbeiten.IsEnabled = e is TextElement;
        TextBearbeitenKnopf.Visibility = e is TextElement ? Visibility.Visible : Visibility.Collapsed;
        MenueEntfernen.IsEnabled = e is not null;
        _felderSetzen = true;
        BreiteFeld.Text = e is null ? string.Empty : Zahl(e.Breite);
        HoeheFeld.Text = e is null ? string.Empty : Zahl(e.Hoehe);
        _felderSetzen = false;
        ElementTitel.Text = e switch
        {
            BildElement => "BILD",
            TextElement t => $"TEXT · {t.GroessePt.ToString("0.#", Deutsch)} pt",
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
            SchriftNachfuehren(element);
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
        var fehler = Layout.Pruefen(_etikett).ToList();
        if (!_druckerstatus.DruckErlaubt)
        {
            fehler.Add(_druckerstatus.Ueberhitzt
                ? "Der Drucker ist überhitzt – bitte kurz abkühlen lassen."
                : "Kein Papier oder Deckel offen – Drucken ist gesperrt.");
        }

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
        StartKnoepfe.Visibility = _druckt ? Visibility.Collapsed : Visibility.Visible;
        if (!_druckt)
        {
            IllustrationTitel.Text = "Bild oder Text einfügen";
            IllustrationText.Text = "PNG oder JPG hierher ziehen, ein Bild öffnen oder einen Text mit Symbolen schreiben";
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

    // ---------- Text ----------

    private void TextEinfuegen_Click(object sender, RoutedEventArgs e)
    {
        if (_druckt)
        {
            return;
        }

        var dialog = new TextDialog(new TextElement
        {
            Schriftart = _textVorlage.Schriftart,
            GroessePt = _textVorlage.GroessePt,
            Fett = _textVorlage.Fett,
            Ausrichtung = _textVorlage.Ausrichtung,
        })
        { Owner = this };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var text = new TextElement();
        TextUebernehmen(dialog, text);
        var (x, y, b, h) = _etikett.Druckbereich;
        TextPlatzieren(text, new Point(x + (b / 2), y + (h / 2)));
        _etikett.Elemente.Add(text);
        Ansicht.Auswahl = text;
        Ansicht.Focus();
        ElementFelderAnzeigen();
        Aktualisieren();
    }

    private void TextBearbeiten_Click(object sender, RoutedEventArgs e)
    {
        if (_druckt || Ansicht.Auswahl is not TextElement text)
        {
            return;
        }

        var dialog = new TextDialog(text) { Owner = this };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var mitte = new Point(text.X + (text.Breite / 2), text.Y + (text.Hoehe / 2));
        TextUebernehmen(dialog, text);
        TextPlatzieren(text, mitte);
        ElementFelderAnzeigen();
        Ansicht.InvalidateVisual();
        Aktualisieren();
    }

    private void TextUebernehmen(TextDialog dialog, TextElement text)
    {
        text.Text = dialog.Text;
        text.Schriftart = dialog.Schriftart;
        text.GroessePt = dialog.GroessePt;
        text.Fett = dialog.Fett;
        text.Ausrichtung = dialog.Ausrichtung;
        _textVorlage = new TextElement { Schriftart = text.Schriftart, GroessePt = text.GroessePt, Fett = text.Fett, Ausrichtung = text.Ausrichtung };
    }

    /// <summary>Bemisst den Text, setzt ihn mittig auf den Punkt und verkleinert ihn, falls er nicht in den Druckbereich passt.</summary>
    private void TextPlatzieren(TextElement text, Point mitte)
    {
        Renderer.TextBemessen(text);
        var (x, y, b, h) = _etikett.Druckbereich;
        if (text.Breite > b || text.Hoehe > h)
        {
            text.X = x;
            text.Y = y;
            Bearbeitung.GroesseSetzen(text, text.Breite, null, _etikett);
            Renderer.SchriftgroesseAusKasten(text);
        }

        Bearbeitung.Verschieben(text, mitte.X - (text.Breite / 2), mitte.Y - (text.Hoehe / 2), _etikett);
    }

    /// <summary>Nach einer Größenänderung die Schriftgröße in pt anpassen.</summary>
    private void SchriftNachfuehren(Element? element)
    {
        if (element is TextElement text)
        {
            Renderer.SchriftgroesseAusKasten(text);
            ElementFelderAnzeigen();
        }
    }

    // ---------- Element bearbeiten ----------

    private void Drehen_Click(object sender, RoutedEventArgs e)
    {
        if (Ansicht.Auswahl is { } element && sender is FrameworkElement { Tag: string winkel })
        {
            Layout.Drehen(element, (Drehung)int.Parse(winkel, CultureInfo.InvariantCulture));
            Bearbeitung.GroesseSetzen(element, element.Breite, null, _etikett);
            SchriftNachfuehren(element);
            ElementFelderAnzeigen();
            Ansicht.InvalidateVisual();
            Aktualisieren();
        }
    }

    private void Einpassen_Click(object sender, RoutedEventArgs e)
    {
        if (Ansicht.Auswahl is { } element)
        {
            Layout.MaximalEinpassen(_etikett, element);
            SchriftNachfuehren(element);
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

    private async void DruckerSuchen_Click(object sender, RoutedEventArgs e)
    {
        if (!_druckt)
        {
            await _drucker.NeuVerbindenAsync();
        }
    }

    private void LetztenDruckerMerken(Printer.T02Fundstelle fund)
    {
        if (!string.Equals(fund.Adresse, _einstellungen.LetzterDrucker, StringComparison.OrdinalIgnoreCase))
        {
            _einstellungen = _einstellungen with { LetzterDrucker = fund.Adresse };
            EinstellungenSpeichern();
        }
    }

    /// <summary>Statusleiste: Verbindung, Akku % + Balken, Papier, Verbindungsqualität % + Balken (F-02).</summary>
    private void DruckerstatusAnzeigen(Druckerstatus status)
    {
        _druckerstatus = status;
        var name = status.Name ?? "T02";
        var port = status.ComPort is null ? string.Empty : $" ({status.ComPort})";
        (ZustandText.Text, var farbe) = status.Zustand switch
        {
            Verbindungszustand.Bereit => ($"{name} bereit{port}", "Erfolg"),
            Verbindungszustand.Druckt => ($"{name} druckt{port}", "Akzent"),
            Verbindungszustand.Verbindet => ("Verbinde mit dem Drucker …", "TextGedimmt"),
            _ => (status.Name is null ? "Kein gekoppelter T02 gefunden" : $"{name} nicht verbunden – eingeschaltet?", "Fehler"),
        };
        DruckerPunkt.Fill = (Brush)FindResource(farbe);

        var verbunden = status.Zustand is Verbindungszustand.Bereit or Verbindungszustand.Druckt;
        Statuswerte.Visibility = verbunden ? Visibility.Visible : Visibility.Collapsed;
        AkkuBalken.Prozent = status.AkkuProzent;
        AkkuText.Text = status.AkkuProzent is { } akku ? $"{akku} %" : "–";
        AkkuText.Foreground = status.AkkuProzent is < 20 ? (Brush)FindResource("Akzent") : (Brush)FindResource("Text");
        (PapierText.Text, var papierFarbe) = (status.Papier, status.Ueberhitzt) switch
        {
            (_, true) => ("Drucker überhitzt", "Fehler"),
            (Papierzustand.KeinPapier, _) => ("Kein Papier oder Deckel offen", "Fehler"),
            (Papierzustand.Ok, _) => ("Papier ok", "Text"),
            _ => ("–", "TextGedimmt"),
        };
        PapierText.Foreground = (Brush)FindResource(papierFarbe);
        QualitaetBalken.Prozent = status.VerbindungsqualitaetProzent;
        QualitaetText.Text = status.VerbindungsqualitaetProzent is { } q ? $"{q} %" : "–";
        Aktualisieren();
    }

    /// <summary>Kurze Meldung rechts neben dem Druckerstatus.</summary>
    private void StatusSetzen(string text, string farbe)
    {
        StatusText.Text = text;
        StatusText.Foreground = (Brush)FindResource(farbe == "Erfolg" ? "TextGedimmt" : farbe);
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
            daten = Druckvorbereitung.Erstellen(_etikett, new Druckeinstellungen(StaerkeRegler.Wert, DichteRegler.Wert, _einstellungen.Kalibrierung));
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(this, ex.Message, "Drucken", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        await DruckenAsync(daten, _etikett.Laenge);
    }

    /// <summary>Sendet den Auftrag mit Druckanimation (FR-022); liefert true bei Erfolg.</summary>
    private async Task<bool> DruckenAsync(Druckdaten daten, double laengeMm)
    {
        _druckt = true;
        Aktualisieren();
        Animation.Starten();
        IllustrationTitel.Text = "Verbinde mit dem Drucker …";
        IllustrationText.Text = $"{Zahl(laengeMm)} mm · Druckdichte Stufe {Printer.T02Befehle.DichtestufeAusProzent(DichteRegler.Wert)}";

        var fortschritt = new Progress<Druckfortschritt>(FortschrittAnzeigen);
        var ergebnis = await _drucker.DruckenAsync(daten, fortschritt);

        Animation.Beenden(ergebnis.Erfolgreich);
        IllustrationTitel.Text = ergebnis.Erfolgreich ? "Fertig" : "Nicht gedruckt";
        IllustrationText.Text = ergebnis.Meldung;
        StatusSetzen(ergebnis.Meldung, ergebnis.Erfolgreich ? "Erfolg" : "Fehler");

        await Task.Delay(ergebnis.Erfolgreich ? 1500 : 3500);
        _druckt = false;
        Animation.Ruhe();
        Aktualisieren();
        return ergebnis.Erfolgreich;
    }

    // ---------- Kalibrierung (F-09) ----------

    private async void Kalibrieren_Click(object sender, RoutedEventArgs e)
    {
        while (!_druckt)
        {
            var dialog = new KalibrierDialog(_einstellungen.Kalibrierung, druckenMoeglich: true) { Owner = this };
            if (dialog.ShowDialog() != true)
            {
                return;
            }

            switch (dialog.Aktion)
            {
                case KalibrierAktion.TestseiteDrucken:
                    // Testseite mit den bisherigen Faktoren; danach erscheint der Dialog zum Eintragen der Messwerte.
                    var daten = Druckvorbereitung.AusRaster(Kalibrierrechnung.Testseite(_einstellungen.Kalibrierung), DichteRegler.Wert);
                    await DruckenAsync(daten, Kalibrierrechnung.TestseiteLaengeMm);
                    continue;
                case KalibrierAktion.Gespeichert:
                case KalibrierAktion.Zurueckgesetzt:
                    KalibrierungSetzen(dialog.Ergebnis ?? Kalibrierung.Keine);
                    return;
                default:
                    return;
            }
        }
    }

    private void KalibrierungZuruecksetzen_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show(this, "Beide Kalibrierfaktoren auf 1,0 zurücksetzen?", "Kalibrierung",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            KalibrierungSetzen(Kalibrierung.Keine);
        }
    }

    private void KalibrierungSetzen(Kalibrierung kalibrierung)
    {
        _einstellungen = _einstellungen with { Kalibrierung = kalibrierung };
        EinstellungenSpeichern();
        StatusSetzen(
            $"Kalibrierung gespeichert: Länge × {kalibrierung.FaktorLaenge.ToString("0.0000", Deutsch)} · Höhe × {kalibrierung.FaktorHoehe.ToString("0.0000", Deutsch)}",
            "Erfolg");
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
