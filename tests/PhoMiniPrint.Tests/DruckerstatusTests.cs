using PhoMiniPrint.Core;

namespace PhoMiniPrint.Tests;

public class DruckerstatusTests
{
    private static readonly DateTime Start = new(2026, 11, 15, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Akku_Einbruch_nach_dem_Druck_wird_ignoriert()
    {
        // Messung aus M1: 63 % vor dem Druck, 34 % direkt danach, eine Minute später wieder 63 %.
        var akku = new Akkuglaettung();
        akku.Hinzufuegen(63, Start);
        akku.DruckBeendet(Start.AddSeconds(20));

        akku.Hinzufuegen(34, Start.AddSeconds(25));
        akku.Hinzufuegen(48, Start.AddSeconds(60));

        Assert.Equal(63, akku.Wert);
    }

    [Fact]
    public void Kurzer_Einbruch_ohne_Druck_schlaegt_nicht_durch()
    {
        var akku = new Akkuglaettung();
        akku.Hinzufuegen(70, Start);
        akku.Hinzufuegen(55, Start.AddSeconds(10));
        akku.Hinzufuegen(69, Start.AddSeconds(20));

        Assert.Equal(70, akku.Wert);
    }

    [Fact]
    public void Echtes_Entladen_wird_nach_dem_Zeitfenster_sichtbar()
    {
        var akku = new Akkuglaettung();
        akku.Hinzufuegen(70, Start);
        for (var s = 10; s <= 240; s += 10)
        {
            akku.Hinzufuegen(60, Start.AddSeconds(s));
        }

        Assert.Equal(60, akku.Wert);
    }

    [Fact]
    public void Erster_Wert_wird_auch_kurz_nach_einem_Druck_angezeigt()
    {
        var akku = new Akkuglaettung();
        akku.DruckBeendet(Start);

        akku.Hinzufuegen(40, Start.AddSeconds(5));

        Assert.Equal(40, akku.Wert);
    }

    [Fact]
    public void Schnelle_Antworten_ergeben_100_Prozent()
    {
        var q = new Verbindungsqualitaet();
        for (var i = 0; i < 5; i++)
        {
            q.Erfolg(TimeSpan.FromMilliseconds(60));
        }

        Assert.Equal(100, q.Prozent);
    }

    [Fact]
    public void Ausfaelle_und_langsame_Antworten_senken_die_Qualitaet()
    {
        var q = new Verbindungsqualitaet();
        for (var i = 0; i < 10; i++)
        {
            q.Erfolg(TimeSpan.FromMilliseconds(60));
        }

        q.Ausfall();
        q.Ausfall();
        Assert.Equal(80, q.Prozent);

        q.Erfolg(TimeSpan.FromMilliseconds(1500));
        Assert.Equal(72, q.Prozent);
    }

    [Fact]
    public void Ohne_Messung_ist_die_Qualitaet_unbekannt()
    {
        var q = new Verbindungsqualitaet();
        Assert.Null(q.Prozent);
        q.Ausfall();
        Assert.Equal(0, q.Prozent);
        q.Zuruecksetzen();
        Assert.Null(q.Prozent);
    }

    [Theory]
    [InlineData(150, 100.0)]
    [InlineData(825, 60.0)]
    [InlineData(5000, 20.0)]
    public void Antwortzeit_wird_linear_bewertet(int ms, double erwartet)
    {
        Assert.Equal(erwartet, Verbindungsqualitaet.Bewerten(TimeSpan.FromMilliseconds(ms)), 1);
    }

    [Fact]
    public void Kein_Papier_oder_Ueberhitzung_sperrt_den_Druck()
    {
        var bereit = Druckerstatus.Unverbunden with { Zustand = Verbindungszustand.Bereit, Papier = Papierzustand.Ok };

        Assert.True(bereit.DruckErlaubt);
        Assert.True(Druckerstatus.Unverbunden.DruckErlaubt);
        Assert.False((bereit with { Papier = Papierzustand.KeinPapier }).DruckErlaubt);
        Assert.False((bereit with { Ueberhitzt = true }).DruckErlaubt);
    }
}
