using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PhoMiniPrint.Core;

namespace PhoMiniPrint.App.Steuerelemente;

/// <summary>
/// Bandansicht (F-03 bis F-05): zeigt das Etikett maßstabsgetreu auf dem 50-mm-Band mit
/// nicht bedruckbaren Streifen, Rändern, mm-Linealen und dem Schwarz-Weiß-Ergebnis.
/// Elemente werden mit der Maus verschoben (Mitte) und skaliert (Ecken, Seitenverhältnis bleibt).
/// </summary>
public sealed class Bandansicht : FrameworkElement
{
    private const double Abstand = 34;
    private const double MaxMassstab = 16;
    private const double AnfasserRadius = 6;

    private static readonly Brush PapierPinsel = Gefroren(new SolidColorBrush(Colors.White));
    private static readonly Brush StreifenPinsel = Schraffur();
    private static readonly Brush AkzentPinsel = Gefroren(new SolidColorBrush(Color.FromRgb(0xF2, 0x6B, 0x1D)));
    private static readonly Brush LinealPinsel = Gefroren(new SolidColorBrush(Color.FromRgb(0x9A, 0xA3, 0xAF)));
    private static readonly Brush SchattenPinsel = Gefroren(new SolidColorBrush(Color.FromArgb(0x70, 0, 0, 0)));
    private static readonly Pen RandStift = Gefroren(new Pen(AkzentPinsel, 1) { DashStyle = new DashStyle([4, 3], 0) });
    private static readonly Pen AuswahlStift = Gefroren(new Pen(AkzentPinsel, 1.5));
    private static readonly Pen UmrissStift = Gefroren(new Pen(LinealPinsel, 1) { DashStyle = DashStyles.Dot });
    private static readonly Pen LinealStift = Gefroren(new Pen(LinealPinsel, 1));

    private Etikett? _etikett;
    private BitmapSource? _vorschau;
    private Element? _auswahl;

    private Ziehen _ziehen = Ziehen.Nichts;
    private Ecke _ecke;
    private Point _startMm;
    private Rect _startRechteck;
    private double _startVerhaeltnis;

    public Bandansicht()
    {
        Focusable = true;
        AllowDrop = true;
        ClipToBounds = true;
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
        SnapsToDevicePixels = true;
    }

    private enum Ziehen
    {
        Nichts,
        Verschieben,
        Skalieren,
    }

    /// <summary>Position, Größe oder Auswahl wurde mit Maus oder Tastatur geändert.</summary>
    public event EventHandler? Geaendert;

    /// <summary>Die Auswahl hat gewechselt.</summary>
    public event EventHandler? AuswahlGeaendert;

    /// <summary>Die Größe eines Elements wurde mit der Maus geändert (beim Loslassen).</summary>
    public event EventHandler? GroesseGeaendert;

    /// <summary>Doppelklick auf ein Element.</summary>
    public event EventHandler? BearbeitenGewuenscht;

    /// <summary>Entf wurde gedrückt, während ein Element ausgewählt ist.</summary>
    public event EventHandler? EntfernenGewuenscht;

    public Etikett? Etikett
    {
        get => _etikett;
        set
        {
            _etikett = value;
            InvalidateVisual();
        }
    }

    public Element? Auswahl
    {
        get => _auswahl;
        set
        {
            if (ReferenceEquals(_auswahl, value))
            {
                return;
            }

            _auswahl = value;
            InvalidateVisual();
            AuswahlGeaendert?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Schwarz-Weiß-Vorschau des bedruckbaren Bereichs (1 Bildpunkt = 1 Druckerpunkt).</summary>
    public void VorschauSetzen(BitmapSource? vorschau)
    {
        _vorschau = vorschau;
        InvalidateVisual();
    }

    // ---------- Maßstab ----------

    private double Massstab
    {
        get
        {
            if (_etikett is null)
            {
                return 1;
            }

            var s = Math.Min((ActualWidth - (2 * Abstand)) / _etikett.Laenge, (ActualHeight - (2 * Abstand)) / Etikett.BandhoeheMm);
            return Math.Clamp(s, 0.5, MaxMassstab);
        }
    }

    private Point Ursprung
    {
        get
        {
            var s = Massstab;
            var laenge = _etikett?.Laenge ?? 0;
            return new Point(
                Math.Round((ActualWidth - (laenge * s)) / 2) + 6,
                Math.Round((ActualHeight - (Etikett.BandhoeheMm * s)) / 2) + 6);
        }
    }

    private Rect AufBildschirm(double x, double y, double b, double h)
    {
        var s = Massstab;
        var u = Ursprung;
        return new Rect(u.X + (x * s), u.Y + (y * s), b * s, h * s);
    }

    private Point InMm(Point bildschirm)
    {
        var s = Massstab;
        var u = Ursprung;
        return new Point((bildschirm.X - u.X) / s, (bildschirm.Y - u.Y) / s);
    }

    // ---------- Zeichnen ----------

    protected override void OnRender(DrawingContext drawingContext)
    {
        ArgumentNullException.ThrowIfNull(drawingContext);
        var dc = drawingContext;
        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
        if (_etikett is not { Laenge: > 0 } etikett)
        {
            return;
        }

        var band = AufBildschirm(0, 0, etikett.Laenge, Etikett.BandhoeheMm);
        var schatten = band;
        schatten.Offset(3, 4);
        dc.DrawRectangle(SchattenPinsel, null, schatten);
        dc.DrawRectangle(PapierPinsel, null, band);

        // Nicht bedruckbare Streifen oben und unten.
        dc.DrawRectangle(StreifenPinsel, null, AufBildschirm(0, 0, etikett.Laenge, Etikett.UnbedruckbarMm));
        dc.DrawRectangle(StreifenPinsel, null, AufBildschirm(0, Etikett.BandhoeheMm - Etikett.UnbedruckbarMm, etikett.Laenge, Etikett.UnbedruckbarMm));

        if (_vorschau is not null)
        {
            dc.DrawImage(_vorschau, AufBildschirm(0, Etikett.UnbedruckbarMm, _vorschau.PixelWidth / 8.0, Etikett.DruckhoeheMm));
        }

        // Ränder.
        var (bx, by, bb, bh) = etikett.Druckbereich;
        if (bb > 0 && bh > 0)
        {
            dc.DrawRectangle(null, RandStift, Ausrichten(AufBildschirm(bx, by, bb, bh)));
        }

        // Umrisse aller Elemente (leicht), damit auch helle Bilder auffindbar sind.
        foreach (var element in etikett.Elemente)
        {
            if (!ReferenceEquals(element, _auswahl))
            {
                dc.DrawRectangle(null, UmrissStift, Ausrichten(AufBildschirm(element.X, element.Y, element.Breite, element.Hoehe)));
            }
        }

        Lineale(dc, etikett, band);

        if (_auswahl is not null && etikett.Elemente.Contains(_auswahl))
        {
            AuswahlZeichnen(dc, _auswahl);
        }
    }

    private void AuswahlZeichnen(DrawingContext dc, Element e)
    {
        var r = Ausrichten(AufBildschirm(e.X, e.Y, e.Breite, e.Hoehe));
        dc.DrawRectangle(null, AuswahlStift, r);
        foreach (var punkt in new[] { r.TopLeft, r.TopRight, r.BottomLeft, r.BottomRight })
        {
            dc.DrawEllipse(Brushes.White, AuswahlStift, punkt, AnfasserRadius, AnfasserRadius);
        }

        var text = Text($"{e.Breite:0.0} × {e.Hoehe:0.0} mm", 11, Brushes.White);
        var hinter = new Rect(r.Left, r.Top - text.Height - 8, text.Width + 10, text.Height + 4);
        if (hinter.Top < 0)
        {
            hinter.Y = r.Bottom + 4;
        }

        dc.DrawRoundedRectangle(AkzentPinsel, null, hinter, 3, 3);
        dc.DrawText(text, new Point(hinter.Left + 5, hinter.Top + 2));
    }

    private void Lineale(DrawingContext dc, Etikett etikett, Rect band)
    {
        var s = Massstab;
        var schritt = s >= 4 ? 1 : s >= 1.5 ? 2 : 5;
        var beschriftung = s >= 3 ? 10 : 20;
        for (var mm = 0; mm <= etikett.Laenge + 1e-6; mm += schritt)
        {
            var x = Math.Round(band.Left + (mm * s)) + 0.5;
            var lang = mm % beschriftung == 0 ? 8 : mm % 5 == 0 ? 5 : 3;
            dc.DrawLine(LinealStift, new Point(x, band.Top - 3 - lang), new Point(x, band.Top - 3));
            if (mm % beschriftung == 0)
            {
                var t = Text(mm.ToString(CultureInfo.CurrentCulture), 10, LinealPinsel);
                dc.DrawText(t, new Point(x - (t.Width / 2), band.Top - 14 - t.Height));
            }
        }

        for (var mm = 0; mm <= Etikett.BandhoeheMm + 1e-6; mm += schritt)
        {
            var y = Math.Round(band.Top + (mm * s)) + 0.5;
            var lang = mm % beschriftung == 0 ? 8 : mm % 5 == 0 ? 5 : 3;
            dc.DrawLine(LinealStift, new Point(band.Left - 3 - lang, y), new Point(band.Left - 3, y));
            if (mm % beschriftung == 0)
            {
                var t = Text(mm.ToString(CultureInfo.CurrentCulture), 10, LinealPinsel);
                dc.DrawText(t, new Point(band.Left - 14 - t.Width, y - (t.Height / 2)));
            }
        }

        var einheit = Text("mm", 10, LinealPinsel);
        dc.DrawText(einheit, new Point(band.Left - 14 - einheit.Width, band.Top - 14 - einheit.Height));
    }

    private FormattedText Text(string text, double groesse, Brush pinsel) =>
        new(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), groesse, pinsel,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

    private static Rect Ausrichten(Rect r) =>
        new(Math.Round(r.X) + 0.5, Math.Round(r.Y) + 0.5, Math.Max(0, Math.Round(r.Width)), Math.Max(0, Math.Round(r.Height)));

    // ---------- Maus und Tastatur ----------

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnMouseLeftButtonDown(e);
        Focus();
        if (_etikett is null)
        {
            return;
        }

        var punkt = e.GetPosition(this);
        if (_auswahl is not null && EckeTreffen(_auswahl, punkt) is { } ecke)
        {
            ZiehenBeginnen(Ziehen.Skalieren, _auswahl, punkt);
            _ecke = ecke;
        }
        else if (ElementTreffen(punkt) is { } element)
        {
            Auswahl = element;
            if (e.ClickCount == 2)
            {
                BearbeitenGewuenscht?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
                return;
            }

            ZiehenBeginnen(Ziehen.Verschieben, element, punkt);
        }
        else
        {
            Auswahl = null;
        }

        e.Handled = true;
    }

    private void ZiehenBeginnen(Ziehen art, Element element, Point punkt)
    {
        _ziehen = art;
        _startMm = InMm(punkt);
        _startRechteck = new Rect(element.X, element.Y, element.Breite, element.Hoehe);
        _startVerhaeltnis = element.Breite / element.Hoehe;
        CaptureMouse();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnMouseMove(e);
        var punkt = e.GetPosition(this);
        if (_etikett is null || _auswahl is null || _ziehen == Ziehen.Nichts || e.LeftButton != MouseButtonState.Pressed)
        {
            Cursor = CursorFuer(punkt);
            return;
        }

        var mm = InMm(punkt);
        if (_ziehen == Ziehen.Verschieben)
        {
            Bearbeitung.Verschieben(_auswahl, _startRechteck.X + (mm.X - _startMm.X), _startRechteck.Y + (mm.Y - _startMm.Y), _etikett);
        }
        else
        {
            Bearbeitung.GroesseAendern(_auswahl, _ecke, mm.X, mm.Y, _etikett, _startVerhaeltnis);
        }

        InvalidateVisual();
        Geaendert?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        var warSkalieren = _ziehen == Ziehen.Skalieren;
        _ziehen = Ziehen.Nichts;
        ReleaseMouseCapture();
        if (warSkalieren)
        {
            GroesseGeaendert?.Invoke(this, EventArgs.Empty);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnKeyDown(e);
        if (_etikett is null || _auswahl is null)
        {
            return;
        }

        // Pfeiltasten: 1 mm, mit Umschalt ein Druckerpunkt (1/8 mm).
        var schritt = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? Layout.PunktMm : 1.0;
        (double dx, double dy) = e.Key switch
        {
            Key.Left => (-schritt, 0.0),
            Key.Right => (schritt, 0.0),
            Key.Up => (0.0, -schritt),
            Key.Down => (0.0, schritt),
            _ => (0.0, 0.0),
        };

        if (e.Key == Key.Delete)
        {
            EntfernenGewuenscht?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        }
        else if (dx != 0 || dy != 0)
        {
            Bearbeitung.Verschieben(_auswahl, _auswahl.X + dx, _auswahl.Y + dy, _etikett);
            InvalidateVisual();
            Geaendert?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        }
    }

    private Element? ElementTreffen(Point punkt)
    {
        if (_etikett is null)
        {
            return null;
        }

        var mm = InMm(punkt);
        for (var i = _etikett.Elemente.Count - 1; i >= 0; i--)
        {
            var e = _etikett.Elemente[i];
            if (mm.X >= e.X && mm.X <= e.Rechts && mm.Y >= e.Y && mm.Y <= e.Unten)
            {
                return e;
            }
        }

        return null;
    }

    private Ecke? EckeTreffen(Element e, Point punkt)
    {
        var r = AufBildschirm(e.X, e.Y, e.Breite, e.Hoehe);
        var grenze = AnfasserRadius + 3;
        (Point Punkt, Ecke Ecke)[] ecken =
        [
            (r.TopLeft, Ecke.ObenLinks),
            (r.TopRight, Ecke.ObenRechts),
            (r.BottomLeft, Ecke.UntenLinks),
            (r.BottomRight, Ecke.UntenRechts),
        ];
        foreach (var (p, ecke) in ecken)
        {
            if ((p - punkt).Length <= grenze)
            {
                return ecke;
            }
        }

        return null;
    }

    private Cursor CursorFuer(Point punkt)
    {
        if (_auswahl is not null && EckeTreffen(_auswahl, punkt) is { } ecke)
        {
            return ecke is Ecke.ObenLinks or Ecke.UntenRechts ? Cursors.SizeNWSE : Cursors.SizeNESW;
        }

        return ElementTreffen(punkt) is not null ? Cursors.SizeAll : Cursors.Arrow;
    }

    // ---------- Hilfen ----------

    private static T Gefroren<T>(T objekt)
        where T : Freezable
    {
        objekt.Freeze();
        return objekt;
    }

    private static Brush Schraffur()
    {
        var gruppe = new DrawingGroup();
        gruppe.Children.Add(new GeometryDrawing(new SolidColorBrush(Color.FromRgb(0xEE, 0xEA, 0xE3)), null, new RectangleGeometry(new Rect(0, 0, 6, 6))));
        gruppe.Children.Add(new GeometryDrawing(null, new Pen(new SolidColorBrush(Color.FromRgb(0xC9, 0xC2, 0xB6)), 1), new LineGeometry(new Point(0, 6), new Point(6, 0))));
        var pinsel = new DrawingBrush(gruppe)
        {
            TileMode = TileMode.Tile,
            Viewport = new Rect(0, 0, 6, 6),
            ViewportUnits = BrushMappingMode.Absolute,
        };
        pinsel.Freeze();
        return pinsel;
    }
}
