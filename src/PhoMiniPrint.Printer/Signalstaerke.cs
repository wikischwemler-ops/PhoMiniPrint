using Windows.Devices.Enumeration;

namespace PhoMiniPrint.Printer;

/// <summary>Ergebnis einer Signalmessung.</summary>
public sealed record Signalmessung(string Name, int? Dbm, bool? Verbunden)
{
    /// <summary>
    /// Umrechnung dBm → 0–100 % (Planungsannahme): −90 dBm = 0 %, −40 dBm = 100 %.
    /// </summary>
    public int? Prozent => Dbm is null ? null : Math.Clamp((int)Math.Round((Dbm.Value + 90) * 2.0), 0, 100);
}

/// <summary>
/// Liest die Bluetooth-Signalstärke über Windows (Eigenschaft System.Devices.Aep.SignalStrength).
/// Ob Windows den Wert für Bluetooth-Classic-Geräte liefert, wird im Prototyp geprüft (FR-021).
/// </summary>
public static class Signalstaerke
{
    private const string BluetoothClassicProtokoll = "{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}";
    private const string EigenschaftSignal = "System.Devices.Aep.SignalStrength";
    private const string EigenschaftVerbunden = "System.Devices.Aep.IsConnected";

    public static async Task<IReadOnlyList<Signalmessung>> MessenAsync()
    {
        var auswahl = $"System.Devices.Aep.ProtocolId:=\"{BluetoothClassicProtokoll}\"";
        var geraete = await DeviceInformation.FindAllAsync(
            auswahl,
            [EigenschaftSignal, EigenschaftVerbunden],
            DeviceInformationKind.AssociationEndpoint);

        return [.. geraete
            .Where(g => T02Geraet.Geraetenamen.Contains(g.Name, StringComparer.OrdinalIgnoreCase))
            .Select(g => new Signalmessung(
                g.Name,
                g.Properties.TryGetValue(EigenschaftSignal, out var s) && s is not null ? Convert.ToInt32(s) : null,
                g.Properties.TryGetValue(EigenschaftVerbunden, out var v) && v is bool b ? b : null))];
    }
}
