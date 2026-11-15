# Abnahme PhoMiniPrint (M8)

Stand: 2026-11-15 · geprüfte Version: 0.1.0, freigegeben als **1.0.0** · Drucker: Phomemo T02, Firmware 3.2.10, COM10

Legende: ✅ bestanden · 🔁 mit neuer Rolle nachholen · ➖ nicht freigegeben / entfällt

Die Rolle ist fast leer. Geprüft wurde daher mit 82 grünen Unit-Tests, an der Oberfläche am Bildschirm und am Gerät nur mit Statusabfragen. **Entscheidung des Benutzers (2026-11-15): Abnahme ohne Druck.** Die Maßgenauigkeit am Gerät ist aus M1 belegt (40 × 40 mm exakt, vom Benutzer gemessen). Der Abnahmestreifen (Abschnitt 2) wird mit einer neuen Rolle nachgeholt.

## 1. Anforderungen

| ID | Akzeptanzkriterium (kurz) | Prüfung | Ergebnis |
|---|---|---|---|
| FR-001 🔒 | Druck auf dem T02 vom PC | M1 am Gerät (40 × 40 mm gedruckt); Druck aus der Oberfläche: Abnahmestreifen | ✅ (Oberfläche: 🔁) |
| FR-002 🔒 | Verbindung über Bluetooth | T02 wird über die Bluetooth-Adresse gefunden (COM10) und verbunden, auch aus der installierten Version | ✅ |
| FR-003 | Verbindungsstatus sichtbar | Statusleiste „T02 bereit (COM10)“, „nicht verbunden – eingeschaltet?“ | ✅ |
| FR-004 🔒 | Bild in exakter Größe | Raster 40 mm = 320 Punkte (Test); M1 am Gerät genau 40 × 40 mm | ✅ |
| FR-005 | PNG/JPG öffnen, deutsche Fehlermeldungen | Dialog, Ziehen, Startparameter; Tests für Format und defekte Datei | ✅ |
| FR-006 | Länge und Höhe in mm, Seitenverhältnis bleibt | Eingabefelder Breite × Höhe, Ecken ziehen (64 × 32 mm aus 2 : 1) | ✅ |
| FR-007 | Regler „Stärke für Schwarz“ und „Druckdichte“ 0–100 % mit Zahl | gestaltete Regler, Vorschau sofort, Stufe 1–4 sichtbar; Tests | ✅ |
| FR-008 | Prüfung gegen bedruckbare Höhe | Meldungen „ragt über den Rand“, Drucken gesperrt; Tests | ✅ |
| FR-009 🔒 | Text mit Symbolen und Emojis | Abnahmetext `° ² € § ± ✓ ✗ ★ ♥ → ☎ ⚠ 😀 👍` Zeichen für Zeichen gerastert; Warnung bei fehlendem Zeichen | ✅ (Papier: 🔁) |
| FR-010 | Größe, Schriftart, fett, Ausrichtung | Textdialog am Bildschirm; Test „mittig“ | ✅ |
| FR-011 | Druckvorschau | Bandansicht zeigt das Schwarz-Weiß-Raster Punkt für Punkt (nicht freigegebener Vorschlag, trotzdem erfüllt) | ➖ |
| FR-012 | Kalibrierung mit Testseite, nur über Menü, gespeichert | Dialog am Bildschirm (70 mm → Warnung, 79,5 mm → 1,0063), Neustart; Testseite in Tests 80 × 40 mm | ✅ (Testseite gedruckt: entfällt, siehe 3) |
| FR-013 | Bild und Text auf einem Ausdruck | Test: Bild und Text im selben Druckauftrag | ✅ (Papier: 🔁) |
| FR-014 | Einstellungen merken | Vorschlag; teilweise durch F-10 erfüllt (letzter Drucker, Länge) | ➖ |
| FR-015 | Bild aus Zwischenablage | abgelehnt | ➖ |
| FR-016 | Ränder einzeln je Seite, gespeichert | vier Felder; nach Neustart erhalten (Rand links 3 mm) | ✅ |
| FR-017 | Drehen 0/90/180/270°, Vorgabegröße | Knöpfe, Test „Drehen ohne Verzerrung“ | ✅ |
| FR-018 | Version MAJOR.MINOR.PATCH sichtbar | Titelleiste, Info-Fenster, EXE, Installer, Apps & Features gleich | ✅ |
| FR-019 | Bandansicht, maximal einpassen, Maus | am Bildschirm geprüft | ✅ |
| FR-020 | Akku und Papier im Status | Akku 60 % mit Balken, „Papier ok“; Glättung getestet; „Papierstau?“ wenn Fertigmeldung fehlt | ✅ |
| FR-021 | Verbindungsqualität % und Balken (E-1) | 94–100 % am Gerät; Berechnung getestet | ✅ |
| FR-022 | Druckanimation | Startillustration geprüft; Animation läuft nur bei echtem Druck | 🔁 |
| NFR-001 🔒 | Deutsch | alle Texte, Meldungen und Installer deutsch | ✅ |
| NFR-002 | Windows 11 | entwickelt und geprüft unter Windows 11 Pro | ✅ |
| NFR-003 | ±1 mm | M1: 40 × 40 mm exakt (Länge und Höhe); Raster-Tests | ✅ |
| NFR-004 | verständliche Fehlermeldungen | deutsche Meldungen in allen Fehlerfällen | ✅ |
| NFR-005 | lokal, ohne Netzwerk | kein Netzwerkcode im Quelltext (nur Link im Info-Fenster) | ✅ |
| NFR-006 | Installer | Installation, Startmenü, Deinstallation geprüft (52,6 MB, eingebautes .NET) | ✅ |
| NFR-007 | C#, .NET, WPF | .NET 10, WPF | ✅ |
| BR-001 | Größe durch Druckbreite begrenzt | 48 mm bedruckbar, Prüfungen | ✅ |
| BR-002 | mm → Punkte | Tests (40 mm = 320 Punkte, Kalibrierfaktor) | ✅ |
| INT-001 | T02-Protokoll | Druck, Status, Fertigmeldung am Gerät | ✅ |
| INT-002 | TiMini-Print als Referenz | NOTICE, eigene Umsetzung | ✅ |

## 2. Abnahmestreifen – mit neuer Rolle nachholen (etwa 68 mm Papier)

Aufbau, gedruckt **aus der Oberfläche** (Bild einfügen, Text einfügen, Drucken):

- Etikett 60 mm lang, Ränder links/rechts 0 mm, oben/unten 1 mm
- Bild 60 × 48 mm: äußerer Rahmen = gesamter bedruckbarer Bereich, darin links ein Quadrat **40 × 40 mm** mit Teilstrichen alle 10 mm
- Text rechts daneben: `★ ✓ ♥ 😀`

| Messung | Soll | Ist | Ergebnis |
|---|---|---|---|
| Quadrat Länge (entlang des Bandes) | 40 mm ±1 | | 🔁 |
| Quadrat Höhe (quer) | 40 mm ±1 | | 🔁 |
| Abstand äußerer Rahmen → obere Papierkante | 1 mm (Druckkopf mittig) | | 🔁 |
| Abstand äußerer Rahmen → untere Papierkante | 1 mm | | 🔁 |
| Symbole vollständig, keine Kästchen | ja | | 🔁 |
| Text seitenrichtig (nicht gespiegelt) | ja | | 🔁 |
| Druckanimation: Wellen, Papier kommt heraus | ja | | 🔁 |

## 3. Bewusst nicht am Gerät geprüft

- Kalibrier-Testseite (91 mm): Rechnung und Raster sind getestet. Gedruckt wird sie erst mit einer neuen Rolle.
- Feintest der Druckdichte (E-3): mit neuer Rolle.
- Papier entnehmen und Drucker ausschalten im laufenden Betrieb: Logik ist getestet, am Gerät mit neuer Rolle.
- Installation auf einem PC ohne .NET: Der Installer bringt die Laufzeit mit, geprüft wurde auf einem PC mit .NET.

Das Abnahmebild (60 × 48 mm, PNG, 480 × 384 Punkte) liegt unter [`docs/bilder/abnahme.png`](bilder/abnahme.png). Einrichten: Länge 60 mm, Ränder 0/0/1/1, Bild einfügen und maximal einpassen, Text `★ ✓ ♥ 😀` mit 15 mm Breite rechts neben dem Quadrat.

## 4. Ergebnis

✅ **Abgenommen ohne Druck** (Entscheidung des Benutzers, 2026-11-15). Alle MUST-Anforderungen sind erfüllt. Die Maßgenauigkeit am Gerät ist aus M1 belegt. Offen und mit neuer Rolle nachzuholen: der Abnahmestreifen (Abschnitt 2), die Druckanimation am Gerät und die Punkte aus Abschnitt 3.

Version **1.0.0**, Installer `PhoMiniPrint-Setup-1.0.0.exe`, Git-Tag `v1.0.0` nach dem Merge.
