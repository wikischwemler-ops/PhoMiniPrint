using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace PhoMiniPrint.App.Steuerelemente;

/// <summary>
/// Drucker-Illustration mit Druckanimation (FR-022): Funkwellen laufen während der Übertragung
/// vom PC zum Drucker, der Papierstreifen kommt passend zum Fortschritt heraus.
/// Im Ruhezustand zeigt sie die Startillustration.
/// </summary>
public partial class DruckerAnimation : UserControl
{
    /// <summary>So weit liegt das Papier im Gehäuse, bevor der Druck beginnt.</summary>
    private const double PapierEingezogen = 210;

    private static readonly Brush LedBereit = Gefroren(Color.FromRgb(0x3E, 0xCF, 0x8E));
    private static readonly Brush LedAktiv = Gefroren(Color.FromRgb(0xF2, 0x6B, 0x1D));
    private static readonly Brush LedFehler = Gefroren(Color.FromRgb(0xFF, 0x4D, 0x4D));

    private readonly Path[] _wellen;
    private readonly double[] _ruheDeckkraft = [1, 0.75, 0.5];
    private bool _wellenLaufen;

    public DruckerAnimation()
    {
        InitializeComponent();
        _wellen = [Welle1, Welle2, Welle3];
    }

    /// <summary>Startillustration: Papier draußen, Wellen ruhig, LED grün.</summary>
    public void Ruhe()
    {
        WellenAnhalten();
        PapierSetzen(1, sofort: true);
        Led.Fill = LedBereit;
    }

    /// <summary>Neuer Druck: Papier eingezogen, Wellen laufen.</summary>
    public void Starten()
    {
        PapierSetzen(0, sofort: true);
        Led.Fill = LedAktiv;
        WellenStarten();
    }

    /// <summary>Übertragungsfortschritt 0–1: Wellen laufen, Papier folgt (der T02 druckt beim Empfangen).</summary>
    public void Uebertragung(double anteil)
    {
        WellenStarten();
        PapierSetzen(Math.Clamp(anteil, 0, 1) * 0.9, sofort: false);
    }

    /// <summary>Übertragung beendet, der Drucker druckt die letzten Zeilen.</summary>
    public void Drucken()
    {
        WellenAnhalten();
        PapierSetzen(0.95, sofort: false);
    }

    /// <summary>Ende: Papier ganz heraus (Erfolg) oder rote LED (Fehler).</summary>
    public void Beenden(bool erfolgreich)
    {
        WellenAnhalten();
        if (erfolgreich)
        {
            PapierSetzen(1, sofort: false);
            Led.Fill = LedBereit;
        }
        else
        {
            Led.Fill = LedFehler;
        }
    }

    private void PapierSetzen(double anteil, bool sofort)
    {
        var ziel = (1 - anteil) * PapierEingezogen;
        if (sofort || !SystemParameters.ClientAreaAnimation)
        {
            PapierVersatz.BeginAnimation(TranslateTransform.YProperty, null);
            PapierVersatz.Y = ziel;
            return;
        }

        var animation = new DoubleAnimation(ziel, TimeSpan.FromMilliseconds(350))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
        };
        PapierVersatz.BeginAnimation(TranslateTransform.YProperty, animation, HandoffBehavior.SnapshotAndReplace);
    }

    private void WellenStarten()
    {
        if (_wellenLaufen)
        {
            return;
        }

        _wellenLaufen = true;
        // Äußere Welle (am PC) zuerst, dann zum Drucker hin.
        for (var i = 0; i < _wellen.Length; i++)
        {
            var animation = new DoubleAnimationUsingKeyFrames
            {
                Duration = TimeSpan.FromMilliseconds(900),
                RepeatBehavior = RepeatBehavior.Forever,
                BeginTime = TimeSpan.FromMilliseconds((_wellen.Length - 1 - i) * 220),
            };
            animation.KeyFrames.Add(new LinearDoubleKeyFrame(0.15, KeyTime.FromPercent(0)));
            animation.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromPercent(0.3)));
            animation.KeyFrames.Add(new LinearDoubleKeyFrame(0.15, KeyTime.FromPercent(1)));
            _wellen[i].Opacity = 0.15;
            _wellen[i].BeginAnimation(OpacityProperty, animation);
        }
    }

    private void WellenAnhalten()
    {
        _wellenLaufen = false;
        for (var i = 0; i < _wellen.Length; i++)
        {
            _wellen[i].BeginAnimation(OpacityProperty, null);
            _wellen[i].Opacity = _ruheDeckkraft[i];
        }
    }

    private static SolidColorBrush Gefroren(Color farbe)
    {
        var pinsel = new SolidColorBrush(farbe);
        pinsel.Freeze();
        return pinsel;
    }
}
