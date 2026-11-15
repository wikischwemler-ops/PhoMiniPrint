namespace PhoMiniPrint.Printer;

/// <summary>
/// Schwarz-Weiß-Rasterbild in Druckerpunkten, 1 Bit pro Punkt.
/// Breite = quer zum Band, Höhe = Zeilen entlang des Bandes.
/// Gesetztes Bit = schwarzer Punkt. Höchstwertiges Bit = linker Punkt.
/// </summary>
public sealed class Rasterbild
{
    private readonly byte[] _daten;

    public Rasterbild(int breite, int hoehe)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(breite);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(hoehe);
        Breite = breite;
        Hoehe = hoehe;
        BytesProZeile = (breite + 7) / 8;
        _daten = new byte[BytesProZeile * hoehe];
    }

    public int Breite { get; }
    public int Hoehe { get; }
    public int BytesProZeile { get; }

    /// <summary>Gepackte Rasterdaten, Zeile für Zeile.</summary>
    public ReadOnlySpan<byte> Daten => _daten;

    public bool this[int x, int y]
    {
        get
        {
            Pruefen(x, y);
            return (_daten[(y * BytesProZeile) + (x >> 3)] & (0x80 >> (x & 7))) != 0;
        }
        set
        {
            Pruefen(x, y);
            var index = (y * BytesProZeile) + (x >> 3);
            var maske = (byte)(0x80 >> (x & 7));
            _daten[index] = value ? (byte)(_daten[index] | maske) : (byte)(_daten[index] & ~maske);
        }
    }

    /// <summary>Setzt ein gefülltes Rechteck (Grenzen werden auf das Bild beschnitten).</summary>
    public void Rechteck(int x, int y, int breite, int hoehe)
    {
        var x0 = Math.Max(0, x);
        var y0 = Math.Max(0, y);
        var x1 = Math.Min(Breite, x + breite);
        var y1 = Math.Min(Hoehe, y + hoehe);
        for (var yy = y0; yy < y1; yy++)
        {
            for (var xx = x0; xx < x1; xx++)
            {
                this[xx, yy] = true;
            }
        }
    }

    /// <summary>Zeichnet einen Rahmen mit der angegebenen Strichstärke.</summary>
    public void Rahmen(int x, int y, int breite, int hoehe, int staerke)
    {
        Rechteck(x, y, breite, staerke);
        Rechteck(x, y + hoehe - staerke, breite, staerke);
        Rechteck(x, y, staerke, hoehe);
        Rechteck(x + breite - staerke, y, staerke, hoehe);
    }

    private void Pruefen(int x, int y)
    {
        if ((uint)x >= (uint)Breite || (uint)y >= (uint)Hoehe)
        {
            throw new ArgumentOutOfRangeException(nameof(x), $"Punkt ({x}, {y}) liegt außerhalb des Bildes {Breite} × {Hoehe}.");
        }
    }
}
