using System.IO;
using PhoMiniPrint.Core;
using PhoMiniPrint.Printer;

namespace PhoMiniPrint.Tests;

public sealed class KalibrierungTests : IDisposable
{
    private readonly string _ordner = Path.Combine(Path.GetTempPath(), $"phominiprint-{Guid.NewGuid():N}");

    private string Pfad => Path.Combine(_ordner, "einstellungen.json");

    public void Dispose()
    {
        if (Directory.Exists(_ordner))
        {
            Directory.Delete(_ordner, recursive: true);
        }
    }

    [Fact]
    public void Einstellungen_bleiben_nach_Neustart_erhalten()
    {
        var gespeichert = new Einstellungen
        {
            Laenge = 62.5,
            Raender = new Raender(3, 4, 1.5, 2.5),
            StaerkeSchwarzProzent = 37,
            DruckdichteProzent = 80,
            Kalibrierung = new Kalibrierung(1.0125, 0.995),
            LetzterDrucker = "AABBCCDDEEFF",
        };

        Assert.True(new Einstellungsspeicher(Pfad).Speichern(gespeichert));
        var (geladen, zurueckgesetzt) = new Einstellungsspeicher(Pfad).Laden();

        Assert.False(zurueckgesetzt);
        Assert.Equal(gespeichert, geladen);
        Assert.False(File.Exists(Pfad + ".neu"));
    }

    [Fact]
    public void Fehlende_Datei_liefert_Standardwerte_ohne_Hinweis()
    {
        var (geladen, zurueckgesetzt) = new Einstellungsspeicher(Pfad).Laden();

        Assert.Equal(Einstellungen.Standard, geladen);
        Assert.False(zurueckgesetzt);
    }

    [Theory]
    [InlineData("kein json {")]
    [InlineData("""{ "StaerkeSchwarzProzent": 250 }""")]
    [InlineData("""{ "Kalibrierung": { "FaktorLaenge": 2.0, "FaktorHoehe": 1.0 } }""")]
    [InlineData("null")]
    public void Defekte_Datei_liefert_Standardwerte_mit_Hinweis(string inhalt)
    {
        Directory.CreateDirectory(_ordner);
        File.WriteAllText(Pfad, inhalt);

        var (geladen, zurueckgesetzt) = new Einstellungsspeicher(Pfad).Laden();

        Assert.Equal(Einstellungen.Standard, geladen);
        Assert.True(zurueckgesetzt);
    }

    [Fact]
    public void Faktoren_aus_Messung_berechnet()
    {
        var (neu, fehler) = Kalibrierrechnung.Berechnen(Kalibrierung.Keine, 79.0, 40.0);

        Assert.Null(fehler);
        Assert.Equal(80.0 / 79.0, neu!.FaktorLaenge, 4);
        Assert.Equal(1.0, neu.FaktorHoehe, 4);
    }

    [Fact]
    public void Faktoren_werden_mit_den_bisherigen_verrechnet()
    {
        var (neu, _) = Kalibrierrechnung.Berechnen(new Kalibrierung(1.02, 1.0), 80.8, 40.0);

        Assert.Equal(1.02 * 80.0 / 80.8, neu!.FaktorLaenge, 4);
    }

    [Theory]
    [InlineData(70.0, 40.0)]
    [InlineData(80.0, 30.0)]
    [InlineData(0.0, 40.0)]
    public void Unplausible_Messung_wird_abgelehnt(double laenge, double hoehe)
    {
        var (neu, fehler) = Kalibrierrechnung.Berechnen(Kalibrierung.Keine, laenge, hoehe);

        Assert.Null(neu);
        Assert.NotNull(fehler);
    }

    [Fact]
    public void Unplausible_Messung_meldet_den_Text_aus_F09()
    {
        var (_, fehler) = Kalibrierrechnung.Berechnen(Kalibrierung.Keine, 70, 40);

        Assert.Equal("Bitte nachmessen. Der Wert weicht mehr als 10 % ab.", fehler);
    }

    [Fact]
    public void Testseite_hat_Rahmen_80_x_40_mm()
    {
        var bild = Kalibrierrechnung.Testseite(Kalibrierung.Keine);

        Assert.Equal(384, bild.Breite);
        Assert.Equal(Layout.MmZuPunkten(Kalibrierrechnung.TestseiteLaengeMm), bild.Hoehe);
        var (zeileVon, zeileBis, spalteVon, spalteBis) = Rahmenmass(bild);
        Assert.Equal(640, zeileBis - zeileVon + 1);
        Assert.Equal(320, spalteBis - spalteVon + 1);
        Assert.Equal(16, zeileVon);
    }

    [Fact]
    public void Kalibrierte_Testseite_ist_entsprechend_verlaengert()
    {
        var bild = Kalibrierrechnung.Testseite(new Kalibrierung(1.025, 0.975));

        var (zeileVon, zeileBis, spalteVon, spalteBis) = Rahmenmass(bild);
        Assert.Equal(Layout.MmZuPunkten(80, 1.025), zeileBis - zeileVon + 1);
        Assert.Equal(Layout.MmZuPunkten(40, 0.975), spalteBis - spalteVon + 1);
    }

    [Fact]
    public void Testseite_ergibt_druckbaren_Auftrag()
    {
        var daten = Druckvorbereitung.AusRaster(Kalibrierrechnung.Testseite(Kalibrierung.Keine), 50);

        Assert.Equal(T02Befehle.Vorlaufzeilen + 672 + T02Befehle.Nachlaufzeilen, daten.Zeilen);
        Assert.Equal(T02Befehle.Druckdichte(3), daten.Auftrag[2..10]);
    }

    /// <summary>Außenmaß des Rahmens: erste/letzte Zeile und Spalte der durchgehenden Rahmenlinien.</summary>
    private static (int ZeileVon, int ZeileBis, int SpalteVon, int SpalteBis) Rahmenmass(Rasterbild bild)
    {
        // Eine Zeile, in der die linke Rahmenseite liegt, ist über die ganze Rahmenhöhe schwarz.
        bool VolleZeile(int z)
        {
            var n = 0;
            for (var s = 0; s < bild.Breite; s++)
            {
                n += bild[s, z] ? 1 : 0;
            }

            return n > 250;
        }

        bool VolleSpalte(int s)
        {
            var n = 0;
            for (var z = 0; z < bild.Hoehe; z++)
            {
                n += bild[s, z] ? 1 : 0;
            }

            return n > 550;
        }

        var zeilen = Enumerable.Range(0, bild.Hoehe).Where(VolleZeile).ToList();
        var spalten = Enumerable.Range(0, bild.Breite).Where(VolleSpalte).ToList();
        return (zeilen.Min(), zeilen.Max(), spalten.Min(), spalten.Max());
    }
}
