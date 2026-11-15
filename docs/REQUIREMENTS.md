# Anforderungen – PhoMiniPrint (fachlich freigegeben)

- **Projektstatus:** FACHLICH FREIGEGEBEN → TECHNISCHE PLANUNG
- **Fachliche Freigabe:** durch den Benutzer mit „FACHLICH FREIGEBEN“ am 2026-11-15
- **Grundlage:** [REQUIREMENTS_DRAFT.md](REQUIREMENTS_DRAFT.md) in der Fassung vom 2026-11-15, [OPEN_QUESTIONS.md](OPEN_QUESTIONS.md)

## Wirkung der Freigabe

- **USER**-Anforderungen mit Status CONFIRMED: unverändert bestätigt.
- **DERIVED**-Anforderungen, die im Entwurf OPEN waren: mit der fachlichen Freigabe bestätigt (Status **CONFIRMED (Freigabe)**).
- **AI PROPOSAL** mit Status PROPOSED: **nicht** Teil der Freigabe. Sie bleiben Vorschläge und werden nur umgesetzt, wenn der Benutzer sie ausdrücklich bestätigt.
- **REJECTED**: wird nicht umgesetzt.
- Einzelwerte, die im Text als offen oder als KI-Vorschlag markiert sind (z. B. Startwerte, Standardränder), sind **Planungsannahmen**. Sie stehen in [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md) und bleiben bis zur Bestätigung änderbar.
- Änderungen an diesen Anforderungen nur mit Zustimmung des Benutzers (Abschnitt 16.1 der Vorlage).

## Legende

| Herkunft | Bedeutung |
|---|---|
| USER | ausdrücklich vom Benutzer formuliert |
| DERIVED | aus Benutzerangaben abgeleitet, Herleitung angegeben |
| AI PROPOSAL | Vorschlag der KI, gilt erst nach Bestätigung |

**Status:** CONFIRMED = vom Benutzer ausdrücklich verlangt · CONFIRMED (Freigabe) = abgeleitet, mit der fachlichen Freigabe bestätigt · PROPOSED = KI-Vorschlag · REJECTED = abgelehnt
**Priorität:** MUST · SHOULD · COULD · LATER

**Mindestanforderungen** aus Abschnitt 4 der Eingabe sind mit 🔒 markiert. Sie dürfen nicht entfernt, abgeschwächt oder ersetzt werden.

## Übersicht

| ID | Bezeichnung | Herkunft | Prio | Status |
|---|---|---|---|---|
| FR-001 🔒 | Druck auf Phomemo T02 vom PC | USER | MUST | CONFIRMED |
| FR-002 🔒 | Verbindung über Bluetooth | USER | MUST | CONFIRMED |
| FR-003 | Verbindungsstatus anzeigen | DERIVED | MUST | CONFIRMED (Freigabe) |
| FR-004 🔒 | Bild auf exakte Größe skalieren und so drucken | USER | MUST | CONFIRMED |
| FR-005 | Bilddatei öffnen (PNG, JPG) | DERIVED | MUST | CONFIRMED |
| FR-006 | Länge und Höhe in mm, Seitenverhältnis bleibt | USER | MUST | CONFIRMED |
| FR-007 | Schwarz-Weiß: Regler Umwandlung + Regler Druckdichte, je 0–100 % mit Zahl | USER | MUST | CONFIRMED |
| FR-008 | Prüfung gegen die bedruckbare Höhe | DERIVED | MUST | CONFIRMED (Freigabe) |
| FR-009 🔒 | Text mit Symbolen (Unicode + Emojis s/w, Symbolauswahl) | USER | MUST | CONFIRMED |
| FR-010 | Text: Größe, Schriftart, fett, Ausrichtung | USER | MUST | CONFIRMED |
| FR-011 | Druckvorschau | AI PROPOSAL | SHOULD | PROPOSED |
| FR-012 | Kalibrierung mit Testseite, nur über Menü, gespeichert | USER | MUST | CONFIRMED |
| FR-013 | Bild und Text auf einem Ausdruck | USER | MUST | CONFIRMED |
| FR-014 | Einstellungen merken | AI PROPOSAL | COULD | PROPOSED |
| FR-015 | Bild aus Zwischenablage | AI PROPOSAL | COULD | REJECTED |
| FR-016 | Ränder einzeln je Seite, gespeichert | USER | MUST | CONFIRMED |
| FR-017 | Drehen 0/90/180/270° per Benutzerauswahl, immer auf Vorgabegröße | USER | MUST | CONFIRMED |
| FR-018 | Versionsinformation in der Software | USER | MUST | CONFIRMED (Freigabe) |
| FR-019 | Bandansicht, automatisch max. einpassen, Anpassen mit Maus | USER | MUST | CONFIRMED |
| FR-020 | Akkustand und Papierstatus (Papierstau, kein Papier) im Status | USER | MUST | CONFIRMED |
| FR-021 | Bluetooth-Signalstärke im Status (% und Balken) | USER | MUST | CONFIRMED |
| NFR-001 🔒 | Programm auf Deutsch | USER | MUST | CONFIRMED |
| NFR-002 | Läuft unter Windows 11 | USER | MUST | CONFIRMED |
| NFR-003 | Maßgenauigkeit ±1 mm | USER | MUST | CONFIRMED |
| NFR-004 | Verständliche Fehlermeldungen | DERIVED | SHOULD | CONFIRMED (Freigabe) |
| NFR-005 | Lokal, ohne Cloud und Telemetrie | AI PROPOSAL | SHOULD | PROPOSED |
| NFR-006 | Auslieferung als Installer | USER | MUST | CONFIRMED |
| NFR-007 | Programmiersprache C# mit .NET (WPF) | USER | MUST | CONFIRMED |
| BR-001 | Größenbegrenzung durch Druckbreite | DERIVED | MUST | CONFIRMED (Freigabe) |
| BR-002 | Umrechnung mm → Druckerpunkte | DERIVED | MUST | CONFIRMED (Freigabe) |
| INT-001 | Bluetooth-Druckprotokoll des T02 | DERIVED | MUST | CONFIRMED (Freigabe) |
| INT-002 | TiMini-Print als Protokollreferenz (Portierung nach C#) | DERIVED | MUST | CONFIRMED (Freigabe) |

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
- **Priorität / Status:** MUST / CONFIRMED (Freigabe 2026-11-15)
- **Akzeptanzkriterien:** Statusanzeige mit mindestens „nicht verbunden“, „verbindet …“, „bereit“, „Fehler“.
- **Abhängigkeiten:** FR-002

### FR-020 Akkustand und Papierstatus im Status
- **Beschreibung:** Im Druckerstatus werden zusätzlich der **Akkustand** des T02 (in % und als Balken) und der **Papierstatus** angezeigt, insbesondere **Papierstau** und **kein Papier**. Bei einem Papierproblem wird der Druck nicht gestartet bzw. mit einer deutschen Meldung abgebrochen.
- **Herkunft:** USER (Q-E3, 2026-11-15)
- **Priorität / Status:** MUST / CONFIRMED
- **Akzeptanzkriterien:**
  - Der Akkustand ist bei verbundenem Drucker sichtbar, **als Zahl in % und zusätzlich als Balkenanzeige** (USER, 2026-11-15).
  - Papierstau bzw. fehlendes Papier wird angezeigt, sobald der Drucker es meldet.
- **Risiko:** Ob und wie genau der T02 Akku und Papierzustand über Bluetooth meldet, ist noch nicht belegt. Wird im Prototyp geprüft (INT-001). Meldet der Drucker einen Wert nicht, wird das dem Benutzer zur Entscheidung vorgelegt.
- **Abhängigkeiten:** FR-003, INT-001

### FR-021 Bluetooth-Signalstärke im Status
- **Beschreibung:** Im Druckerstatus wird die **Signalstärke der Bluetooth-Verbindung** zum T02 angezeigt, ähnlich wie der Akkustand: **als Zahl in % und als Balkenanzeige**.
- **Herkunft:** USER (2026-11-15)
- **Priorität / Status:** MUST / CONFIRMED
- **Akzeptanzkriterien:**
  - Bei verbundenem oder gefundenem Drucker ist die Signalstärke als % und als Balken sichtbar.
  - Entfernt man den Drucker vom PC, nimmt die Anzeige erkennbar ab.
- **Risiko:** Der T02 nutzt Bluetooth Classic. Windows liefert die Signalstärke (RSSI) für Classic-Geräte nicht zuverlässig über eine öffentliche Schnittstelle, teils nur während der Gerätesuche. Wird im Prototyp geprüft (INT-001). Lässt sie sich während der Verbindung nicht messen, wird dem Benutzer eine Alternative zur Entscheidung vorgelegt (z. B. Anzeige nur bei der Suche oder Verbindungsqualität aus Übertragungsfehlern).
- **Abhängigkeiten:** FR-003, FR-020, INT-001

### FR-004 🔒 Bild auf exakte Größe skalieren und so drucken
- **Beschreibung:** Bilder können auf eine exakte Größe skaliert und in genau dieser Größe gedruckt werden.
- **Herkunft:** USER (Mindestanforderung 3; Hauptmangel von TiMini-Print)
- **Priorität / Status:** MUST / CONFIRMED
- **Akzeptanzkriterien:** Eingestellte Größe und gemessene Größe auf dem Papier stimmen im Rahmen von NFR-003 überein.
- **Abhängigkeiten:** FR-005, FR-006, FR-007, FR-008, BR-002, NFR-003

### FR-005 Bilddatei öffnen
- **Beschreibung:** Bilddateien im Format **PNG und JPG** vom PC laden.
- **Herkunft:** DERIVED – Voraussetzung für FR-004. Formate: USER (Q-B1, 2026-11-15).
- **Priorität / Status:** MUST / CONFIRMED
- **Akzeptanzkriterien:** PNG- und JPG-Dateien (.png, .jpg, .jpeg) lassen sich öffnen. Andere Formate werden mit einer deutschen Meldung abgelehnt.
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
- **Beschreibung:** Farb- und Graustufenbilder werden für den Thermodruck in reines Schwarz-Weiß umgewandelt. Es gibt **zwei getrennte Schieberegler**, jeweils **0–100 %** mit **sichtbarem Zahlenwert**:
  1. **Stärke für Schwarz (Umwandlung):** ab welcher Helligkeit ein Bildpunkt schwarz wird; mehr % = mehr Fläche wird schwarz.
  2. **Druckdichte:** wie dunkel der Drucker druckt (Hitze des Druckkopfs).
- **Herkunft:** DERIVED (Thermodruck kann nur Schwarz/Weiß) und USER (Regler 0–100 % mit Zahl: Q-B2; zwei Regler: Q-B2b, beide 2026-11-15)
- **Priorität / Status:** MUST / CONFIRMED (Startwerte/Speichern: Q-B2c offen)
- **Akzeptanzkriterien:**
  - Zwei Schieberegler von 0 bis 100 %, der eingestellte Wert steht jeweils als Zahl daneben.
  - Eine Änderung der Umwandlung ist in der Bandansicht (FR-019) sofort sichtbar. Eine Änderung der Druckdichte ist auf dem Ausdruck erkennbar heller bzw. dunkler.
  - Fotos bleiben erkennbar, Linien und Text bleiben scharf.
- **Risiko:** Ob und in wie vielen Stufen der T02 die Druckdichte über Bluetooth einstellen lässt, ist noch nicht belegt; wird im Prototyp geprüft (INT-001). Die 0–100 % werden dann auf die verfügbaren Stufen abgebildet.
- **Abhängigkeiten:** FR-004, FR-019, INT-001

### FR-008 Prüfung gegen die bedruckbare Höhe
- **Beschreibung:** Ist die eingestellte Höhe (plus Ränder oben/unten) größer als die bedruckbare Höhe, wird das vor dem Druck gemeldet. Das Programm verkleinert nicht stillschweigend.
- **Herkunft:** DERIVED – sonst würde FR-004 (exakte Größe) unbemerkt verletzt. Bestätigt durch Q-A4: Es wird immer auf die vorgegebene Größe skaliert.
- **Priorität / Status:** MUST / CONFIRMED (Freigabe 2026-11-15)
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
- **Priorität / Status:** MUST / CONFIRMED (Freigabe 2026-11-15) (Ort und weiterer Inhalt: Q-D5 a–c)
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
- **Priorität / Status:** MUST / CONFIRMED
- **Symbolumfang (Q-C1, 2026-11-15):** vom Benutzer an die KI übertragen („frei wählbar für dich“). **KI-Festlegung im Auftrag des Benutzers**, jederzeit änderbar:
  - Unicode-Sonderzeichen aus *Segoe UI* / *Segoe UI Symbol*, z. B. ° ² ³ € § ± × ÷ ✓ ✗ ★ ☆ ♥ → ← ↑ ↓ ☎ ⚠ ☀ ♪ ✂ ✉
  - Emojis aus *Segoe UI Emoji*, gedruckt in Schwarz-Weiß
  - Icons/Piktogramme außerhalb von Unicode: LATER
  - Eingabe per Tastatur/Einfügen und über eine Symbolauswahl im Programm (Q-C2)
- **Akzeptanzkriterien:**
  - Ein Testtext mit `° ² € § ± ✓ ✗ ★ ♥ → ☎ ⚠ 😀 👍` erscheint vollständig und lesbar auf dem Ausdruck, ohne Kästchen oder Fragezeichen.
  - Die Symbolauswahl fügt ein gewähltes Symbol an der Cursorposition ein.
- **Abhängigkeiten:** FR-010, BR-002

### FR-010 Textformatierung
- **Beschreibung:** Einstellbar sind genau: **Schriftgröße, Schriftart, fett, Ausrichtung** (links, mittig, rechts).
- **Herkunft:** USER (Q-C3, 2026-11-15)
- **Priorität / Status:** MUST / CONFIRMED (Text in exakter Größe: Q-C4 offen)
- **Akzeptanzkriterien:** Alle vier Einstellungen sind in der Oberfläche vorhanden und wirken sichtbar in der Bandansicht und auf dem Ausdruck. **[KI-Hinweis, unbestätigt]** Zeilenumbruch bei zu langem Text und kursiv sind nicht verlangt; mehrzeiliger Text wird vorerst durch Eingabe von Zeilenumbrüchen ermöglicht.
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
- **Beschreibung:** Bild und Text können auf einem gemeinsamen Ausdruck stehen, z. B. Logo mit Beschriftung. Beide werden in der Bandansicht platziert (FR-019).
- **Herkunft:** USER (Q-C5, 2026-11-15). Ursprünglich KI-Vorschlag.
- **Priorität / Status:** MUST / CONFIRMED
- **Akzeptanzkriterien:** Ein Ausdruck mit einem Bild und einem Text erscheint so, wie er in der Bandansicht angeordnet ist, ±1 mm (NFR-003).
- **Abhängigkeiten:** FR-004, FR-009, FR-019

### FR-014 Einstellungen merken
- **Beschreibung:** Letzter Drucker, letzte Größe und Vorlagen bleiben gespeichert.
- **Herkunft:** AI PROPOSAL
- **Priorität / Status:** COULD / PROPOSED (Q-D3)

### FR-015 Bild aus Zwischenablage
- **Beschreibung:** Bild mit Strg+V einfügen.
- **Herkunft:** AI PROPOSAL
- **Priorität / Status:** COULD / REJECTED (Q-B1, 2026-11-15: PNG und JPG reichen)

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
- **Priorität / Status:** SHOULD / CONFIRMED (Freigabe 2026-11-15)
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
- **Priorität / Status:** MUST / CONFIRMED (Freigabe 2026-11-15) (Wert am Gerät messen)

### BR-002 Umrechnung mm → Druckerpunkte
- **Beschreibung:** Punkte = Größe in mm × Punkte pro mm (vermutlich 8, **unbestätigt**), gegebenenfalls mit Kalibrierfaktor (FR-012). Das Bild wird genau einmal auf diese Punktzahl skaliert, und der Drucker druckt 1 Bildpunkt = 1 Druckerpunkt.
- **Herkunft:** DERIVED – Voraussetzung für FR-004.
- **Priorität / Status:** MUST / CONFIRMED (Freigabe 2026-11-15)

---

## Schnittstellen

### INT-001 Bluetooth-Druckprotokoll des T02
- **Beschreibung:** Kommunikation mit dem T02 über dessen Bluetooth-Protokoll (laut TiMini-Print Bluetooth Classic, herstellerspezifisch, kein ESC/POS).
- **Herkunft:** DERIVED – aus FR-001, FR-002 und dem erfolgreichen Test von TiMini-Print.
- **Priorität / Status:** MUST / CONFIRMED (Freigabe 2026-11-15)
- **Ist-Stand (2026-11-15):** T02 ist in Windows gekoppelt („T02“, Status OK, Q-E2). Windows stellt zwei Bluetooth-COM-Ports bereit (COM9, COM10). Ob die Verbindung über einen dieser seriellen Ports oder direkt per RFCOMM-Socket läuft, klärt der Prototyp.
- **Akzeptanzkriterien:** Bilddaten in voller Druckbreite werden fehlerfrei übertragen und gedruckt.

### INT-002 TiMini-Print als Protokollreferenz
- **Beschreibung:** TiMini-Print funktioniert nachweislich mit dem T02. Es dient als **Referenz für das Bluetooth-Druckprotokoll**, das nach C# portiert wird (NFR-007). Werden Teile davon sinngemäß übernommen, werden die Pflichten der Apache-2.0-Lizenz eingehalten (Lizenztext, NOTICE, Herkunftsangabe).
- **Herkunft:** DERIVED – Benutzer hat TiMini-Print als funktionierende Lösung genannt; eigenes Programm in C# laut Q-F1.
- **Priorität / Status:** MUST / CONFIRMED (Freigabe 2026-11-15)
- **Akzeptanzkriterien:** Die C#-Umsetzung druckt auf dem T02 dieselben Testbilder wie TiMini-Print, ohne Abbrüche.
