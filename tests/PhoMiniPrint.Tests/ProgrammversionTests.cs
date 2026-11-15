using System.Text.RegularExpressions;
using PhoMiniPrint.Core;

namespace PhoMiniPrint.Tests;

public class ProgrammversionTests
{
    [Fact]
    public void Version_folgt_dem_Schema_MAJOR_MINOR_PATCH()
    {
        var version = Programmversion.Von(typeof(Programmversion).Assembly);

        Assert.Matches(new Regex(@"^\d+\.\d+\.\d+$"), version);
    }

    [Theory]
    [InlineData("0.1.0+3f2a9c1", "0.1.0")]
    [InlineData("1.2.3", "1.2.3")]
    public void Build_Metadaten_werden_entfernt(string eingabe, string erwartet)
    {
        Assert.Equal(erwartet, Programmversion.Bereinigen(eingabe));
    }
}
