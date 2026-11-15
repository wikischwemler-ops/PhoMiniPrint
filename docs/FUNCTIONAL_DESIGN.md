# Funktionales Design – PhoMiniPrint

- **Projektstatus:** TECHNISCHE PLANUNG
- **Grundlage:** [REQUIREMENTS.md](REQUIREMENTS.md) (fachlich freigegeben am 2026-11-15)
- **Weitere Planung:** [PROCESS_FLOWS.md](PROCESS_FLOWS.md), [ARCHITECTURE.md](ARCHITECTURE.md), [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md)

Mit **[Vorschlag]** markierte Werte sind Planungsannahmen der KI. Sie sind nicht freigegeben und können vom Benutzer geändert werden.

---

## Begriffe

| Begriff | Bedeutung |
|---|---|
| **Band** | Thermopapier-Rolle des T02, ca. 52 mm hoch (quer zur Laufrichtung) |
| **Länge** | Maß **entlang** des Bandes (Laufrichtung). Nur durch die Rolle begrenzt. |
| **Höhe** | Maß **quer** zum Band. Bedruckbar sind 384 Punkte ≈ 48 mm. |
| **Etikett** | Ein Ausdruck: festgelegte Länge × Bandhöhe, mit Rändern und Elementen |
| **Element** | Ein Bild oder ein Textblock auf dem Etikett |
| **Druckerpunkt** | Kleinste druckbare Einheit, 1/8 mm (203 dpi) |
| **Bandansicht** | Arbeitsfläche, die das Etikett maßstabsgetreu mit Bandrahmen und Rändern zeigt |

## Übersicht der Funktionen

| Nr. | Funktion | Anforderungen |
|---|---|---|
| F-01 | Drucker verbinden | FR-001, FR-002, FR-003, INT-001 |
| F-02 | Druckerstatus anzeigen (Akku, Papier, Signal) | FR-003, FR-020, FR-021, NFR-004 |
| F-03 | Etikett einrichten (Länge, Ränder) | FR-006, FR-016, FR-008, BR-001 |
| F-04 | Bild einfügen | FR-005, FR-019, FR-004 |
| F-05 | Element anpassen (Maus, Drehen) | FR-017, FR-019, FR-006 |
| F-06 | Text mit Symbolen einfügen | FR-009, FR-010, FR-013 |
| F-07 | Schwarz-Weiß und Druckdichte einstellen | FR-007 |
| F-08 | Drucken | FR-001, FR-004, FR-008, BR-002, NFR-003 |
| F-09 | Kalibrieren | FR-012, NFR-003 |
| F-10 | Einstellungen speichern und laden | FR-007, FR-012, FR-016 |
| F-11 | Versionsinformation anzeigen | FR-018 |
| F-12 | Installieren | NFR-002, NFR-006 |

Alle Texte der Oberfläche sind auf Deutsch (NFR-001).

---

## F-01 Drucker verbinden

1. **Zweck:** Verbindung zum gekoppelten T02 über Bluetooth herstellen, damit gedruckt und der Status gelesen werden kann.
2. **Auslöser:** Programmstart (automatischer Versuch mit dem zuletzt verwendeten Drucker) oder Klick auf „Verbinden“.
3. **Voraussetzungen:** T02 ist in Windows gekoppelt (laut Q-E2 erfüllt), eingeschaltet und in Reichweite. Bluetooth am PC ist an.
4. **Eingabedaten:** gespeicherter Drucker (Gerätename „T02“, zugehöriger Bluetooth-COM-Port), optional die Auswahl des Benutzers aus der Liste gefundener Drucker.
5. **Verarbeitung:**
   1. Liste der gekoppelten Bluetooth-Geräte lesen und Geräte mit Namen „T02“, „T02E“ oder „C02E“ anbieten.
   2. Zugehörigen seriellen Bluetooth-Port (SPP) ermitteln.
   3. Port öffnen. Status „verbindet …“ anzeigen.
   4. Erste Statusabfrage senden (F-02). Kommt innerhalb von 5 s **[Vorschlag]** eine gültige Antwort, gilt der Drucker als „bereit“.
   5. Drucker als „zuletzt verwendet“ speichern (F-10).
6. **Ausgabedaten:** offene Verbindung, Status „bereit“.
7. **Fehlerfälle:**
   - Kein T02 gekoppelt → Meldung „Kein T02 gefunden. Bitte den Drucker in den Windows-Bluetooth-Einstellungen koppeln.“
   - Port lässt sich nicht öffnen oder keine Antwort → Status „Fehler“, Meldung „Drucker antwortet nicht. Ist er eingeschaltet und in Reichweite?“, Schaltfläche „Erneut versuchen“.
   - Bluetooth am PC aus → Meldung mit Hinweis auf die Windows-Einstellungen.
8. **Alternative Abläufe:** Mehrere T02 gekoppelt → Auswahlliste. Verbindung bricht später ab → Status „nicht verbunden“, automatischer Wiederverbindungsversuch beim nächsten Druck.
9. **Abhängigkeiten:** Windows-Bluetooth, INT-001.
10. **Anforderungs-IDs:** FR-001, FR-002, FR-003, INT-001, NFR-004.
11. **Akzeptanzkriterien:** Ein eingeschalteter, gekoppelter T02 wird ohne Kabel verbunden. Der Status wechselt sichtbar über „verbindet …“ zu „bereit“. Jeder Fehler hat eine deutsche Meldung mit nächstem Schritt.

## F-02 Druckerstatus anzeigen

1. **Zweck:** Jederzeit sehen, ob der Drucker bereit ist, wie voll der Akku ist, ob Papier da ist und wie gut die Funkverbindung ist.
2. **Auslöser:** Verbindung hergestellt, danach regelmäßig alle 10 s **[Vorschlag]** und unmittelbar vor jedem Druck.
3. **Voraussetzungen:** F-01 erfolgreich.
4. **Eingabedaten:** Statusantworten des Druckers (Akku, Papier, Deckel, Überhitzung), Signalstärke von Windows.
5. **Verarbeitung:**
   1. Statusabfragen an den Drucker senden (Akku, Papier, Deckel).
   2. Antworten auswerten und Statusmodell aktualisieren.
   3. Signalstärke über Windows abfragen (siehe Risiko in FR-021) und auf 0–100 % umrechnen.
   4. Anzeige in der Statusleiste aktualisieren.
6. **Ausgabedaten:** Statusleiste mit
   - Verbindung: nicht verbunden / verbindet … / bereit / Fehler
   - **Akku:** Zahl in % und Balken; bei niedrigem Stand orange markiert
   - **Papier:** „Papier ok“ / „kein Papier“ / „Deckel offen“ / „Papierstau“, sofern gemeldet
   - **Signal:** Zahl in % und Balken
7. **Fehlerfälle:** Drucker liefert einen Wert nicht → Anzeige „–“ mit Tooltip „wird vom Drucker nicht gemeldet“. Ob das akzeptabel ist, entscheidet der Benutzer nach dem Prototyp.
8. **Alternative Abläufe:** Während eines Druckauftrags werden keine regelmäßigen Abfragen gesendet, Statusmeldungen des Druckers werden aber ausgewertet.
9. **Abhängigkeiten:** F-01, INT-001.
10. **Anforderungs-IDs:** FR-003, FR-020, FR-021, NFR-004.
11. **Akzeptanzkriterien:** Akku und Signal erscheinen als % und Balken. „Kein Papier“ erscheint spätestens 10 s nach dem Entnehmen der Rolle bzw. sofort beim Druckversuch.

> **Hinweis aus der Protokollanalyse:** Laut TiMini-Print meldet der T02 „kein Papier“, „Deckel offen“, „überhitzt“ und den Akkustand. Ein eigener Code für **Papierstau** ist dort nicht bekannt. Wird im Prototyp geprüft und dem Benutzer vorgelegt.

## F-03 Etikett einrichten

1. **Zweck:** Größe des Ausdrucks und Ränder festlegen.
2. **Auslöser:** Neues Etikett, Änderung in den Feldern „Länge“ und „Ränder“.
3. **Voraussetzungen:** keine.
4. **Eingabedaten:** Länge in mm (eine Nachkommastelle), Ränder links, rechts, oben, unten in mm, jeweils einzeln.
5. **Verarbeitung:**
   1. Werte prüfen: Länge > 0, Ränder ≥ 0, Ränder oben + unten < bedruckbare Höhe, Ränder links + rechts < Länge.
   2. Druckbereich berechnen: Länge − Rand links − Rand rechts × bedruckbare Höhe − Rand oben − Rand unten.
   3. Bandansicht neu zeichnen.
   4. Ränder speichern (F-10).
6. **Ausgabedaten:** aktualisierte Bandansicht mit Bandrahmen (52 mm), bedruckbarem Bereich (48 mm) und Rändern.
7. **Fehlerfälle:** Ungültige Werte → Feld rot, Hinweis „Ränder sind größer als das Etikett“. Drucken ist gesperrt, bis die Werte gültig sind (FR-008).
8. **Alternative Abläufe:** Startwerte beim allerersten Start: Länge 40 mm, Ränder je 2 mm **[Vorschlag]**.
9. **Abhängigkeiten:** BR-001, F-10.
10. **Anforderungs-IDs:** FR-006, FR-008, FR-016, BR-001.
11. **Akzeptanzkriterien:** Vier getrennte Randfelder. Nach Neustart sind die letzten Ränder gesetzt. Die Bandansicht zeigt die Ränder maßstabsgetreu.

> **Begriffsklärung Rand oben/unten:** „Oben“ und „unten“ beziehen sich auf die Höhe (quer zum Band), „links“ und „rechts“ auf die Länge (Anfang und Ende des Etiketts).

## F-04 Bild einfügen

1. **Zweck:** Ein PNG- oder JPG-Bild auf das Etikett bringen.
2. **Auslöser:** Menü „Bild einfügen …“ oder Ziehen einer Datei in die Bandansicht **[Vorschlag]**.
3. **Voraussetzungen:** gültiges Etikett (F-03).
4. **Eingabedaten:** Datei .png, .jpg oder .jpeg.
5. **Verarbeitung:**
   1. Datei laden, Seitenverhältnis bestimmen. Ausrichtung aus EXIF-Daten von Fotos berücksichtigen **[Vorschlag]**.
   2. Bild **automatisch so groß wie möglich** in den Druckbereich einpassen, Seitenverhältnis beibehalten, zentriert.
   3. Element in der Bandansicht mit Anfassern an den Ecken anzeigen.
   4. Länge und Höhe des Elements in mm neben der Bandansicht anzeigen.
6. **Ausgabedaten:** Bildelement mit Position und Größe in mm.
7. **Fehlerfälle:** Anderes Format → „Dieses Format wird nicht unterstützt. Bitte PNG oder JPG verwenden.“ Defekte Datei → „Das Bild konnte nicht gelesen werden.“
8. **Alternative Abläufe:** Ein zweites Bild oder ein Text kann zusätzlich eingefügt werden (FR-013).
9. **Abhängigkeiten:** F-03, F-07 (Schwarz-Weiß-Darstellung in der Ansicht).
10. **Anforderungs-IDs:** FR-004, FR-005, FR-019.
11. **Akzeptanzkriterien:** Nach dem Einfügen füllt das Bild den Druckbereich in einer Richtung vollständig aus und ist nicht verzerrt.

## F-05 Element anpassen

1. **Zweck:** Größe, Position und Drehung eines Elements selbst festlegen.
2. **Auslöser:** Ziehen mit der Maus, Menü bzw. Schaltflächen „Drehen 0° / 90° / 180° / 270°“.
3. **Voraussetzungen:** Element ausgewählt.
4. **Eingabedaten:** Mausbewegung, gewählter Drehwinkel.
5. **Verarbeitung:**
   1. **Ecke ziehen:** Größe ändern, Seitenverhältnis bleibt fest.
   2. **Mitte ziehen:** Position ändern.
   3. **Drehen:** Element um 0°, 90°, 180° oder 270° drehen. Danach wird es **auf die vorgegebene Größe** skaliert, d. h. die aktuelle Länge × Höhe des Elements bleibt erhalten, soweit das Seitenverhältnis es erlaubt; sonst wird es in diese Fläche eingepasst.
   4. Position und Größe werden auf Druckerpunkte gerundet (1/8 mm), damit Ansicht und Ausdruck übereinstimmen.
   5. Ragt ein Element über die Ränder, wird es rot umrandet (FR-008).
6. **Ausgabedaten:** aktualisiertes Element, Maße in mm sichtbar.
7. **Fehlerfälle:** Element außerhalb des Druckbereichs → Drucken gesperrt mit Hinweis „Ein Element ragt über den Rand.“
8. **Alternative Abläufe:** Element löschen mit Entf-Taste **[Vorschlag]**.
9. **Abhängigkeiten:** F-04, F-06.
10. **Anforderungs-IDs:** FR-006, FR-017, FR-019, FR-008.
11. **Akzeptanzkriterien:** Bild wird nie verzerrt. Alle vier Drehwinkel wählbar. Gedruckte Maße entsprechen der Anzeige ±1 mm.

## F-06 Text mit Symbolen einfügen

1. **Zweck:** Text mit Sonderzeichen und Emojis drucken, allein oder zusammen mit einem Bild.
2. **Auslöser:** Menü „Text einfügen“.
3. **Voraussetzungen:** gültiges Etikett.
4. **Eingabedaten:** Text (mehrzeilig über Eingabetaste), Schriftart, Schriftgröße, fett, Ausrichtung links/mittig/rechts.
5. **Verarbeitung:**
   1. Textfeld öffnen. Symbolauswahl daneben mit häufigen Zeichen (° ² ³ € § ± × ÷ ✓ ✗ ★ ☆ ♥ → ← ↑ ↓ ☎ ⚠ ☀ ♪ ✂ ✉ und gängige Emojis).
   2. Klick auf ein Symbol fügt es an der Cursorposition ein.
   3. Text mit den gewählten Einstellungen rendern. Fehlt einem Zeichen die Schrift, nutzt Windows automatisch eine Ersatzschrift (Segoe UI Symbol, Segoe UI Emoji).
   4. Emojis werden einfarbig gerendert.
   5. Textblock als Element in die Bandansicht setzen, anpassbar wie in F-05.
6. **Ausgabedaten:** Textelement.
7. **Fehlerfälle:** Zeichen, das keine installierte Schrift darstellen kann → Warnung „Das Zeichen ‚…‘ kann nicht gedruckt werden.“ statt eines leeren Kästchens.
8. **Alternative Abläufe:** Schriftgröße in pt; die resultierende Zeichenhöhe in mm wird angezeigt **[Vorschlag zu Q-C4]**.
9. **Abhängigkeiten:** F-05, F-07.
10. **Anforderungs-IDs:** FR-009, FR-010, FR-013.
11. **Akzeptanzkriterien:** Der Testtext `° ² € § ± ✓ ✗ ★ ♥ → ☎ ⚠ 😀 👍` wird vollständig ohne Kästchen gedruckt. Größe, Schriftart, fett und Ausrichtung wirken sichtbar. Bild und Text erscheinen gemeinsam wie in der Ansicht.

## F-07 Schwarz-Weiß und Druckdichte einstellen

1. **Zweck:** Das Druckbild für den Thermodruck optimieren.
2. **Auslöser:** Verschieben eines der beiden Regler.
3. **Voraussetzungen:** keine.
4. **Eingabedaten:**
   - **Stärke für Schwarz:** 0–100 %, Startwert 50 % **[Vorschlag]**
   - **Druckdichte:** 0–100 %, Startwert 50 % **[Vorschlag]**
5. **Verarbeitung:**
   1. **Stärke für Schwarz:** Schwellwert für die Umwandlung. Jeder Bildpunkt wird in Helligkeit umgerechnet; ist er dunkler als die Schwelle, wird er schwarz. Höherer %-Wert → höhere Schwelle → mehr Fläche schwarz.
   2. Bandansicht zeigt sofort das Schwarz-Weiß-Ergebnis.
   3. **Druckdichte:** wird beim Drucken als Befehl an den T02 gesendet. Laut TiMini-Print kennt der T02 **4 Stufen**. Abbildung **[Vorschlag]**: 0–24 % → Stufe 1, 25–49 % → 2, 50–74 % → 3, 75–100 % → 4.
   4. Beide Werte werden gespeichert (F-10).
6. **Ausgabedaten:** Schwarz-Weiß-Vorschau, Dichtestufe für den Druckauftrag.
7. **Fehlerfälle:** keine.
8. **Alternative Abläufe:** Rasterung (Dithering) für Fotos ist nicht verlangt und wird nicht umgesetzt, sofern der Benutzer es nicht bestätigt.
9. **Abhängigkeiten:** F-08, INT-001.
10. **Anforderungs-IDs:** FR-007.
11. **Akzeptanzkriterien:** Zwei Regler mit Zahl daneben. Änderung der Schwelle ist sofort in der Ansicht sichtbar. Stufe 1 und Stufe 4 der Druckdichte sind auf dem Ausdruck deutlich unterscheidbar.

## F-08 Drucken

1. **Zweck:** Das Etikett exakt in der eingestellten Größe drucken.
2. **Auslöser:** Schaltfläche „Drucken“ oder Strg+P.
3. **Voraussetzungen:** Drucker „bereit“, Papier vorhanden, alle Elemente im Druckbereich.
4. **Eingabedaten:** Etikett mit Elementen, Ränder, Regler, Kalibrierfaktoren, Anzahl Exemplare (Standard 1) **[Vorschlag]**.
5. **Verarbeitung:**
   1. Prüfen: Elemente innerhalb der Ränder, Höhe ≤ 48 mm (FR-008).
   2. Statusabfrage (F-02). Kein Papier oder Deckel offen → Abbruch mit Meldung.
   3. Etikett in ein Raster rendern: **384 Punkte breit** (quer zum Band) × **Länge in mm × 8 × Kalibrierfaktor** Zeilen (entlang des Bandes). Die Bandansicht zeigt das Etikett liegend; für den Druck wird es so gedreht, dass die Länge zur Laufrichtung wird.
   4. Schwarz-Weiß-Umwandlung mit der eingestellten Schwelle.
   5. Druckauftrag erzeugen: Initialisierung, Druckdichte, Rasterdaten, Papiervorschub.
   6. In Paketen über Bluetooth senden, Fortschritt anzeigen.
   7. Fehler- und Statusmeldungen des Druckers während des Drucks auswerten.
6. **Ausgabedaten:** Ausdruck, Meldung „Gedruckt“.
7. **Fehlerfälle:** Papier aus oder Deckel offen während des Drucks → Abbruch, Meldung „Druck abgebrochen: kein Papier“. Kein automatischer Neudruck. Verbindungsabbruch → Meldung, Status „nicht verbunden“.
8. **Alternative Abläufe:** Mehrere Exemplare nacheinander **[Vorschlag]**.
9. **Abhängigkeiten:** F-01, F-02, F-03 bis F-07, F-09.
10. **Anforderungs-IDs:** FR-001, FR-004, FR-008, BR-001, BR-002, NFR-003.
11. **Akzeptanzkriterien:** Ein 40 × 40 mm großes Bild wird 40 × 40 mm ±1 mm groß gedruckt. Ränder stimmen ±1 mm.

## F-09 Kalibrieren

1. **Zweck:** Abweichungen von Druckkopf und Papiervorschub ausgleichen, damit ±1 mm sicher eingehalten wird.
2. **Auslöser:** Nur über das Menü „Kalibrierung“ (Q-A5b).
3. **Voraussetzungen:** Drucker bereit.
4. **Eingabedaten:** gemessene Länge und Höhe des Testrahmens in mm.
5. **Verarbeitung:**
   1. Testseite drucken: Rahmen mit **Sollmaß 80 mm Länge × 40 mm Höhe** **[Vorschlag]**, Maße als Text aufgedruckt, dazu mm-Teilstriche.
   2. Benutzer misst mit Lineal und trägt Länge und Höhe ein.
   3. Faktoren berechnen: Faktor Länge = Soll ÷ Ist (Länge), Faktor Höhe = Soll ÷ Ist (Höhe), jeweils mit den bisherigen Faktoren verrechnet.
   4. Plausibilität prüfen: Faktoren zwischen 0,9 und 1,1 **[Vorschlag]**.
   5. Faktoren speichern (F-10). Angebot: „Testseite erneut drucken zur Kontrolle“.
6. **Ausgabedaten:** gespeicherte Kalibrierfaktoren.
7. **Fehlerfälle:** Werte außerhalb der Plausibilität → „Bitte nachmessen. Der Wert weicht mehr als 10 % ab.“
8. **Alternative Abläufe:** „Kalibrierung zurücksetzen“ setzt beide Faktoren auf 1,0.
9. **Abhängigkeiten:** F-08, F-10.
10. **Anforderungs-IDs:** FR-012, NFR-003.
11. **Akzeptanzkriterien:** Nach der Kalibrierung weicht die erneut gedruckte Testseite höchstens ±1 mm ab. Faktoren bleiben nach Neustart erhalten.

> Die Höhe (quer) ist durch den festen Punktabstand des Druckkopfs bestimmt und weicht in der Regel kaum ab. Der Faktor wird trotzdem angeboten, weil die Anforderung beide Maße nennt.

## F-10 Einstellungen speichern und laden

1. **Zweck:** Ränder, Reglerwerte, Kalibrierung und den letzten Drucker dauerhaft behalten.
2. **Auslöser:** Jede Änderung der betroffenen Werte; Laden beim Programmstart.
3. **Voraussetzungen:** keine.
4. **Eingabedaten:** Ränder (4 Werte), Stärke für Schwarz, Druckdichte, Kalibrierfaktoren (2), letzter Drucker, letzte Länge.
5. **Verarbeitung:** Speichern als Datei im Benutzerprofil (`%APPDATA%\PhoMiniPrint\einstellungen.json`). Laden beim Start; fehlende oder defekte Datei → Standardwerte.
6. **Ausgabedaten:** Einstellungsdatei.
7. **Fehlerfälle:** Datei nicht lesbar → Standardwerte und Hinweis „Einstellungen wurden zurückgesetzt“.
8. **Alternative Abläufe:** keine.
9. **Abhängigkeiten:** F-03, F-07, F-09.
10. **Anforderungs-IDs:** FR-007, FR-012, FR-016.
11. **Akzeptanzkriterien:** Nach Neustart sind Ränder, Regler und Kalibrierung unverändert.

> Ob weitere Einstellungen (Schrift, letzte Größe, Vorlagen) gespeichert werden, ist Vorschlag FR-014 und nicht freigegeben.

## F-11 Versionsinformation anzeigen

1. **Zweck:** Erkennen, welche Version installiert ist.
2. **Auslöser:** Menü „Hilfe → Über PhoMiniPrint“ **[Vorschlag]**; Titelleiste **[Vorschlag]**.
3. **Voraussetzungen:** keine.
4. **Eingabedaten:** Versionsnummer aus dem Build.
5. **Verarbeitung:** Version nach Schema `MAJOR.MINOR.PATCH` lesen und anzeigen, dazu Build-Datum und Lizenzhinweise **[Vorschlag]**.
6. **Ausgabedaten:** Info-Fenster.
7. **Fehlerfälle:** keine.
8. **Alternative Abläufe:** keine.
9. **Abhängigkeiten:** F-12 (gleiche Nummer im Installer).
10. **Anforderungs-IDs:** FR-018.
11. **Akzeptanzkriterien:** Die Versionsnummer ist in der laufenden Software sichtbar und folgt `MAJOR.MINOR.PATCH`.

## F-12 Installieren

1. **Zweck:** Das Programm auf Windows 11 einrichten.
2. **Auslöser:** Doppelklick auf `PhoMiniPrint-Setup-<Version>.exe`.
3. **Voraussetzungen:** Windows 11.
4. **Eingabedaten:** Installationsordner (Standard: `C:\Program Files\PhoMiniPrint`).
5. **Verarbeitung:** Dateien kopieren (mit eingebauter .NET-Laufzeit), Startmenü-Eintrag anlegen, Deinstallationseintrag anlegen **[Vorschlag]**.
6. **Ausgabedaten:** installiertes Programm.
7. **Fehlerfälle:** keine Administratorrechte → Installation nur für den aktuellen Benutzer anbieten **[Vorschlag]**.
8. **Alternative Abläufe:** Neue Version über alte Version installieren; Einstellungen bleiben erhalten.
9. **Abhängigkeiten:** F-11.
10. **Anforderungs-IDs:** NFR-002, NFR-006.
11. **Akzeptanzkriterien:** Auf einem Windows-11-PC ohne .NET startet das Programm nach der Installation über das Startmenü.
