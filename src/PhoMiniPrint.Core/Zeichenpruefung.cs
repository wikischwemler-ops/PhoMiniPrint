using System.Globalization;
using System.Text;
using System.Windows.Media;

namespace PhoMiniPrint.Core;

/// <summary>
/// Findet Zeichen, die keine installierte Schrift darstellen kann (F-06, Fehlerfall):
/// Statt eines leeren Kästchens wird gewarnt.
/// </summary>
public static class Zeichenpruefung
{
    /// <summary>Ersatzschriften, auf die Windows/WPF für Symbole, Emojis und andere Schriftsysteme ausweicht.</summary>
    public static IReadOnlyList<string> Ersatzschriften { get; } =
    [
        "Segoe UI", "Segoe UI Symbol", "Segoe UI Emoji", "Segoe UI Historic", "Cambria Math",
        "Microsoft YaHei", "Yu Gothic", "Malgun Gothic", "Nirmala UI", "Ebrima", "Gadugi",
        "Leelawadee UI", "Myanmar Text", "Mongolian Baiti", "Microsoft Himalaya", "Javanese Text", "Sylfaen",
    ];

    /// <summary>Häufige Symbole und Emojis für die Symbolauswahl (F-06).</summary>
    public static IReadOnlyList<string> Symbole { get; } =
    [
        "°", "²", "³", "€", "§", "±", "×", "÷", "µ", "Ø", "½", "¼",
        "✓", "✗", "★", "☆", "♥", "→", "←", "↑", "↓", "☎", "⚠", "☀", "♪", "✂", "✉", "☺", "©", "®", "™", "•",
        "😀", "😊", "👍", "❤", "⭐", "🎉", "🎂", "🎁", "📞", "📦", "🔥", "✅", "❌", "⚡", "❄", "🌡", "🍎", "🏠", "⏰", "📌",
    ];

    /// <summary>Gibt die Zeichen des Textes zurück, die weder die gewählte Schrift noch eine Ersatzschrift enthält.</summary>
    public static IReadOnlyList<string> NichtDruckbar(string text, string schriftart)
    {
        ArgumentNullException.ThrowIfNull(text);
        var schriften = new List<GlyphTypeface>();
        foreach (var name in Ersatzschriften.Prepend(schriftart).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (new Typeface(name).TryGetGlyphTypeface(out var glyphen))
            {
                schriften.Add(glyphen);
            }
        }

        var fehlend = new List<string>();
        foreach (var zeichen in text.EnumerateRunes())
        {
            if (IstSteuerzeichen(zeichen))
            {
                continue;
            }

            if (!schriften.Any(s => s.CharacterToGlyphMap.ContainsKey(zeichen.Value)))
            {
                var anzeige = zeichen.ToString();
                if (!fehlend.Contains(anzeige))
                {
                    fehlend.Add(anzeige);
                }
            }
        }

        return fehlend;
    }

    /// <summary>Leerraum, Steuer- und Verbindungszeichen (z. B. Emoji-Varianten) werden nicht gedruckt und nicht geprüft.</summary>
    private static bool IstSteuerzeichen(Rune zeichen) =>
        Rune.IsWhiteSpace(zeichen)
        || Rune.IsControl(zeichen)
        || zeichen.Value is 0x200D or 0xFE0E or 0xFE0F or 0x20E3
        || Rune.GetUnicodeCategory(zeichen) is UnicodeCategory.Format;
}
