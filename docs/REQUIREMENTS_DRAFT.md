# Anforderungsentwurf – PhoMiniPrint

- **Projektstatus:** ANFORDERUNGSENTWURF (vor der fachlichen Prüfung)
- **Grundlage:** [REQUIREMENTS_INPUT.md](REQUIREMENTS_INPUT.md), [ANALYSIS_SUMMARY.md](ANALYSIS_SUMMARY.md)
- **Offene Fragen:** [OPEN_QUESTIONS.md](OPEN_QUESTIONS.md)

## Legende

| Herkunft | Bedeutung |
|---|---|
| USER | ausdrücklich vom Benutzer formuliert |
| DERIVED | aus Benutzerangaben abgeleitet, Herleitung angegeben |
| AI PROPOSAL | Vorschlag der KI, gilt erst nach Bestätigung |

**Status:** CONFIRMED = vom Benutzer ausdrücklich verlangt · OPEN = Details offen oder abgeleitet, Bestätigung nötig · PROPOSED = KI-Vorschlag · REJECTED = abgelehnt
**Priorität:** MUST · SHOULD · COULD · LATER

**Mindestanforderungen** aus Abschnitt 4 der Eingabe sind mit 🔒 markiert. Sie dürfen nicht entfernt, abgeschwächt oder ersetzt werden.

## Übersicht

| ID | Bezeichnung | Herkunft | Prio | Status |
|---|---|---|---|---|
| FR-001 🔒 | Druck auf Phomemo T02 vom PC | USER | MUST | CONFIRMED |
| FR-002 🔒 | Verbindung über Bluetooth | USER | MUST | CONFIRMED |
| FR-003 | Verbindungsstatus anzeigen | DERIVED | MUST | OPEN |
| FR-004 🔒 | Bild auf exakte Größe skalieren und so drucken | USER | MUST | CONFIRMED |
| FR-005 | Bilddatei öffnen | DERIVED | MUST | OPEN |
| FR-006 | Länge und Höhe in mm, Seitenverhältnis bleibt | USER | MUST | CONFIRMED |
| FR-007 | Umwandlung in Schwarz-Weiß | DERIVED | MUST | OPEN |
| FR-008 | Prüfung gegen die bedruckbare Höhe | DERIVED | MUST | OPEN |
| FR-009 🔒 | Text schreiben und drucken, auch mit Symbolen | USER | MUST | CONFIRMED |
| FR-010 | Textformatierung | DERIVED | SHOULD | OPEN |
| FR-011 | Druckvorschau | AI PROPOSAL | SHOULD | PROPOSED |
| FR-012 | Kalibrierung mit Testseite, nur über Menü, gespeichert | USER | MUST | CONFIRMED |
| FR-013 | Bild und Text kombinieren | AI PROPOSAL | COULD | PROPOSED |
| FR-014 | Einstellungen merken | AI PROPOSAL | COULD | PROPOSED |
| FR-015 | Bild aus Zwischenablage | AI PROPOSAL | COULD | PROPOSED |
| FR-016 | Ränder einzeln je Seite, gespeichert | USER | MUST | CONFIRMED |
| FR-017 | Drehen 0/90/180/270° per Benutzerauswahl, immer auf Vorgabegröße | USER | MUST | CONFIRMED |
| FR-018 | Versionsinformation in der Software | USER | MUST | OPEN |
| FR-019 | Bandansicht, automatisch max. einpassen, Anpassen mit Maus | USER | MUST | CONFIRMED |
| NFR-001 🔒 | Programm auf Deutsch | USER | MUST | CONFIRMED |
| NFR-002 | Läuft unter Windows 11 | USER | MUST | CONFIRMED |
| NFR-003 | Maßgenauigkeit ±1 mm | USER | MUST | CONFIRMED |
| NFR-004 | Verständliche Fehlermeldungen | DERIVED | SHOULD | OPEN |
| NFR-005 | Lokal, ohne Cloud und Telemetrie | AI PROPOSAL | SHOULD | PROPOSED |
| NFR-006 | Auslieferung als Installer | USER | MUST | CONFIRMED |
| NFR-007 | Programmiersprache C# mit .NET (WPF) | USER | MUST | CONFIRMED |
| BR-001 | Größenbegrenzung durch Druckbreite | DERIVED | MUST | OPEN |
| BR-002 | Umrechnung mm → Druckerpunkte | DERIVED | MUST | OPEN |
| INT-001 | Bluetooth-Druckprotokoll des T02 | DERIVED | MUST | OPEN |
| INT-002 | TiMini-Print als Protokollreferenz (Portierung nach C#) | DERIVED | MUST | OPEN |

---

## Funktionale Anforderungen

### FR-001 🔒 Druck auf Phomemo T02 vom PC
- **Beschreibung:** Das Programm läuft auf dem PC und druckt auf dem Phomemo T02.
- **Herkunft:** USER (Mindestanforderung 1)
- **Priorität / Status:** MUST / CONFIRMED
- **Akzeptanzkriterien:** Ein am PC gestarteter Druckauftrag kommt vollständig auf dem T02 heraus.
- **Abhängigkeiten:** FR-002, INT-001

### FR-002 🔒 Verbindung über Bluetooth
- **Beschreibung:** Der Drucker verbindet sich über Bluetooth mit dem PC.
- **Herkunft:** USER (Mindestanforderung 2)
- **Priorität / Status:** MUST / CONFIRMED
- **Akzeptanzkriterien:** Das Programm findet einen eingeschalteten T02 in Reichweite, verbindet sich und druckt, ohne Kabel.
- **Abhängigkeiten:** INT-001

### FR-003 Verbindungsstatus anzeigen
- **Beschreibung:** Das Programm zeigt an, ob der Drucker gefunden und verbunden ist, und bietet „Suchen“ und „Verbinden“ an.
- **Herkunft:** DERIVED – ohne sichtbaren Status kann der Benutzer FR-002 nicht bedienen und Fehler nicht erkennen.
- **Priorität / Status:** MUST / OPEN
- **Akzeptanzkriterien:** Statusanzeige mit mindestens „nicht verbunden“, „verbindet …“, „bereit“, „Fehler“.
- **Abhängigkeiten:** FR-002

### FR-004 🔒 Bild auf exakte Größe skalieren und so drucken
- **Beschreibung:** Bilder können auf eine exakte Größe skaliert und in genau dieser Größe gedruckt werden.
- **Herkunft:** USER (Mindestanforderung 3; Hauptmangel von TiMini-Print)
- **Priorität / Status:** MUST / CONFIRMED
- **Akzeptanzkriterien:** Eingestellte Größe und gemessene Größe auf dem Papier stimmen im Rahmen von NFR-003 überein.
- **Abhängigkeiten:** FR-005, FR-006, FR-007, FR-008, BR-002, NFR-003

### FR-005 Bilddatei öffnen
- **Beschreibung:** Bilddateien vom PC laden.
- **Herkunft:** DERIVED – Voraussetzung für FR-004.
- **Priorität / Status:** MUST / OPEN (Formate: Q-B1)
- **Akzeptanzkriterien:** Mindestens PNG und JPG lassen sich öffnen. Weitere Formate laut Antwort Q-B1.
- **Abhängigkeiten:** –

### FR-006 Größe in physischer Einheit eingeben
- **Beschreibung:** Der Benutzer gibt die Zielgröße in **Millimetern** ein, nicht in Pixeln: die **Länge** entlang des Bandes und die **Höhe** quer zum Band. Die Höhe ist durch das Band (52 mm) begrenzt und muss nicht voll ausgenutzt werden.
  Das **Seitenverhältnis des Bildes bleibt erhalten** (keine Verzerrung).
- **Herkunft:** USER (Q-A1 Einheit mm, Q-A2 Länge/Höhe, Q-A2b Seitenverhältnis, alle 2026-11-15)
- **Priorität / Status:** MUST / CONFIRMED
- **Akzeptanzkriterien:** Eingabe von Länge und Höhe in mm, Werte mit mindestens einer Nachkommastelle. Eine Höhe über der bedruckbaren Höhe wird nach FR-008 gemeldet.
- **Abhängigkeiten:** BR-002

### FR-016 Ränder links/rechts und oben/unten
- **Beschreibung:** Ränder links, rechts, oben und unten sind **einzeln je Seite** in mm einstellbar. Das Bild bzw. der Text wird innerhalb dieser Ränder platziert. Die Randeinstellungen werden **gespeichert** und beim nächsten Start wieder verwendet.
- **Herkunft:** USER (Q-A2, Q-A6, 2026-11-15)
- **Priorität / Status:** MUST / CONFIRMED (Standardwerte beim allerersten Start: noch offen)
- **Akzeptanzkriterien:**
  - Vier getrennte Eingaben (links, rechts, oben, unten) in mm.
  - Gemessene Ränder auf dem Ausdruck entsprechen den eingestellten Werten ±1 mm (NFR-003).
  - Nach einem Neustart sind die zuletzt eingestellten Ränder wieder gesetzt.
- **Abhängigkeiten:** FR-006, BR-001, BR-002

### FR-007 Umwandlung in Schwarz-Weiß
- **Beschreibung:** Farb- und Graustufenbilder werden für den Thermodruck in reines Schwarz-Weiß umgewandelt.
- **Herkunft:** DERIVED – der Thermodrucker kann nur Schwarz/Weiß drucken.
- **Priorität / Status:** MUST / OPEN (Art und Regler: Q-B2)
- **Akzeptanzkriterien:** Fotos bleiben erkennbar, Linien und Text bleiben scharf.
- **Abhängigkeiten:** FR-004

### FR-008 Prüfung gegen die bedruckbare Höhe
- **Beschreibung:** Ist die eingestellte Höhe (plus Ränder oben/unten) größer als die bedruckbare Höhe, wird das vor dem Druck gemeldet. Das Programm verkleinert nicht stillschweigend.
- **Herkunft:** DERIVED – sonst würde FR-004 (exakte Größe) unbemerkt verletzt. Bestätigt durch Q-A4: Es wird immer auf die vorgegebene Größe skaliert.
- **Priorität / Status:** MUST / OPEN
- **Akzeptanzkriterien:** Bei zu großer Höhe erscheint ein Hinweis, und der Druck startet erst, wenn die Größe passt. Eine automatische Größenänderung findet nicht statt.
- **Abhängigkeiten:** BR-001, FR-016

### FR-017 Drehen per Benutzerauswahl
- **Beschreibung:** Der Benutzer wählt im Menü den Drehwinkel **0°, 90°, 180° oder 270°**. Das Programm dreht nie automatisch. Bei jedem Winkel wird das Bild **immer auf die vorgegebene Größe** (Länge × Höhe in mm) skaliert.
- **Herkunft:** USER (Q-A4 und Drehwinkel „alle vier“, 2026-11-15)
- **Priorität / Status:** MUST / CONFIRMED
- **Akzeptanzkriterien:** Alle vier Drehwinkel sind im Menü wählbar. Gedruckte Länge und Höhe entsprechen bei jedem Winkel der Vorgabe ±1 mm.
- **Abhängigkeiten:** FR-004, FR-006, NFR-003

### FR-019 Bandansicht beim Einfügen und Anpassen durch den Benutzer
- **Beschreibung:** Beim Einfügen eines Bildes zeigt das Programm den **Rahmen des Bandes** (Bandhöhe, Länge, Ränder). Das Bild wird zunächst **automatisch so groß wie möglich** innerhalb der Ränder eingepasst, mit **beibehaltenem Seitenverhältnis**. Danach passt der Benutzer es **mit der Maus** an: an den Ecken ziehen für die Größe, in der Mitte ziehen für die Position.
- **Herkunft:** USER (Q-A2b, Q-A2c Maus, Q-A2d automatisch maximal einpassen, alle 2026-11-15)
- **Priorität / Status:** MUST / CONFIRMED
- **Akzeptanzkriterien:**
  - Nach dem Einfügen sind Bandrahmen und Ränder sichtbar, und das Bild füllt die verfügbare Fläche innerhalb der Ränder maximal aus.
  - Größe und Position lassen sich mit der Maus ändern.
  - Das Bild wird beim Anpassen nie verzerrt.
  - Der Ausdruck entspricht der Anordnung in der Bandansicht ±1 mm (NFR-003).
- **Abhängigkeiten:** FR-006, FR-016, FR-017, FR-011
- **Hinweis:** Die Bandansicht übernimmt weitgehend die Rolle der vorgeschlagenen Druckvorschau (FR-011).

### FR-018 Versionsinformation in der Software
- **Beschreibung:** Die Software zeigt ihre Versionsangaben an.
- **Herkunft:** USER („beachte auch Versionsinfo – in der SW Angaben“, 2026-11-15)
- **Priorität / Status:** MUST / OPEN (Ort und weiterer Inhalt: Q-D5 a–c)
- **Akzeptanzkriterien:**
  - Die aktuelle Versionsnummer ist in der laufenden Software sichtbar.
  - Die Versionsnummer folgt dem Schema **`MAJOR.MINOR.PATCH`**, z. B. 1.0.0 (USER, Q-D5, 2026-11-15).
- **[KI-Vorschlag, unbestätigt] Details:**
  - eigenes Fenster „Info“ bzw. „Über PhoMiniPrint“ im Menü „Hilfe“
  - Inhalt: Programmname, Versionsnummer, Build-Datum, Lizenzhinweis (inkl. Apache-2.0-Hinweis für TiMini-Print als Protokollreferenz)
  - Versionsnummer zusätzlich in der Titelleiste
  - dieselbe Versionsnummer im Installer, in den Dateieigenschaften der EXE und in „Apps & Features“
- **Abhängigkeiten:** NFR-006

### FR-009 🔒 Text schreiben und drucken, auch mit Symbolen
- **Beschreibung:** Text kann geschrieben und gedruckt werden, auch mit Symbolen.
- **Herkunft:** USER (Mindestanforderung 4)
- **Priorität / Status:** MUST / CONFIRMED (welche Symbole: Q-C1)
- **Akzeptanzkriterien:** Eingegebener Text inklusive der in Q-C1 genannten Symbole erscheint vollständig und lesbar auf dem Ausdruck, ohne Kästchen oder Fragezeichen.
- **Abhängigkeiten:** FR-010, BR-002

### FR-010 Textformatierung
- **Beschreibung:** Schriftgröße und weitere Formatierung des Textes einstellen.
- **Herkunft:** DERIVED – Text muss auf die schmale Druckbreite passen.
- **Priorität / Status:** SHOULD / OPEN (Umfang: Q-C3, Q-C4)
- **Akzeptanzkriterien:** Mindestens die Schriftgröße ist einstellbar, mehrzeiliger Text wird umbrochen.
- **Abhängigkeiten:** FR-009

### FR-011 Druckvorschau
- **Beschreibung:** Vorschau des Druckbilds in Schwarz-Weiß, optional in Originalgröße am Bildschirm.
- **Herkunft:** AI PROPOSAL – vermeidet Fehldrucke und ist auch bei TiMini-Print gewünscht (Issue #40).
- **Priorität / Status:** SHOULD / PROPOSED (Q-D2)
- **Akzeptanzkriterien:** Die Vorschau zeigt Pixel für Pixel, was gedruckt wird.

### FR-012 Kalibrierung
- **Beschreibung:**
  1. Das Programm druckt eine **Testseite**, z. B. einen Rahmen mit aufgedruckten Sollmaßen in mm.
  2. Der Benutzer **misst** den Ausdruck nach und **trägt die gemessenen Werte ein**.
  3. Das Programm berechnet daraus Korrekturen für Länge und Höhe und **speichert** sie dauerhaft. Sie gelten ab dann für jeden Druck.
  4. Die Kalibrierung ist **nur** über ein eigenes **Kalibrierungsmenü** erreichbar. Das Programm schlägt sie nicht von selbst vor.
- **Herkunft:** USER (Q-A5, Q-A5b, 2026-11-15). Ursprünglich ein KI-Vorschlag, vom Benutzer bestätigt und konkretisiert.
- **Priorität / Status:** MUST / CONFIRMED
- **Akzeptanzkriterien:**
  - Die Testseite enthält einen Rahmen mit aufgedruckten Sollmaßen für Länge und Höhe.
  - Nach Eingabe der Messwerte weicht ein erneuter Druck der Testseite höchstens ±1 mm ab (NFR-003).
  - Die Kalibrierdaten bleiben nach einem Neustart des Programms erhalten.
- **Abhängigkeiten:** BR-002, NFR-003

### FR-013 Bild und Text kombinieren
- **Beschreibung:** Bild und Text auf einem gemeinsamen Ausdruck.
- **Herkunft:** AI PROPOSAL
- **Priorität / Status:** COULD / PROPOSED (Q-C5)

### FR-014 Einstellungen merken
- **Beschreibung:** Letzter Drucker, letzte Größe und Vorlagen bleiben gespeichert.
- **Herkunft:** AI PROPOSAL
- **Priorität / Status:** COULD / PROPOSED (Q-D3)

### FR-015 Bild aus Zwischenablage
- **Beschreibung:** Bild mit Strg+V einfügen.
- **Herkunft:** AI PROPOSAL
- **Priorität / Status:** COULD / PROPOSED (Q-B1)

---

## Nichtfunktionale Anforderungen

### NFR-001 🔒 Programm auf Deutsch
- **Beschreibung:** Das Programm ist auf Deutsch, also alle Texte, Menüs, Meldungen und Fehlermeldungen.
- **Herkunft:** USER (Mindestanforderung 5)
- **Priorität / Status:** MUST / CONFIRMED
- **Akzeptanzkriterien:** Kein englischer Text in der Oberfläche, auch nicht in Fehlermeldungen.

### NFR-002 Läuft unter Windows 11
- **Herkunft:** USER (2026-11-15)
- **Priorität / Status:** MUST / CONFIRMED
- **Akzeptanzkriterien:** Läuft auf Windows 11 mit eingebautem Bluetooth.

### NFR-003 Maßgenauigkeit
- **Beschreibung:** Höchstzulässige Abweichung zwischen eingestellter und gedruckter Größe.
- **Herkunft:** USER – Toleranz ±1 mm (Q-A3, 2026-11-15)
- **Priorität / Status:** MUST / CONFIRMED
- **Akzeptanzkriterien:** Länge, Höhe und Ränder weichen auf dem Ausdruck höchstens ±1 mm vom eingestellten Wert ab, gemessen mit Lineal.

### NFR-004 Verständliche Fehlermeldungen
- **Herkunft:** DERIVED – folgt aus NFR-001 und der Bluetooth-Verbindung (Drucker aus, kein Papier, außer Reichweite).
- **Priorität / Status:** SHOULD / OPEN
- **Akzeptanzkriterien:** Jede Fehlermeldung nennt Ursache und nächsten Schritt auf Deutsch.

### NFR-005 Lokal, ohne Cloud und Telemetrie
- **Herkunft:** AI PROPOSAL
- **Priorität / Status:** SHOULD / PROPOSED
- **Akzeptanzkriterien:** Keine Netzwerkverbindung außer Bluetooth zum Drucker. Keine automatische Update-Prüfung, die TiMini-Print standardmäßig macht.

### NFR-006 Auslieferung als Installer
- **Beschreibung:** Das Programm wird als Windows-Installer angeboten.
- **Herkunft:** USER (Q-F2, 2026-11-15)
- **Priorität / Status:** MUST / CONFIRMED
- **Akzeptanzkriterien:**
  - Ein Installer richtet das Programm auf Windows 11 ein, ohne dass der Benutzer vorher eine Laufzeitumgebung (z. B. .NET) oder Bibliotheken installieren muss.
  - Das Programm ist danach über das Startmenü erreichbar.
  - **[KI-Vorschlag, unbestätigt]** Deinstallation über „Apps & Features“ möglich; Desktop-Verknüpfung optional.

### NFR-007 Programmiersprache C# mit .NET (WPF)
- **Beschreibung:** Das Programm wird in **C# mit .NET** und der Oberfläche **WPF** entwickelt.
- **Herkunft:** USER (Q-F1, 2026-11-15). Die KI hatte Python mit Qt empfohlen; der Benutzer hat die Alternative C#/.NET gewählt.
- **Priorität / Status:** MUST / CONFIRMED (technische Vorgabe für ARCHITECTURE.md)
- **Folgen:**
  - TiMini-Print (Python) kann nicht direkt übernommen werden. Das T02-Druckprotokoll wird nach C# portiert (INT-002).
  - Bluetooth über die Windows-Schnittstellen bzw. eine .NET-Bibliothek (Auswahl in der technischen Planung).
  - Installer und EXE-Versionsinfo (FR-018, NFR-006) mit den üblichen .NET-Werkzeugen.

---

## Geschäftsregeln

### BR-001 Größenbegrenzung durch Druckbreite
- **Beschreibung:** Die **Höhe** (quer zum Band) ist mechanisch begrenzt: Bandhöhe 52 mm (USER, Q-A2). Bedruckbar ist davon vermutlich nur der Druckkopfbereich von 384 Punkten ≈ 48 mm (**unbestätigt**), also bleiben je Seite mindestens ca. 2 mm unbedruckbar. Die **Länge** (entlang des Bandes) ist nur durch die Rolle begrenzt.
- **Herkunft:** USER (Bandhöhe 52 mm) und DERIVED (Druckkopfbreite).
- **Priorität / Status:** MUST / OPEN (Wert am Gerät messen)

### BR-002 Umrechnung mm → Druckerpunkte
- **Beschreibung:** Punkte = Größe in mm × Punkte pro mm (vermutlich 8, **unbestätigt**), gegebenenfalls mit Kalibrierfaktor (FR-012). Das Bild wird genau einmal auf diese Punktzahl skaliert, und der Drucker druckt 1 Bildpunkt = 1 Druckerpunkt.
- **Herkunft:** DERIVED – Voraussetzung für FR-004.
- **Priorität / Status:** MUST / OPEN

---

## Schnittstellen

### INT-001 Bluetooth-Druckprotokoll des T02
- **Beschreibung:** Kommunikation mit dem T02 über dessen Bluetooth-Protokoll (laut TiMini-Print Bluetooth Classic, herstellerspezifisch, kein ESC/POS).
- **Herkunft:** DERIVED – aus FR-001, FR-002 und dem erfolgreichen Test von TiMini-Print.
- **Priorität / Status:** MUST / OPEN
- **Akzeptanzkriterien:** Bilddaten in voller Druckbreite werden fehlerfrei übertragen und gedruckt.

### INT-002 TiMini-Print als Protokollreferenz
- **Beschreibung:** TiMini-Print funktioniert nachweislich mit dem T02. Es dient als **Referenz für das Bluetooth-Druckprotokoll**, das nach C# portiert wird (NFR-007). Werden Teile davon sinngemäß übernommen, werden die Pflichten der Apache-2.0-Lizenz eingehalten (Lizenztext, NOTICE, Herkunftsangabe).
- **Herkunft:** DERIVED – Benutzer hat TiMini-Print als funktionierende Lösung genannt; eigenes Programm in C# laut Q-F1.
- **Priorität / Status:** MUST / OPEN
- **Akzeptanzkriterien:** Die C#-Umsetzung druckt auf dem T02 dieselben Testbilder wie TiMini-Print, ohne Abbrüche.
