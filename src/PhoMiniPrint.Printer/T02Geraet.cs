namespace PhoMiniPrint.Printer;

/// <summary>
/// Feste Eckdaten des Phomemo T02.
/// Quelle: Analyse von TiMini-Print (Apache-2.0), Profil "phomemo_t02".
/// Noch nicht am Gerät bestätigt – wird in Meilenstein M1 geprüft.
/// </summary>
public static class T02Geraet
{
    /// <summary>Druckerpunkte pro Millimeter (203 dpi).</summary>
    public const int PunkteProMm = 8;

    /// <summary>Bedruckbare Breite quer zum Band in Druckerpunkten (≈ 48 mm).</summary>
    public const int DruckbreitePunkte = 384;

    /// <summary>Gerätenamen, unter denen der T02 bzw. baugleiche Modelle erscheinen.</summary>
    public static readonly IReadOnlyList<string> Geraetenamen = ["T02", "T02E", "C02E"];
}
