using System.Reflection;

namespace PhoMiniPrint.Core;

/// <summary>
/// Liefert die Programmversion nach dem Schema MAJOR.MINOR.PATCH (FR-018).
/// Quelle ist die Eigenschaft &lt;Version&gt; in Directory.Build.props.
/// </summary>
public static class Programmversion
{
    /// <summary>Version der angegebenen Assembly, z. B. "0.1.0".</summary>
    public static string Von(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var info = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(info))
        {
            return Bereinigen(info);
        }

        var v = assembly.GetName().Version;
        return v is null ? "0.0.0" : $"{v.Major}.{v.Minor}.{v.Build}";
    }

    /// <summary>
    /// Entfernt Build-Metadaten wie "+abc123" (Git-Commit), die .NET automatisch anhängt.
    /// </summary>
    public static string Bereinigen(string informationalVersion)
    {
        ArgumentNullException.ThrowIfNull(informationalVersion);
        var plus = informationalVersion.IndexOf('+');
        return plus >= 0 ? informationalVersion[..plus] : informationalVersion;
    }
}
