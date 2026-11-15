namespace PhoMiniPrint.Printer;

/// <summary>Eine Antwort des Druckers: Kennung (Opcode), erster Wert und alle Nutzdaten.</summary>
public readonly record struct T02Meldung(byte Kennung, byte Wert)
{
    /// <summary>Alle Nutzdaten nach der Kennung (bei 1-Byte-Meldungen nur der Wert).</summary>
    public byte[] Nutzdaten { get; init; } = [Wert];

    /// <summary>Gibt Kennung, Werte und Deutung aus. Die Seriennummer wird ausgeblendet.</summary>
    public override string ToString()
    {
        var daten = Kennung == 0x08 ? "(ausgeblendet)" : Convert.ToHexString(Nutzdaten);
        return $"1A {Kennung:X2} {daten} – {Beschreibung()}";
    }

    /// <summary>Deutsche Deutung bekannter Meldungen (laut TiMini-Print, nicht am Gerät bestätigt).</summary>
    public string Beschreibung() => Kennung switch
    {
        0x03 when Wert == 0xA9 => "überhitzt",
        0x03 when Wert == 0xA8 => "Temperatur normal",
        0x04 when Wert is 0xA1 or 0xA2 or 0xA3 => "Akku niedrig",
        0x04 => $"Akku {Wert} %",
        0x05 when Wert == 0x99 => "Deckel offen",
        0x05 when Wert == 0x98 => "Deckel zu",
        0x06 when Wert == 0x88 => "kein Papier",
        0x06 => "Papier vorhanden",
        0x07 when Nutzdaten.Length == 3 => $"Firmware {Nutzdaten[0]}.{Nutzdaten[1]}.{Nutzdaten[2]}",
        0x08 => "Seriennummer",
        0x17 => $"Chip-Typ {Wert}",
        0x3B => "Fähigkeiten",
        0x09 => $"Abschaltzeit {Wert * 5} min",
        0x35 when Wert == 0x02 => "lädt",
        0x35 => "lädt nicht",
        _ => "unbekannt",
    };
}

/// <summary>
/// Zerlegt den Empfangsstrom des T02 in Meldungen der Form 1A kk Nutzdaten.
/// Die meisten Meldungen haben 1 Byte Nutzdaten; einige sind länger (am T02 gemessen, M1).
/// Bytes, die nicht in dieses Muster passen, werden als unbekannt gesammelt.
/// </summary>
public sealed class T02Antwortleser
{
    /// <summary>Nutzdatenlänge je Kennung, sofern nicht 1 (gemessen am T02, 2026-11-15).</summary>
    private static readonly Dictionary<byte, int> Laengen = new()
    {
        [0x07] = 3,  // Firmware, z. B. 03 02 0A
        [0x08] = 15, // Seriennummer (ASCII)
        [0x3B] = 5,  // Fähigkeiten
    };

    private readonly List<byte> _puffer = [];

    /// <summary>Hängt empfangene Bytes an und liefert alle vollständigen Meldungen.</summary>
    public IReadOnlyList<T02Meldung> Hinzufuegen(ReadOnlySpan<byte> bytes)
    {
        _puffer.AddRange(bytes);
        var meldungen = new List<T02Meldung>();
        var i = 0;
        while (i < _puffer.Count)
        {
            if (_puffer[i] != 0x1A)
            {
                Unbekannt.Add(_puffer[i]);
                i++;
                continue;
            }

            if (i + 1 >= _puffer.Count)
            {
                break;
            }

            var kennung = _puffer[i + 1];
            var laenge = Laengen.GetValueOrDefault(kennung, 1);
            if (i + 1 + laenge >= _puffer.Count)
            {
                break;
            }

            var nutzdaten = _puffer.GetRange(i + 2, laenge).ToArray();
            meldungen.Add(new T02Meldung(kennung, nutzdaten[0]) { Nutzdaten = nutzdaten });
            i += 2 + laenge;
        }

        _puffer.RemoveRange(0, i);
        return meldungen;
    }

    /// <summary>Bytes außerhalb des bekannten Musters (für die Auswertung im Prototyp).</summary>
    public List<byte> Unbekannt { get; } = [];
}
