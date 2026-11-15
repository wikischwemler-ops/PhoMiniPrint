using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;
using PhoMiniPrint.Core;

namespace PhoMiniPrint.App;

/// <summary>Info-Fenster mit Version (MAJOR.MINOR.PATCH), Build-Datum und Lizenzhinweisen (F-11, FR-018).</summary>
public partial class InfoFenster : Window
{
    public InfoFenster()
    {
        InitializeComponent();
        var assembly = typeof(InfoFenster).Assembly;
        var version = Programmversion.Von(assembly);
        VersionText.Text = $"Version {version}";
        VersionWert.Text = version;
        DatumWert.Text = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == "BuildDatum")?.Value is { } datum && DateTime.TryParse(datum, out var d)
            ? d.ToString("d. MMMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("de-DE"))
            : "–";
        EinstellungenWert.Text = Einstellungsspeicher.StandardPfad;

        var notice = Path.Combine(AppContext.BaseDirectory, "NOTICE.txt");
        HinweisText.Text = File.Exists(notice)
            ? File.ReadAllText(notice)
            : "Druckprotokoll abgeleitet aus TiMini-Print (Apache License 2.0), https://github.com/Dejniel/TiMini-Print";
    }

    private void Link_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }
}
