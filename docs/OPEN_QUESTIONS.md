# Offene Fragen – PhoMiniPrint

- **Projektstatus:** RÜCKFRAGEN
- **Bezug:** [ANALYSIS_SUMMARY.md](ANALYSIS_SUMMARY.md), [REQUIREMENTS_DRAFT.md](REQUIREMENTS_DRAFT.md)

Bitte die Antwort hinter **Antwort:** eintragen. Wo Optionen stehen, reicht der Buchstabe. Die Optionen sind **Vorschläge der KI**, sie gelten erst mit deiner Antwort.

---

## A – Exakte Größe (Mindestanforderung 3)

**Q-A1: In welcher Einheit gibst du die Größe an?**
a) Millimeter b) Zentimeter c) beides wählbar
Antwort: a) Millimeter (2026-11-15)

**Q-A2: Was legst du fest?**
a) nur die Breite, die Höhe ergibt sich aus dem Seitenverhältnis
b) Breite und Höhe frei (Bild wird ggf. verzerrt)
c) Breite und Höhe, Bild passt hinein ohne Verzerrung
d) alle drei Varianten wählbar
Antwort (2026-11-15, Wortlaut sinngemäß): Festgelegt wird die **Länge entlang des Bandes**. Die **Höhe** (quer zum Band) ist mechanisch begrenzt, Bandhöhe 52 mm, muss aber nicht voll ausgenutzt werden. Deshalb **Ränder links/rechts und oben/unten**.
→ Begriffe ab jetzt: **Länge** = in Laufrichtung des Bandes, **Höhe** = quer zum Band (max. 52 mm Band).
→ Rückfrage Q-A2b siehe unten.

**Q-A2b: Wenn du Länge und Höhe eingibst, soll das Seitenverhältnis des Bildes erhalten bleiben?**
a) ja – Länge eingeben, Höhe ergibt sich (oder umgekehrt)
b) ja – Bild passt in Länge × Höhe hinein, Rest bleibt weiß
c) nein – Bild wird auf Länge × Höhe gestreckt
Antwort (2026-11-15, sinngemäß): **Seitenverhältnis beibehalten.** Beim Einfügen soll der **Rahmen des Bandes zu sehen** sein. Der Benutzer soll das Bild **selbst** auf das Band-Verhältnis **anpassen** können.

**Q-A2c: Wie willst du das Bild auf dem Band anpassen?**
a) mit der Maus: an den Ecken ziehen (Größe), in der Mitte ziehen (Position)
b) mit Zahlen: Länge/Höhe und Position in mm eintippen
c) beides, Maus und Zahlen bleiben synchron
Antwort: a) Maus (2026-11-15)

**Q-A2d: Was passiert beim Einfügen zuerst?**
a) Bild wird automatisch so groß wie möglich ins Band (innerhalb der Ränder) eingepasst, danach anpassen
b) Bild wird in der zuletzt verwendeten Größe eingefügt
Antwort: a) automatisch maximal einpassen (2026-11-15)

**Q-A3: Wie genau muss „exakt“ sein?**
a) ±1 mm b) ±0,5 mm c) so genau wie der Drucker kann (ca. ±0,125 mm)
Antwort: a) ±1 mm (2026-11-15)

**Q-A4: Darf das Programm ein Bild automatisch drehen, wenn es breiter als die Druckbreite (ca. 48 mm) ist und dann längs gedruckt wird?**
a) ja, automatisch b) ja, aber nachfragen c) nein, nur warnen
Antwort (2026-11-15): Auswahl im Menü durch den Benutzer – drehen oder nicht. Das Bild muss **immer auf die vorgegebene Größe skaliert** werden. Drehwinkel: **alle vier (0°, 90°, 180°, 270°)**.

**Q-A5: Soll es eine Kalibrierung geben (Testmuster drucken, nachmessen, Korrektur eintragen), damit auch die Länge exakt stimmt?**
a) ja b) nein
Antwort (2026-11-15): ja. Testseite, z. B. Rahmen mit Maßangaben. Der Benutzer misst nach und stellt die Werte ein. Wird zusätzlich über ein eigenes **Kalibrierungsmenü** angeboten. Das Programm **speichert** die Kalibrierdaten.

**Q-A5b: Wann soll die Kalibrierung angeboten werden?**
a) nur über das Kalibrierungsmenü
b) zusätzlich beim ersten Start automatisch vorschlagen
Antwort: a) nur über das Kalibrierungsmenü (2026-11-15)

**Q-A6: Soll ein Rand (Abstand oben/unten) einstellbar sein?**
a) ja b) nein
Antwort: ja – Ränder links/rechts und oben/unten (aus Antwort Q-A2, 2026-11-15). Ausdrücklich bestätigt: „Ränder sollten einstellbar sein“ (2026-11-15). Ränder **einzeln je Seite** einstellbar und werden **gespeichert** (2026-11-15).

## B – Bilder

**Q-B1: Welche Dateiformate brauchst du?**
a) PNG und JPG b) zusätzlich BMP, GIF c) zusätzlich PDF d) Bild aus der Zwischenablage einfügen (Strg+V)
Antwort (mehrere möglich):

**Q-B2: Wie soll ein Farb- oder Graustufenbild in Schwarz-Weiß umgewandelt werden?**
a) automatisch b) wählbar: harte Schwelle oder Rasterung, mit Helligkeits- und Kontrastregler
Antwort:

**Q-B3: Soll das Bild im Programm zuschneidbar oder drehbar sein?**
a) ja, beides b) nur drehen c) nein
Antwort:

## C – Text und Symbole (Mindestanforderung 4)

**Q-C1: Welche Symbole meinst du?** Bitte Beispiele nennen.
a) Sonderzeichen wie ° ² ³ € § ± ✓ ✗ ★ → ←
b) Emojis wie 😀 ❤️ 👍 (werden schwarz-weiß gedruckt)
c) Icons/Piktogramme wie Telefon, Haus, Warnzeichen
d) Sonstiges:
Antwort:

**Q-C2: Wie sollen Symbole eingefügt werden?**
a) per Tastatur/Einfügen b) Symbolauswahl im Programm c) beides
Antwort:

**Q-C3: Was soll beim Text einstellbar sein?**
a) Schriftgröße b) Schriftart c) fett/kursiv d) Ausrichtung links/mittig/rechts e) mehrere Zeilen
Antwort (mehrere möglich):

**Q-C4: Soll Text auch in exakter Größe gedruckt werden, z. B. „Schrifthöhe 5 mm“ oder „Etikett 40 × 20 mm“?**
a) ja b) nein
Antwort:

**Q-C5: Sollen Bild und Text zusammen auf einem Ausdruck stehen können (z. B. Logo mit Beschriftung)?**
a) ja b) nein, getrennt reicht
Antwort:

## D – Bedienung

**Q-D1: Wie soll das Programm aussehen?**
a) ein Fenster mit Vorschau und Einstellungen daneben b) Assistent Schritt für Schritt c) egal
Antwort:

**Q-D2: Soll die Vorschau den Ausdruck in Originalgröße am Bildschirm zeigen?**
a) ja b) nein
Antwort:

**Q-D3: Soll sich das Programm Einstellungen merken (letzter Drucker, letzte Größe, Vorlagen)?**
a) ja b) nein
Antwort:

**Q-D4: Soll man mehrere Exemplare auf einmal drucken können?**
a) ja b) nein
Antwort:

**Q-D5: Welche Versionsangaben sollen in der Software stehen und wo?** (aus „beachte auch Versionsinfo – in der SW Angaben“, 2026-11-15)
a) Info-Fenster „Über PhoMiniPrint“ im Menü „Hilfe“
b) zusätzlich Versionsnummer in der Titelleiste
c) Inhalt: Versionsnummer, Build-Datum, Lizenzhinweise
d) Schema `MAJOR.MINOR.PATCH` (z. B. 1.0.0), gleich in Installer und EXE
Antwort (mehrere möglich): d) Schema `MAJOR.MINOR.PATCH` (2026-11-15). a)–c) noch offen.

## E – Drucker und Verbindung

**Q-E1: Hast du nur einen T02 oder auch andere Phomemo-Modelle (z. B. M02, M110)?**
Antwort:

**Q-E2: Ist der T02 in Windows schon über Bluetooth gekoppelt?**
a) ja b) nein
Antwort:

**Q-E3: Soll das Programm den Akkustand bzw. den Papierstatus anzeigen, falls der Drucker das meldet?**
a) ja b) nein
Antwort:

## F – Technik und Umsetzung

**Q-F1: Soll TiMini-Print als Grundlage weiterentwickelt werden oder ein eigenes Programm entstehen?**
a) TiMini-Print forken und erweitern (Python, schnellster Weg)
b) eigenes Programm, Bluetooth-Protokoll aus TiMini-Print übernehmen
c) KI soll beides bewerten und vorschlagen
Antwort (2026-11-15): eigenes Programm in **C# mit .NET (WPF)**. TiMini-Print dient als Referenz für das Bluetooth-Protokoll, das nach C# portiert wird. (KI hatte Python mit Qt empfohlen; Benutzer hat die Alternative C#/.NET gewählt.)

**Q-F2: Wie soll das Programm gestartet werden?**
a) Doppelklick auf eine EXE ohne Installation b) Installation mit Startmenü-Eintrag c) egal
Antwort: b) als **Installer** angeboten; läuft auf **Windows 11** (2026-11-15)

**Q-F3: Auf wie vielen PCs soll es laufen?**
a) nur dieser PC b) mehrere Windows-PCs
Antwort:

## G – Sonstiges

**Q-G1: Gibt es optionale Wünsche (Abschnitt 5 der Eingabe)?** Zum Beispiel QR-Code, Barcode, Datum/Uhrzeit, Vorlagen, Druckverlauf.
Antwort:

**Q-G2: Konkrete Anwendungsbeispiele?** Wofür druckst du typischerweise?
Antwort:
