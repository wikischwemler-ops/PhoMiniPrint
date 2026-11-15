using System.Text;
using PhoMiniPrint.Printer;

// T02Probe – Prototyp aus Meilenstein M1.
// Prüft am echten Phomemo T02: Port-Zuordnung, Statusabfragen, Signalstärke und Testdrucke.

Console.OutputEncoding = Encoding.UTF8;
var befehl = args.Length > 0 ? args[0].ToLowerInvariant() : "hilfe";
var portVorgabe = args.SkipWhile(a => a != "--port").Skip(1).FirstOrDefault();

try
{
    switch (befehl)
    {
        case "suche":
            Suche();
            break;
        case "status":
            Status(PortErmitteln(portVorgabe));
            break;
        case "signal":
            if (args.Contains("--verbunden"))
            {
                // Während der Messung eine Verbindung offen halten (FR-021: Signal bei verbundenem Drucker).
                using var offen = new SppVerbindung(PortErmitteln(portVorgabe));
                offen.Oeffnen();
                Thread.Sleep(1000);
                await Signal();
            }
            else
            {
                await Signal();
            }

            break;
        case "test40":
            Drucken(PortErmitteln(portVorgabe), Testbilder.Quadrat40mm(), 3, "Quadrat 40 × 40 mm");
            break;
        case "laenge":
            Drucken(PortErmitteln(portVorgabe), Testbilder.Rahmen80x40mm(), 3, "Rahmen 80 mm (Länge) × 40 mm (Höhe)");
            break;
        case "breite":
            Drucken(PortErmitteln(portVorgabe), Testbilder.VolleBreite(), 3, "volle Druckbreite 384 Punkte mit mm-Skala");
            break;
        case "dichte":
            var port = PortErmitteln(portVorgabe);
            for (var stufe = 1; stufe <= 4; stufe++)
            {
                Drucken(port, Testbilder.Dichtefeld(stufe), stufe, $"Dichtestufe {stufe}");
                Thread.Sleep(1500);
            }

            break;
        case "ueberwachen":
            Ueberwachen(PortErmitteln(portVorgabe), TimeSpan.FromSeconds(90));
            break;
        case "gesamt":
            Gesamttest(PortErmitteln(portVorgabe));
            break;
        default:
            Console.WriteLine("""
                T02Probe – Prüfprogramm für den Phomemo T02

                  suche    gekoppelte T02 und zugehörigen COM-Port anzeigen
                  status   Statusabfragen senden und Antworten anzeigen
                  signal   Bluetooth-Signalstärke über Windows lesen
                  test40   Quadrat 40 × 40 mm drucken (nachmessen)
                  laenge   Rahmen 80 × 40 mm drucken (nachmessen)
                  breite   volle Druckbreite mit mm-Skala drucken
                  dichte   vier Felder mit Dichtestufe 1 bis 4 drucken
                  ueberwachen  90 s lang Papier, Deckel und Akku beobachten (kein Druck)
                  gesamt   alle Testmuster auf einem Streifen (Papier sparen)

                  --port COMx   Port vorgeben statt automatisch suchen
                """);
            break;
    }

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Fehler: {ex.Message}");
    return 1;
}

static void Suche()
{
    var funde = T02Suche.Suchen();
    if (funde.Count == 0)
    {
        Console.WriteLine("Kein gekoppelter T02 gefunden.");
        return;
    }

    foreach (var f in funde)
    {
        Console.WriteLine($"{f.Name}: COM-Port {f.ComPort ?? "nicht gefunden"}");
    }
}

static string PortErmitteln(string? vorgabe)
{
    if (!string.IsNullOrWhiteSpace(vorgabe))
    {
        return vorgabe;
    }

    var fund = T02Suche.Suchen().FirstOrDefault(f => f.ComPort is not null)
        ?? throw new InvalidOperationException("Kein T02 mit Bluetooth-COM-Port gefunden. Ist der Drucker gekoppelt?");
    Console.WriteLine($"Verwende {fund.Name} an {fund.ComPort}.");
    return fund.ComPort!;
}

static void Status(string port)
{
    using var verbindung = new SppVerbindung(port);
    verbindung.Oeffnen();
    Console.WriteLine("Verbunden. Sende Abfragen …");
    Thread.Sleep(500);

    foreach (var (name, daten) in T02Befehle.Abfragen)
    {
        var (meldungen, roh) = verbindung.Abfragen(daten, TimeSpan.FromMilliseconds(1000));
        var rohText = roh.Length == 0 ? "keine Antwort"
            : meldungen.Any(m => m.Kennung == 0x08) ? "(enthält Seriennummer, ausgeblendet)"
            : Convert.ToHexString(roh);
        Console.WriteLine($"{name,-24} roh: {rohText}");
        foreach (var m in meldungen)
        {
            Console.WriteLine($"{"",-24}      {m}");
        }
    }

    Console.WriteLine($"Meldungen insgesamt: {verbindung.Meldungen.Count}");
}

static void Ueberwachen(string port, TimeSpan dauer)
{
    using var verbindung = new SppVerbindung(port);
    verbindung.Oeffnen();
    Console.WriteLine($"Beobachte {dauer.TotalSeconds:0} s lang. Jetzt Deckel öffnen, Rolle entnehmen, wieder einlegen, Deckel schließen.");
    var ende = DateTime.UtcNow + dauer;
    var letzter = string.Empty;
    var gesehen = 0;
    while (DateTime.UtcNow < ende)
    {
        foreach (var abfrage in new[] { T02Befehle.AbfragePapier(), T02Befehle.AbfrageDeckel(), T02Befehle.AbfrageAkku() })
        {
            verbindung.Abfragen(abfrage, TimeSpan.FromMilliseconds(300));
        }

        List<T02Meldung> neu;
        lock (verbindung.Meldungen)
        {
            neu = verbindung.Meldungen.Skip(gesehen).ToList();
            gesehen += neu.Count;
        }

        var papier = neu.LastOrDefault(m => m.Kennung == 0x06);
        var deckel = neu.LastOrDefault(m => m.Kennung == 0x05);
        var akku = neu.LastOrDefault(m => m.Kennung == 0x04);
        var zustand = $"{(papier.Kennung == 0 ? "Papier ?" : papier.Beschreibung())} · {(deckel.Kennung == 0 ? "Deckel ?" : deckel.Beschreibung())} · {(akku.Kennung == 0 ? "Akku ?" : akku.Beschreibung())}";
        var sonstige = neu.Where(m => m.Kennung is not (0x04 or 0x05 or 0x06)).ToList();
        if (zustand != letzter)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss}  {zustand}  (roh Papier {papier.Wert:X2}, Deckel {deckel.Wert:X2})");
            letzter = zustand;
        }

        foreach (var m in sonstige)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss}  weitere Meldung: {m}");
        }

        Thread.Sleep(500);
    }
}

static void Gesamttest(string port)
{
    // Ein Auftrag: Initialisieren einmal, dann je Abschnitt Dichte + Raster, am Ende ein Nachlauf.
    // Prüft nebenbei, ob die Dichte innerhalb eines Auftrags umgeschaltet werden kann.
    var abschnitte = new List<(int Stufe, Rasterbild Bild)>
    {
        (3, Testbilder.VolleBreite()),
        (3, Testbilder.Quadrat40mm()),
    };
    for (var stufe = 1; stufe <= 4; stufe++)
    {
        abschnitte.Add((stufe, Testbilder.DichtefeldKlein(stufe)));
    }

    var daten = new List<byte>();
    daten.AddRange(T02Befehle.Initialisieren());
    daten.AddRange(T02Befehle.Medium());
    daten.AddRange(T02Befehle.Unkomprimiert());
    daten.AddRange(T02Befehle.Raster(new Rasterbild(T02Geraet.DruckbreitePunkte, T02Befehle.Vorlaufzeilen)));
    var zeilen = T02Befehle.Vorlaufzeilen;
    foreach (var (stufe, bild) in abschnitte)
    {
        daten.AddRange(T02Befehle.Druckdichte(stufe));
        daten.AddRange(T02Befehle.Raster(bild));
        var abstand = new Rasterbild(T02Geraet.DruckbreitePunkte, 2 * T02Geraet.PunkteProMm);
        daten.AddRange(T02Befehle.Raster(abstand));
        zeilen += bild.Hoehe + abstand.Hoehe;
    }

    daten.AddRange(T02Befehle.Raster(new Rasterbild(T02Geraet.DruckbreitePunkte, T02Befehle.Nachlaufzeilen)));
    daten.AddRange(T02Befehle.Vorschub());
    daten.AddRange(T02Befehle.Vorschub());
    zeilen += T02Befehle.Nachlaufzeilen;

    using var verbindung = new SppVerbindung(port);
    verbindung.Oeffnen();
    Thread.Sleep(300);
    Console.WriteLine($"Gesamttest: {zeilen / T02Geraet.PunkteProMm} mm Streifen, {daten.Count} Byte");
    Console.WriteLine("Inhalt: mm-Skala volle Breite · Quadrat 40 × 40 mm · Dichtefelder Stufe 1–4 (Kästchen = Stufe)");
    verbindung.Senden(daten.ToArray());
    verbindung.AufDruckendeWarten(zeilen);
    foreach (var m in verbindung.Meldungen)
    {
        Console.WriteLine($"Meldung: {m}");
    }

    Console.WriteLine("Fertig.");
}

static async Task Signal()
{
    for (var i = 1; i <= 5; i++)
    {
        var messungen = await Signalstaerke.MessenAsync();
        if (messungen.Count == 0)
        {
            Console.WriteLine("Windows meldet keinen T02 als Bluetooth-Endpunkt.");
            return;
        }

        foreach (var m in messungen)
        {
            var dbm = m.Dbm is null ? "kein Wert" : $"{m.Dbm} dBm = {m.Prozent} %";
            Console.WriteLine($"Messung {i}: {m.Name}, verbunden: {m.Verbunden?.ToString() ?? "?"}, Signal: {dbm}");
        }

        await Task.Delay(1000);
    }
}

static void Drucken(string port, Rasterbild bild, int dichtestufe, string titel)
{
    using var verbindung = new SppVerbindung(port);
    verbindung.Oeffnen();
    Thread.Sleep(300);
    var auftrag = T02Befehle.Druckauftrag(bild, dichtestufe);
    Console.WriteLine($"Drucke „{titel}“: {bild.Breite} × {bild.Hoehe} Punkte, {auftrag.Length} Byte, Dichtestufe {dichtestufe}");
    var letzte = -1;
    verbindung.Senden(auftrag, new Progress<double>(p =>
    {
        var prozent = (int)(p * 100) / 10 * 10;
        if (prozent != letzte)
        {
            letzte = prozent;
            Console.Write($"{prozent} % ");
        }
    }));
    Console.WriteLine();
    Console.WriteLine("Warte auf das Druckende …");
    verbindung.AufDruckendeWarten(bild.Hoehe + T02Befehle.Vorlaufzeilen + T02Befehle.Nachlaufzeilen);
    foreach (var m in verbindung.Meldungen)
    {
        Console.WriteLine($"Meldung: {m}");
    }

    Console.WriteLine("Fertig.");
}

/// <summary>Testbilder. Breite = quer zum Band (Höhe), Zeilen = entlang des Bandes (Länge).</summary>
internal static class Testbilder
{
    private const int Mm = T02Geraet.PunkteProMm;
    private const int Breite = T02Geraet.DruckbreitePunkte;

    /// <summary>Quadrat 40 × 40 mm, mittig, Strich 2 Punkte, Mittelkreuz und 10-mm-Marken.</summary>
    public static Rasterbild Quadrat40mm()
    {
        var bild = new Rasterbild(Breite, 40 * Mm);
        var x = (Breite - (40 * Mm)) / 2;
        bild.Rahmen(x, 0, 40 * Mm, 40 * Mm, 2);
        bild.Rechteck(x + (20 * Mm) - 1, (20 * Mm) - (2 * Mm), 2, 4 * Mm);
        bild.Rechteck(x + (18 * Mm), (20 * Mm) - 1, 4 * Mm, 2);
        for (var i = 1; i < 4; i++)
        {
            bild.Rechteck(x + (i * 10 * Mm) - 1, 0, 2, 3 * Mm);
            bild.Rechteck(x, (i * 10 * Mm) - 1, 3 * Mm, 2);
        }

        return bild;
    }

    /// <summary>Rahmen 80 mm entlang des Bandes × 40 mm quer, 10-mm-Marken entlang des Bandes.</summary>
    public static Rasterbild Rahmen80x40mm()
    {
        var bild = new Rasterbild(Breite, 80 * Mm);
        var x = (Breite - (40 * Mm)) / 2;
        bild.Rahmen(x, 0, 40 * Mm, 80 * Mm, 2);
        for (var i = 1; i < 8; i++)
        {
            bild.Rechteck(x, (i * 10 * Mm) - 1, (i % 5 == 0 ? 8 : 4) * Mm, 2);
        }

        return bild;
    }

    /// <summary>Volle Breite: Rahmen über 384 Punkte, Skala mit Strichen je 1 mm, 5 mm, 10 mm.</summary>
    public static Rasterbild VolleBreite()
    {
        var bild = new Rasterbild(Breite, 15 * Mm);
        bild.Rahmen(0, 0, Breite, 15 * Mm, 2);
        for (var mm = 1; mm < Breite / Mm; mm++)
        {
            var laenge = mm % 10 == 0 ? 10 : mm % 5 == 0 ? 6 : 3;
            bild.Rechteck((mm * Mm) - 1, 0, 2, laenge * Mm);
        }

        return bild;
    }

    /// <summary>Kleines Dichtefeld 20 × 5 mm mit Stufenmarken (Papier sparen).</summary>
    public static Rasterbild DichtefeldKlein(int stufe)
    {
        var bild = new Rasterbild(Breite, 5 * Mm);
        bild.Rechteck(4 * Mm, 0, 20 * Mm, 5 * Mm);
        for (var i = 0; i < stufe; i++)
        {
            bild.Rechteck((27 + (i * 3)) * Mm, Mm, 2 * Mm, 3 * Mm);
        }

        return bild;
    }

    /// <summary>Schwarzes Feld 30 × 10 mm mit Stufenmarken (1–4 Kästchen) daneben.</summary>
    public static Rasterbild Dichtefeld(int stufe)
    {
        var bild = new Rasterbild(Breite, 12 * Mm);
        bild.Rechteck(2 * Mm, Mm, 30 * Mm, 10 * Mm);
        for (var i = 0; i < stufe; i++)
        {
            bild.Rechteck((35 + (i * 3)) * Mm, 4 * Mm, 2 * Mm, 4 * Mm);
        }

        return bild;
    }
}
