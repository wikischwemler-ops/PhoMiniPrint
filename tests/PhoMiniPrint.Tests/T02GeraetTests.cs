using PhoMiniPrint.Printer;

namespace PhoMiniPrint.Tests;

public class T02GeraetTests
{
    [Fact]
    public void Druckbreite_entspricht_48_mm()
    {
        Assert.Equal(48, T02Geraet.DruckbreitePunkte / T02Geraet.PunkteProMm);
    }
}
