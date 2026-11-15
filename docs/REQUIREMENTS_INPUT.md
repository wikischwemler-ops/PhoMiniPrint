# REQUIREMENTS INPUT – PhoMiniPrint

> **Zweck:** Diese Datei dient als zentrale Eingabe für die strukturierte Analyse und Ausarbeitung von Softwareideen durch eine KI.
>
> **Herkunft:** Ausgefüllte Kopie von `REQUIREMENTS INPUT TEMPLATE.md`. Die Originalvorlage bleibt unverändert. Sämtliche Analyseergebnisse werden in separaten Dateien gespeichert.
>
> **Kennzeichnung:** Alles ohne Markierung stammt aus den Aussagen des Benutzers. Ergänzungen der KI sind mit **[KI-Hinweis, unbestätigt]** markiert und gelten erst nach Bestätigung.

---

# 1. Projektinformationen

- **Projektname:** PhoMiniPrint
- **Erstellt am:** 2026-11-15
- **Projektstatus:** IDEENSAMMLUNG
- **Version:** 1.0
- **Ansprechpartner:** wikischwemler-ops
- **Repository:** https://github.com/wikischwemler-ops/PhoMiniPrint (privat)

# 2. Grundidee

## 2.1 Beschreibung

Ein PC-Programm, mit dem man auf dem Mini-Thermodrucker **Phomemo T02** („Mini Drucker Pocket Printer für Smartphone, Inkless, Thermo“) drucken kann. Der Drucker verbindet sich per **Bluetooth** mit dem PC. Bilder sollen in **exakter Größe** gedruckt werden, und es soll auch Text mit Symbolen gedruckt werden können. Das Programm ist auf **Deutsch**.

## 2.2 Welches Problem soll gelöst werden?

Der Phomemo T02 ist für die Smartphone-App gedacht. Für den PC gibt es bereits Lösungen, die aber noch nicht gut genug sind.

Ausprobiert wurde **TiMini-Print** (https://github.com/Dejniel/TiMini-Print): Verbindung und Druck funktionieren mit dem T02. Es fehlt aber:

- Bilder lassen sich nicht auf eine **exakte Größe skalieren** und in genau dieser Größe drucken.
- Text mit **Symbolen** zu schreiben und zu drucken ist nicht wie gewünscht möglich.
- Das Programm ist nicht auf **Deutsch**.

## 2.3 Ziel der Software

Vom PC aus Bilder und Text auf dem Phomemo T02 drucken, wobei das Ergebnis auf dem Papier genau die eingestellte Größe hat.

## 2.4 Zielgruppe

Der Benutzer selbst (private Nutzung am eigenen PC).

# 3. Freie Ideensammlung

## Gedanken und Ideen

- TiMini-Print funktioniert grundsätzlich mit dem T02 und kann als Ausgangspunkt oder Vorlage für die Bluetooth-Kommunikation dienen.
- Bild beim Drucken auf die gewünschte Größe skalieren.
- Text schreiben, auch mit Symbolen, und drucken.

**[KI-Hinweis, unbestätigt]** Infos aus dem TiMini-Print-README:
- Python-Programm mit tkinter-Oberfläche, Lizenz Apache-2.0 (Weiterverwendung des Codes mit Namensnennung erlaubt).
- Druckt Bilder (PNG, JPG, GIF, BMP), PDF und Textdateien.
- Verbindung über Bluetooth Classic, unter Windows ist beim ersten Verbinden ggf. eine Kopplungsbestätigung nötig.
- Offene Wünsche dort: Druckvorschau (#40), Schwarz-Weiß-Vorverarbeitung von Bildern (#41).

**[KI-Hinweis, unbestätigt]** Technische Eckdaten des T02 (vor der Umsetzung am Gerät prüfen): Papierbreite ca. 53 mm, Druckbreite 384 Punkte, Auflösung ca. 203 dpi (≈ 8 Punkte pro mm). Daraus folgt eine maximale Druckbreite von ca. 48 mm.

## Beispiele und Anwendungsszenarien

- Ein Bild genau in einer vorgegebenen Größe ausdrucken.
- Einen Text mit Symbolen ausdrucken.

[Weitere konkrete Beispiele ergänzen, z. B. Etiketten, Aufkleber, Notizen, Maße.]

## Spätere Ergänzungen

[Weitere Gedanken jederzeit ergänzen.]

# 4. Unveränderliche Mindestanforderungen

> Diese Anforderungen sind verbindlich. Die KI darf sie nicht eigenständig entfernen, abschwächen oder durch andere Vorschläge ersetzen.

- [ ] Mindestanforderung 1: Das Programm läuft auf dem PC und druckt auf dem Phomemo T02.
- [ ] Mindestanforderung 2: Der Drucker verbindet sich über Bluetooth mit dem PC.
- [ ] Mindestanforderung 3: Bilder können auf eine exakte Größe skaliert und in genau dieser Größe gedruckt werden.
- [ ] Mindestanforderung 4: Text kann geschrieben und gedruckt werden, auch mit Symbolen.
- [ ] Mindestanforderung 5: Das Programm ist auf Deutsch.

Weitere Mindestanforderungen:

[Freitext]

# 5. Optionale Wünsche und Erweiterungen

- [ ] Wunsch 1:
- [ ] Wunsch 2:
- [ ] Wunsch 3:

Diese Punkte sind zunächst unverbindlich.

# 6. Benutzer und Rollen

Welche Benutzergruppen gibt es?

- Administrator: –
- Standardbenutzer: der Benutzer selbst
- Weitere Rollen: –

Welche Berechtigungen benötigen die jeweiligen Rollen?

[Freitext]

# 7. Arbeitsabläufe und Funktionen

## Ablauf 1 – Bild in exakter Größe drucken

- Auslöser: Benutzer will ein Bild ausdrucken.
- Eingaben: Bilddatei, gewünschte Größe.
- Verarbeitung: Bild auf die gewünschte Größe skalieren und an den T02 senden.
- Ergebnis: Ausdruck in genau der eingestellten Größe.
- Sonderfälle: [z. B. Bild breiter als die Druckbreite – noch offen]

## Ablauf 2 – Text mit Symbolen drucken

- Auslöser: Benutzer will einen Text ausdrucken.
- Eingaben: Text, auch mit Symbolen.
- Verarbeitung: Text aufbereiten und an den T02 senden.
- Ergebnis: Ausdruck des Textes mit den Symbolen.
- Sonderfälle: [noch offen]

## Ablauf 3 – Drucker verbinden

- Auslöser: Programmstart oder Druckauftrag.
- Eingaben: –
- Verarbeitung: T02 per Bluetooth finden und verbinden.
- Ergebnis: Drucker ist bereit.
- Sonderfälle: [z. B. Drucker aus, nicht gekoppelt – noch offen]

# 8. Benutzeroberfläche und Bedienung

- Gewünschte Plattform: PC
- Desktop / Web / Mobile: Desktop
- Bedienkonzept: [offen]
- Navigation: [offen]
- Designvorstellungen: [offen]
- Besondere Anforderungen: Oberfläche auf Deutsch

[Freitext]

# 9. Daten und Dateien

- Welche Daten werden verarbeitet? Bilder und Text (auch mit Symbolen).
- Welche Daten müssen gespeichert werden? [offen]
- Welche Dateiformate werden benötigt? [offen – welche Bildformate?]
- Gibt es Import- oder Exportfunktionen? [offen]
- Gibt es bestehende Datenbestände? [offen]

[Freitext]

# 10. Schnittstellen und externe Systeme

- Externe APIs: –
- Datenbanken: –
- Andere Software: TiMini-Print als Referenz (funktioniert mit dem T02)
- Datei- und Netzwerkzugriffe: lokale Bild- und Textdateien
- Sonstige Schnittstellen: Bluetooth zum Phomemo T02

[Freitext]

# 11. Technische Rahmenbedingungen

Nur bekannte Vorgaben eintragen.

- Programmiersprache: [offen]
- Framework: [offen]
- Betriebssystem: Windows 11 (PC des Benutzers)
- Datenbank: –
- Hosting: lokal, Quellcode auf GitHub (privat)
- Bestehende Infrastruktur: Windows-PC mit Bluetooth, Phomemo T02
- Technische Einschränkungen: [offen]

# 12. Qualität und Sicherheit

Bekannte Anforderungen:

- [ ] Benutzerverwaltung
- [ ] Berechtigungssystem
- [ ] Datensicherung
- [ ] Fehlerprotokollierung
- [ ] Datenschutz
- [ ] Performance
- [ ] Erweiterbarkeit
- [ ] Wartbarkeit
- [ ] Weitere:

# 13. Risiken und offene Gedanken

- „Exakte Größe“: In welcher Einheit wird die Größe angegeben (mm, cm, Pixel)? Nur Breite oder Breite und Höhe? Seitenverhältnis beibehalten?
- „Symbole“: Welche Symbole sind gemeint (Emojis, Sonderzeichen wie ° ² € ✓ ★, Pfeile, Icons)?
- Die maximale Druckbreite des T02 begrenzt, wie groß ein Bild quer gedruckt werden kann.
- Weiterentwicklung von TiMini-Print oder Neuentwicklung mit eigenem Code? (Lizenz Apache-2.0 erlaubt beides.)

# 14. Weitere Ergänzungen

[Freier Bereich für zusätzliche Informationen.]

---

# 15. VERBINDLICHE KI-ARBEITSANWEISUNGEN

## 15.1 Grundprinzip

Du bist Requirements Engineer, Softwareanalyst und technischer Planer.

Deine Aufgabe ist es, aus den gesammelten Ideen ein vollständiges, nachvollziehbares und technisch verwertbares Anforderungsdokument zu entwickeln.

Du darfst keine eigenen Annahmen als bestätigte Benutzeranforderungen ausgeben.

## 15.2 Analyse nur auf ausdrücklichen Befehl

Solange der Benutzer Ideen sammelt, darfst du keine vollständige Analyse beginnen.

Das verbindliche Startsignal lautet:

**ANALYSE STARTEN**

Erst nach diesem Befehl darfst du die gesamte Eingabedatei systematisch analysieren.

Berücksichtige dabei immer den vollständigen Inhalt einschließlich späterer Ergänzungen.

## 15.3 Analyseverfahren

Nach dem Startsignal:

1. Gesamte Eingabedatei lesen.
2. Alle Anforderungen und Gedanken erfassen.
3. Inhalt thematisch gruppieren.
4. Zusammengehörige Funktionen identifizieren.
5. Widersprüche und Abhängigkeiten erkennen.
6. Fehlende Informationen feststellen.
7. Unklare Anforderungen identifizieren.
8. Risiken dokumentieren.
9. Rückfragen thematisch gruppiert stellen.
10. Anforderungen strukturiert ausarbeiten.

Unterbreche den Benutzer nicht mit einer Rückfrage nach jedem einzelnen Gedanken.

## 15.4 Herkunft jeder Anforderung kennzeichnen

Jede Anforderung muss einer Herkunftskategorie zugeordnet werden.

**USER REQUIREMENT**

Direkte, ausdrücklich vom Benutzer formulierte Anforderung.

**DERIVED REQUIREMENT**

Logisch aus Benutzerangaben abgeleitete Anforderung. Die Herleitung muss dokumentiert werden.

**AI PROPOSAL**

Zusätzlicher Vorschlag der KI. Dieser gilt erst nach ausdrücklicher Bestätigung als angenommen.

KI-Vorschläge dürfen niemals stillschweigend zu verbindlichen Anforderungen werden.

## 15.5 Anforderungen eindeutig dokumentieren

Verwende eindeutige IDs:

- FR-001: Funktionale Anforderungen
- NFR-001: Nichtfunktionale Anforderungen
- BR-001: Geschäftsregeln
- INT-001: Schnittstellenanforderungen

Jede Anforderung enthält:

- ID
- Bezeichnung
- Beschreibung
- Herkunft
- Priorität
- Status
- Akzeptanzkriterien
- Abhängigkeiten
- Gegebenenfalls Begründung

Statuswerte:

- CONFIRMED
- OPEN
- PROPOSED
- REJECTED

Prioritäten:

- MUST
- SHOULD
- COULD
- LATER

## 15.6 Mindestanforderungen schützen

Alle unter Abschnitt 4 eingetragenen Mindestanforderungen sind besonders zu behandeln.

Sie dürfen nicht:

- entfernt werden,
- abgeschwächt werden,
- durch Alternativen ersetzt werden,
- ohne Zustimmung umformuliert werden, wenn sich dadurch ihre Bedeutung ändert.

Erkannte Konflikte müssen dem Benutzer ausdrücklich vorgelegt werden.

## 15.7 Ergebnisdateien

Erstelle die folgenden Dateien getrennt von dieser Eingabedatei:

| Datei | Inhalt |
|---|---|
| ANALYSIS_SUMMARY.md | Zusammenfassung der Analyse |
| OPEN_QUESTIONS.md | Offene Fragen und Entscheidungen |
| REQUIREMENTS_DRAFT.md | Erster Anforderungsentwurf |
| REQUIREMENTS.md | Fachlich freigegebene Anforderungen |
| FUNCTIONAL_DESIGN.md | Ausführliche Funktionsbeschreibungen |
| PROCESS_FLOWS.md | Prozessabläufe und Diagramme |
| ARCHITECTURE.md | Technische Architektur |
| IMPLEMENTATION_PLAN.md | Technischer Umsetzungsplan |

## 15.8 Funktionales Design

Die Datei FUNCTIONAL_DESIGN.md muss jede wichtige Funktion ausführlich beschreiben.

Für jede Funktion dokumentieren:

1. Zweck und Beschreibung
2. Auslöser
3. Voraussetzungen
4. Eingabedaten
5. Verarbeitung in Einzelschritten
6. Ausgabedaten
7. Fehlerfälle
8. Alternative Abläufe
9. Abhängigkeiten
10. Zugehörige Anforderungs-IDs
11. Akzeptanzkriterien

Eine bloße Aufzählung von Funktionen reicht nicht aus.

## 15.9 Prozessabläufe

Erstelle verständliche Prozessbeschreibungen und bei Bedarf Mermaid-Diagramme.

Alle wichtigen Abläufe müssen nachvollziehbar sein.

## 15.10 Architektur

Beschreibe:

- Softwarekomponenten
- Zuständigkeiten
- Datenflüsse
- Schnittstellen
- Abhängigkeiten
- Sicherheitsaspekte
- Erweiterungsmöglichkeiten

Trenne bestätigte Architekturentscheidungen von KI-Vorschlägen.

## 15.11 Schutz vorhandener Dateien

Diese Regel ist zwingend:

Die Originaldatei REQUIREMENTS_INPUT_TEMPLATE.md darf niemals verändert, überschrieben, umbenannt oder gelöscht werden.

Auch andere vorhandene Dateien dürfen nicht ungefragt überschrieben werden.

Wenn eine Zieldatei bereits existiert:

1. Benutzer informieren.
2. Nachfragen, ob eine Versionierung gewünscht ist.
3. Alternativ einen neuen Dateinamen mit Versionsnummer verwenden.

Keine eigenmächtigen Lösch- oder Überschreibaktionen.

---

# 16. FREIGABEPROZESS

Es existieren zwei strikt getrennte Freigabepunkte.

## Freigabe 1: Fachliche Freigabe

Verbindliches Kommando:

**FACHLICH FREIGEBEN**

Damit bestätigt der Benutzer die fachlichen Anforderungen und das funktionale Konzept.

Vorher dürfen offene fachliche Entscheidungen nicht als bestätigt behandelt werden.

Die fachliche Freigabe erlaubt noch keine Programmierung.

## Freigabe 2: Umsetzungsfreigabe

Verbindliches Kommando:

**UMSETZUNG FREIGEBEN**

Diese Freigabe erfolgt erst nach Prüfung des technischen Plans.

Erst danach darf mit der Programmierung begonnen werden.

Ohne diese ausdrückliche Freigabe ist jede Implementierung untersagt.

## 16.1 Regeln während der Umsetzung

Nach der Umsetzungsfreigabe:

- In nachvollziehbaren Arbeitsschritten entwickeln.
- Anforderungen und Architekturentscheidungen einhalten.
- Tests durchführen.
- Abweichungen dokumentieren.
- Risiken melden.
- Keine fachlichen Anforderungen eigenständig ändern.
- Bei notwendigen Änderungen zuerst Zustimmung einholen.

---

# 17. Ergebnispräsentation

Nach Abschluss der Analyse soll die KI eine verständliche Zusammenfassung präsentieren.

Diese soll enthalten:

- Grundidee und Ziel der Software
- Wichtigste Funktionsbereiche
- Zentrale Arbeitsabläufe
- Wesentliche Anforderungen
- Erkannte Risiken
- Offene Entscheidungen
- Vorgeschlagene nächste Schritte

Wenn eine Sprachfunktion verfügbar ist, kann die Zusammenfassung zusätzlich mündlich erläutert werden.

Die mündliche Erläuterung ersetzt niemals die schriftlichen Dokumente.

---

# 18. ABSCHLUSSREGEL

Die KI muss jederzeit zwischen folgenden Zuständen unterscheiden:

1. IDEENSAMMLUNG
2. ANALYSE
3. RÜCKFRAGEN
4. ANFORDERUNGSENTWURF
5. FACHLICHE PRÜFUNG
6. FACHLICH FREIGEGEBEN
7. TECHNISCHE PLANUNG
8. UMSETZUNGSPLAN GEPRÜFT
9. UMSETZUNG FREIGEGEBEN
10. IMPLEMENTIERUNG
11. TEST UND ABSCHLUSS

Ein Übergang in den nächsten Zustand erfolgt nur, wenn die dafür erforderlichen Voraussetzungen erfüllt sind.

**Grundsatz: Der Benutzer entscheidet. Die KI analysiert, strukturiert, dokumentiert und schlägt vor. Die KI erteilt sich niemals selbst eine Freigabe.**

---

**ENDE DER EINGABEVORLAGE**