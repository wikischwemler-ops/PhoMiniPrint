namespace PhoMiniPrint.Printer;

/// <summary>
/// Befehle für den Phomemo T02.
/// Abgeleitet aus der Analyse von TiMini-Print (Apache-2.0), Familie "Phomemo ESC", Variante "t02".
/// Eigene Umsetzung; noch nicht am Gerät bestätigt (Meilenstein M1).
/// </summary>
public static class T02Befehle
{
    /// <summary>Leerzeilen, die vor dem Bild eingefügt werden (aus dem T02-Rezept).</summary>
    public const int Vorlaufzeilen = 4;

    /// <summary>
    /// Leerzeilen nach dem Bild, damit das Ende des Ausdrucks über die Abreißkante hinauskommt.
    /// Ohne Nachlauf fehlte im M1-Test das Ende des Ausdrucks; 12 mm reichten, 7 mm laut Benutzer ausreichend (2026-11-15).
    /// </summary>
    public const int Nachlaufzeilen = 7 * T02Geraet.PunkteProMm;

    /// <summary>Höchstzahl Zeilen je Rasterblock (16-Bit-Feld im GS-v-0-Befehl).</summary>
    public const int MaxZeilenProBlock = 0xFFFF;

    public static byte[] Initialisieren() => [0x1B, 0x40];

    /// <summary>Medium: Endlospapier bzw. Etikett.</summary>
    public static byte[] Medium() => [0x1F, 0x11, 0x0B];

    /// <summary>Unkomprimierte Rasterdaten.</summary>
    public static byte[] Unkomprimiert() => [0x1F, 0x11, 0x35, 0x00];

    /// <summary>Papiervorschub nach dem Druck.</summary>
    public static byte[] Vorschub() => [0x1B, 0x64, 0x02];

    /// <summary>
    /// Druckdichte in 4 Stufen. Stufe 1–3 setzen nur die Dichte (1, 2, 4),
    /// Stufe 4 zusätzlich einen höheren Koeffizienten.
    /// </summary>
    public static byte[] Druckdichte(int stufe)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(stufe, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(stufe, 4);
        var (dichte, koeffizient) = stufe switch
        {
            1 => (1, 100),
            2 => (2, 100),
            3 => (4, 100),
            _ => (4, 150),
        };
        return [0x1F, 0x11, 0x02, (byte)dichte, 0x1F, 0x11, 0x37, (byte)koeffizient];
    }

    /// <summary>Bildet den Regler 0–100 % auf die 4 Dichtestufen ab (Planungsannahme).</summary>
    public static int DichtestufeAusProzent(int prozent)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(prozent, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(prozent, 100);
        return prozent switch
        {
            < 25 => 1,
            < 50 => 2,
            < 75 => 3,
            _ => 4,
        };
    }

    /// <summary>Rasterbild als GS-v-0-Blöcke (1D 76 30 00 xL xH yL yH Daten).</summary>
    public static byte[] Raster(Rasterbild bild)
    {
        ArgumentNullException.ThrowIfNull(bild);
        var ausgabe = new List<byte>(bild.Daten.Length + 16);
        var daten = bild.Daten;
        for (var start = 0; start < bild.Hoehe; start += MaxZeilenProBlock)
        {
            var zeilen = Math.Min(MaxZeilenProBlock, bild.Hoehe - start);
            ausgabe.AddRange([
                0x1D, 0x76, 0x30, 0x00,
                (byte)(bild.BytesProZeile & 0xFF), (byte)(bild.BytesProZeile >> 8),
                (byte)(zeilen & 0xFF), (byte)(zeilen >> 8),
            ]);
            ausgabe.AddRange(daten.Slice(start * bild.BytesProZeile, zeilen * bild.BytesProZeile));
        }

        return [.. ausgabe];
    }

    /// <summary>
    /// Vollständiger Druckauftrag: Initialisieren, Dichte, Medium, unkomprimiert,
    /// Bild mit 4 Vorlaufzeilen und Nachlauf-Leerzeilen, zweimal Vorschub.
    /// </summary>
    public static byte[] Druckauftrag(Rasterbild bild, int dichtestufe)
    {
        ArgumentNullException.ThrowIfNull(bild);
        if (bild.Breite > T02Geraet.DruckbreitePunkte)
        {
            throw new ArgumentException($"Das Bild ist {bild.Breite} Punkte breit, der T02 druckt höchstens {T02Geraet.DruckbreitePunkte}.", nameof(bild));
        }

        var mitVorlauf = new Rasterbild(bild.Breite, Vorlaufzeilen + bild.Hoehe + Nachlaufzeilen);
        for (var y = 0; y < bild.Hoehe; y++)
        {
            for (var x = 0; x < bild.Breite; x++)
            {
                if (bild[x, y])
                {
                    mitVorlauf[x, y + Vorlaufzeilen] = true;
                }
            }
        }

        return [
            .. Initialisieren(),
            .. Druckdichte(dichtestufe),
            .. Medium(),
            .. Unkomprimiert(),
            .. Raster(mitVorlauf),
            .. Vorschub(),
            .. Vorschub(),
        ];
    }

    /// <summary>Abfrage Akkustand → Antwort 1A 04 (am T02 gemessen, M1).</summary>
    public static byte[] AbfrageAkku() => [0x1F, 0x11, 0x08];

    /// <summary>Abfrage Papier → Antwort 1A 06 (am T02 gemessen, M1).</summary>
    public static byte[] AbfragePapier() => [0x1F, 0x11, 0x11];

    /// <summary>Abfrage Deckel → Antwort 1A 05 (am T02 gemessen, M1).</summary>
    public static byte[] AbfrageDeckel() => [0x1F, 0x11, 0x12];

    /// <summary>
    /// Statusabfragen (1F 11 xx) mit der am T02 beobachteten Antwort (M1, 2026-11-15).
    /// Abweichend von TiMini-Print liefert 08 den Akku und 09 die Seriennummer.
    /// </summary>
    public static IReadOnlyDictionary<string, byte[]> Abfragen { get; } = new Dictionary<string, byte[]>
    {
        ["0E → Fähigkeiten/Abschaltzeit"] = [0x1F, 0x11, 0x0E],
        ["07 → Firmware"] = [0x1F, 0x11, 0x07],
        ["08 → Akku"] = AbfrageAkku(),
        ["09 → Seriennummer"] = [0x1F, 0x11, 0x09],
        ["11 → Papier"] = AbfragePapier(),
        ["12 → Deckel"] = AbfrageDeckel(),
        ["38 → Chip-Typ"] = [0x1F, 0x11, 0x38],
    };
}
