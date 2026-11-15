using System.Management;
using System.Text.RegularExpressions;

namespace PhoMiniPrint.Printer;

/// <summary>Ein gekoppelter T02 mit dem zugehörigen Bluetooth-COM-Port.</summary>
public sealed record T02Fundstelle(string Name, string Adresse, string? ComPort);

/// <summary>
/// Findet gekoppelte T02 und den zugehörigen ausgehenden Bluetooth-COM-Port (SPP).
/// Zuordnung über die Bluetooth-Adresse in der Windows-Geräte-ID, nicht über eine feste Portnummer.
/// </summary>
public static partial class T02Suche
{
    public static IReadOnlyList<T02Fundstelle> Suchen()
    {
        var geraete = new List<(string Name, string Adresse)>();
        var ports = new List<(string Port, string Adresse)>();

        using var abfrage = new ManagementObjectSearcher(
            "SELECT Name, PNPDeviceID FROM Win32_PnPEntity WHERE PNPDeviceID LIKE 'BTHENUM%'");
        foreach (var eintrag in abfrage.Get())
        {
            var name = eintrag["Name"] as string ?? string.Empty;
            var id = eintrag["PNPDeviceID"] as string ?? string.Empty;

            var geraet = GeraeteId().Match(id);
            if (geraet.Success && T02Geraet.Geraetenamen.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                geraete.Add((name, geraet.Groups[1].Value.ToUpperInvariant()));
                continue;
            }

            var port = PortName().Match(name);
            var portAdresse = PortAdresse().Match(id);
            if (port.Success && portAdresse.Success)
            {
                ports.Add((port.Groups[1].Value, portAdresse.Groups[1].Value.ToUpperInvariant()));
            }
        }

        return [.. geraete.Select(g => new T02Fundstelle(
            g.Name,
            g.Adresse,
            ports.FirstOrDefault(p => p.Adresse == g.Adresse).Port))];
    }

    [GeneratedRegex(@"^BTHENUM\\DEV_([0-9A-F]{12})\\", RegexOptions.IgnoreCase)]
    private static partial Regex GeraeteId();

    [GeneratedRegex(@"\((COM\d+)\)")]
    private static partial Regex PortName();

    // Ausgehender SPP-Port: ...&<ADRESSE>_C00000000 ; eingehende Ports haben die Adresse 000000000000.
    [GeneratedRegex(@"&([0-9A-F]{12})_C[0-9A-F]+$", RegexOptions.IgnoreCase)]
    private static partial Regex PortAdresse();
}
