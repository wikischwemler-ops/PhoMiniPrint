using PhoMiniPrint.Printer;

namespace PhoMiniPrint.Core;

/// <summary>Einstellungen für einen Druck (FR-007, FR-012).</summary>
public sealed record Druckeinstellungen(int StaerkeSchwarzProzent, int DruckdichteProzent, Kalibrierung Kalibrierung)
{
    /// <summary>Startwerte (Planungsannahme): Regler je 50 %, keine Kalibrierung.</summary>
    public static Druckeinstellungen Standard { get; } = new(50, 50, Kalibrierung.Keine);
}

/// <summary>Ergebnis der Druckvorbereitung.</summary>
public sealed record Druckdaten(byte[] Auftrag, Rasterbild Bild, int Zeilen);

/// <summary>Prüft das Etikett und erzeugt den fertigen T02-Druckauftrag (F-08).</summary>
public static class Druckvorbereitung
{
    public static Druckdaten Erstellen(Etikett etikett, Druckeinstellungen einstellungen)
    {
        ArgumentNullException.ThrowIfNull(etikett);
        ArgumentNullException.ThrowIfNull(einstellungen);

        var fehler = Layout.Pruefen(etikett);
        if (fehler.Count > 0)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, fehler));
        }

        var bild = Renderer.Rastern(etikett, einstellungen.StaerkeSchwarzProzent, einstellungen.Kalibrierung);
        var stufe = T02Befehle.DichtestufeAusProzent(einstellungen.DruckdichteProzent);
        var auftrag = T02Befehle.Druckauftrag(bild, stufe);
        var zeilen = T02Befehle.Vorlaufzeilen + bild.Hoehe + T02Befehle.Nachlaufzeilen;
        return new Druckdaten(auftrag, bild, zeilen);
    }
}
