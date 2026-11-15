using System.Windows.Input;

namespace PhoMiniPrint.App.Steuerelemente;

/// <summary>Befehle für Tastenkürzel im Hauptfenster.</summary>
public static class Befehle
{
    public static RoutedUICommand BildEinfuegen { get; } = new("Bild einfügen", nameof(BildEinfuegen), typeof(Befehle));

    public static RoutedUICommand TextEinfuegen { get; } = new("Text einfügen", nameof(TextEinfuegen), typeof(Befehle));

    public static RoutedUICommand Drucken { get; } = new("Drucken", nameof(Drucken), typeof(Befehle));
}
