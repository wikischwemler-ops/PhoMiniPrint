using PhoMiniPrint.Printer;

namespace PhoMiniPrint.Tests;

public class T02BefehleTests
{
    [Fact]
    public void Rasterbild_packt_linken_Punkt_ins_hoechste_Bit()
    {
        var bild = new Rasterbild(16, 1);
        bild[0, 0] = true;
        bild[9, 0] = true;

        Assert.Equal([0x80, 0x40], bild.Daten.ToArray());
    }

    [Fact]
    public void Raster_erzeugt_GS_v_0_Kopf_mit_Breite_in_Byte_und_Zeilenzahl()
    {
        var bild = new Rasterbild(T02Geraet.DruckbreitePunkte, 320);

        var befehl = T02Befehle.Raster(bild);

        Assert.Equal([0x1D, 0x76, 0x30, 0x00, 48, 0, 0x40, 0x01], befehl[..8]);
        Assert.Equal(8 + (48 * 320), befehl.Length);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(24, 1)]
    [InlineData(25, 2)]
    [InlineData(50, 3)]
    [InlineData(74, 3)]
    [InlineData(75, 4)]
    [InlineData(100, 4)]
    public void Regler_wird_auf_vier_Dichtestufen_abgebildet(int prozent, int stufe)
    {
        Assert.Equal(stufe, T02Befehle.DichtestufeAusProzent(prozent));
    }

    [Fact]
    public void Druckauftrag_hat_die_erwartete_Befehlsfolge()
    {
        var bild = new Rasterbild(8, 2);

        var auftrag = T02Befehle.Druckauftrag(bild, 3);

        byte[] erwartetAnfang = [0x1B, 0x40, 0x1F, 0x11, 0x02, 4, 0x1F, 0x11, 0x37, 100, 0x1F, 0x11, 0x0B, 0x1F, 0x11, 0x35, 0x00];
        Assert.Equal(erwartetAnfang, auftrag[..erwartetAnfang.Length]);
        Assert.Equal([0x1B, 0x64, 0x02, 0x1B, 0x64, 0x02], auftrag[^6..]);

        // 4 Vorlaufzeilen + 2 Bildzeilen + Nachlauf.
        var zeilen = T02Befehle.Vorlaufzeilen + 2 + T02Befehle.Nachlaufzeilen;
        var kopf = auftrag[erwartetAnfang.Length..(erwartetAnfang.Length + 8)];
        Assert.Equal([0x1D, 0x76, 0x30, 0x00, 1, 0, (byte)(zeilen & 0xFF), (byte)(zeilen >> 8)], kopf);
    }

    [Fact]
    public void Zu_breites_Bild_wird_abgelehnt()
    {
        var bild = new Rasterbild(T02Geraet.DruckbreitePunkte + 8, 1);

        Assert.Throws<ArgumentException>(() => T02Befehle.Druckauftrag(bild, 2));
    }

    [Fact]
    public void Antwortleser_zerlegt_auch_auf_mehrere_Pakete_verteilte_Meldungen()
    {
        var leser = new T02Antwortleser();

        var erste = leser.Hinzufuegen([0x1A, 0x04, 0x5A, 0x1A, 0x06]);
        var zweite = leser.Hinzufuegen([0x88]);

        Assert.Equal([(0x04, 0x5A)], erste.Select(m => ((int)m.Kennung, (int)m.Wert)));
        Assert.Equal([(0x06, 0x88)], zweite.Select(m => ((int)m.Kennung, (int)m.Wert)));
        Assert.Equal("kein Papier", zweite[0].Beschreibung());
        Assert.Empty(leser.Unbekannt);
    }

    [Fact]
    public void Antwortleser_erkennt_mehrbytige_Meldungen_wie_am_T02_gemessen()
    {
        var leser = new T02Antwortleser();

        // Ausschnitt aus der echten Antwortfolge des T02 (Seriennummer durch Platzhalter ersetzt).
        byte[] strom =
        [
            0x1A, 0x3B, 0x04, 0x40, 0x00, 0x00, 0x00,
            0x1A, 0x09, 0x0C,
            0x1A, 0x07, 0x03, 0x02, 0x0A,
            0x1A, 0x04, 0x43,
            0x1A, 0x08, .. "XXXXXXXXXXXXXXX"u8,
            0x1A, 0x06, 0x89,
        ];

        var meldungen = leser.Hinzufuegen(strom);

        Assert.Equal([0x3B, 0x09, 0x07, 0x04, 0x08, 0x06], meldungen.Select(m => m.Kennung));
        Assert.Equal("Firmware 3.2.10", meldungen[2].Beschreibung());
        Assert.Equal("Akku 67 %", meldungen[3].Beschreibung());
        Assert.DoesNotContain("58", meldungen[4].ToString());
        Assert.Empty(leser.Unbekannt);
    }

    [Fact]
    public void Signalstaerke_wird_auf_Prozent_umgerechnet()
    {
        Assert.Equal(0, new Signalmessung("T02", -95, true).Prozent);
        Assert.Equal(50, new Signalmessung("T02", -65, true).Prozent);
        Assert.Equal(100, new Signalmessung("T02", -30, true).Prozent);
        Assert.Null(new Signalmessung("T02", null, true).Prozent);
    }
}
