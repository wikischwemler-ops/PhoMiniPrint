# Analyse – PhoMiniPrint

- **Grundlage:** [REQUIREMENTS_INPUT.md](REQUIREMENTS_INPUT.md), Stand 2026-11-15, vollständig gelesen
- **Projektstatus:** ANALYSE → RÜCKFRAGEN
- **Weitere Ergebnisse:** [OPEN_QUESTIONS.md](OPEN_QUESTIONS.md), [REQUIREMENTS_DRAFT.md](REQUIREMENTS_DRAFT.md)

---

## 1. Grundidee und Ziel

PhoMiniPrint ist ein deutschsprachiges PC-Programm für den Mini-Thermodrucker Phomemo T02. Es verbindet sich per Bluetooth mit dem Drucker und druckt:

- **Bilder in exakter Größe**: Der Ausdruck auf dem Papier hat genau die eingestellten Maße.
- **Text mit Symbolen.**

Auslöser ist TiMini-Print. Es funktioniert mit dem T02, kann aber weder in exakter Größe drucken noch Text mit Symbolen, und es ist nicht auf Deutsch.

## 2. Funktionsbereiche

| Bereich | Inhalt | Herkunft |
|---|---|---|
| A – Druckerverbindung | T02 per Bluetooth finden, verbinden, Zustand anzeigen | Benutzer (Mindestanforderung 2) |
| B – Bilddruck in exakter Größe | Bild laden, Zielgröße festlegen, exakt skalieren, in Schwarz-Weiß umwandeln, drucken | Benutzer (Mindestanforderung 3) |
| C – Textdruck mit Symbolen | Text eingeben, Symbole einfügen, Text als Druckbild erzeugen, drucken | Benutzer (Mindestanforderung 4) |
| D – Bedienoberfläche | Desktop-Programm, komplett auf Deutsch | Benutzer (Mindestanforderung 1 und 5) |
| E – Druckaufbereitung | gemeinsame Pipeline für A–C: Druckbild in Druckerauflösung, Begrenzung auf Druckbreite | abgeleitet |

## 3. Zentrale Abläufe

1. **Drucker verbinden:** Drucker suchen → verbinden → Status „bereit“.
2. **Bild in exakter Größe drucken:** Bild öffnen → Größe in mm eingeben → Vorschau → drucken.
3. **Text mit Symbolen drucken:** Text eingeben → Symbole einfügen → Schriftgröße wählen → drucken.

Abläufe 2 und 3 nutzen dieselbe Druckaufbereitung (Bereich E) und denselben Druckerzugang (Bereich A).

## 4. Kern des Problems „exakte Größe“

Damit „exakte Größe“ funktioniert, muss das Programm die physische Größe (z. B. 30 mm) in Druckerpunkte umrechnen:

> Punkte = Größe in mm × Punkte pro mm

Laut TiMini-Print und Herstellerangaben hat der T02 vermutlich **203 dpi ≈ 8 Punkte/mm** und **384 Punkte Druckbreite ≈ 48 mm**. **Diese Werte sind unbestätigt** und müssen am Gerät gemessen werden.

Folgerungen:

- **Quer zur Papierrichtung** ist die Größe auf die Druckbreite begrenzt (ca. 48 mm).
- **In Papierrichtung** ist die Länge praktisch nur durch die Rolle begrenzt.
- Der Papiervorschub des Druckers kann leicht von der Sollauflösung abweichen. Ohne eine **Kalibrierung** ist eine wirklich exakte Länge nicht sicher (Risiko R-02).
- Thermodruck kennt nur Schwarz und Weiß. Bilder müssen umgewandelt werden, entweder mit einer Schwelle oder mit Rasterung (Dithering).

## 5. Widersprüche und Abhängigkeiten

- **Kein Widerspruch** zwischen den Mindestanforderungen gefunden.
- **Abhängigkeit:** Mindestanforderung 3 (exakte Größe) hängt von der realen Druckerauflösung ab. Diese ist bisher nur angenommen.
- **Abhängigkeit:** Mindestanforderung 4 (Symbole) hängt davon ab, welche Symbole gemeint sind. Farbige Emojis lassen sich nur als Schwarz-Weiß-Umriss drucken.
- **Abhängigkeit:** Ob TiMini-Print weiterentwickelt oder neu geschrieben wird, bestimmt Sprache, Oberfläche und Aufwand.

## 6. Fehlende Informationen

Fehlende Informationen sind in [OPEN_QUESTIONS.md](OPEN_QUESTIONS.md) nach Themen gesammelt. Die wichtigsten:

- Einheit und Art der Größenangabe (nur Breite, Breite und Höhe, Seitenverhältnis)
- welche Symbole
- Druckrichtung (quer oder längs)
- Bildformate, PDF, Kombination aus Bild und Text
- TiMini-Print als Basis oder eigene Neuentwicklung

## 7. Risiken

| ID | Risiko | Auswirkung | Gegenmaßnahme (Vorschlag) |
|---|---|---|---|
| R-01 | Druckerauflösung weicht von 203 dpi ab | Ausdrucke falsch groß | Testdruck mit Messlineal vor der Umsetzung |
| R-02 | Papiervorschub ungenau (Länge) | Länge weicht ab, Breite stimmt | Kalibrierung mit Korrekturfaktor |
| R-03 | Bluetooth-Verbindung unter Windows unzuverlässig | Druck bricht ab | Verbindungsablauf von TiMini-Print als Vorbild, verständliche Fehlermeldungen |
| R-07 | Portierung des T02-Protokolls von Python nach C# (Entscheidung C#/.NET, 2026-11-15) | Druck funktioniert anfangs nicht, Mehraufwand | Zuerst ein kleiner C#-Prototyp nur für Verbinden und Testbild drucken, Ergebnis mit TiMini-Print vergleichen, erst danach Oberfläche bauen |
| R-04 | Farbige Emojis/Graustufen auf Thermodruck schlecht lesbar | Symbole unleserlich | Schwarz-Weiß-Schrift bzw. Dithering, Vorschau |
| R-05 | Gewünschte Größe breiter als ca. 48 mm | Bild kann nicht exakt gedruckt werden | Warnung und Angebot: drehen (längs drucken) oder verkleinern |
| R-06 | Codeübernahme aus TiMini-Print | Lizenzpflichten | Apache-2.0: Lizenz und NOTICE beilegen, Herkunft nennen |

## 8. Vorgeschlagene nächste Schritte

1. Benutzer beantwortet die Fragen in [OPEN_QUESTIONS.md](OPEN_QUESTIONS.md).
2. **[Vorschlag]** Testdruck mit TiMini-Print: ein 40 × 40 mm großes Bild mit 320 × 320 Pixeln ausdrucken und das Ergebnis messen. Damit sind R-01 und R-02 geklärt.
3. KI übernimmt die Antworten in [REQUIREMENTS_DRAFT.md](REQUIREMENTS_DRAFT.md) und setzt den Status der Anforderungen.
4. Fachliche Prüfung durch den Benutzer und danach „FACHLICH FREIGEBEN“.
5. Erst danach: FUNCTIONAL_DESIGN.md, PROCESS_FLOWS.md, ARCHITECTURE.md, IMPLEMENTATION_PLAN.md.
