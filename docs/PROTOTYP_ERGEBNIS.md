# Ergebnis Prototyp M1 – Phomemo T02

- **Datum:** 2026-11-15
- **Werkzeug:** `tools/T02Probe` (Konsolenprogramm, ohne Oberfläche)
- **Gerät:** Phomemo T02, Firmware 3.2.10, gekoppelt mit Windows 11
- **Bezug:** [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md) Abschnitt M1, [REQUIREMENTS.md](REQUIREMENTS.md)

---

## Zusammenfassung

| Test | Prüft | Ergebnis |
|---|---|---|
| T1 | COM-Port und Verbindung | ✅ Automatisch gefunden (COM10) über die Bluetooth-Adresse |
| T2 | Akku abfragen | ✅ funktioniert, Wert braucht Glättung |
| T3 | Papier und Deckel | ✅ „kein Papier“ sofort erkannt · ⚠️ kein Deckelsensor |
| T4 | 40 × 40 mm | ✅ **genau 40 × 40 mm** (vom Benutzer gemessen) |
| T5 | Volle Breite | ✅ 384 Punkte vollständig bedruckt · Band **50 mm** |
| T6 | Druckdichte 1–4 | ⚠️ Stufe 1 deutlich heller, Stufe 2–4 kaum unterscheidbar |
| T7 | Signalstärke | ❌ Windows liefert keinen echten Wert |
| T8 | Papierstau | ❌ keine eigene Meldung |

**Kernaussage:** Die wichtigste Anforderung, der Druck in exakter Größe (FR-004, NFR-003), ist am echten Gerät bestätigt. Verbindung, Druck, Akku und Papierstatus funktionieren. Für Signalstärke (FR-021) und Papierstau (FR-020) liefert die Hardware keine Daten. Dazu sind **Entscheidungen des Benutzers** nötig (Abschnitt 4).

---

## 1. Verbindung (T1)

- Der T02 erscheint in Windows als Gerät „T02“ und stellt einen **ausgehenden Bluetooth-COM-Port** bereit (hier COM10).
- Die Zuordnung erfolgt über die Bluetooth-Adresse in der Windows-Geräte-ID (`T02Suche`), nicht über eine feste Portnummer.
- **Befund:** Direkt nach dem Schließen einer Verbindung schlägt das erneute Öffnen einige Sekunden lang mit einer Zeitüberschreitung fehl. → `SppVerbindung.Oeffnen` versucht es bis zu viermal im Abstand von 3 s.
- **Befund:** Der T02 schaltet sich nach 60 min ohne Benutzung ab (Meldung `1A 09 0C` = 12 × 5 min). Danach schlägt das Öffnen fehl, bis er wieder eingeschaltet ist.
- Windows zeigt den T02 zusätzlich unter „Drucker und Scanner“ mit „Treiber nicht verfügbar“ an. Das ist ohne Bedeutung: PhoMiniPrint nutzt keinen Windows-Druckertreiber.

## 2. Statusabfragen (T2, T3)

Am T02 gemessene Zuordnung (weicht teilweise von TiMini-Print ab):

| Abfrage | Antwort | Bedeutung |
|---|---|---|
| `1F 11 08` | `1A 04 xx` | **Akku** in % |
| `1F 11 11` | `1A 06 88` / `1A 06 89` | **kein Papier** / Papier vorhanden |
| `1F 11 12` | `1A 05 98` | Deckel „zu“ – ändert sich nie |
| `1F 11 07` | `1A 07 03 02 0A` | Firmware 3.2.10 |
| `1F 11 09` | `1A 08` + 15 Zeichen | Seriennummer (wird in allen Ausgaben ausgeblendet) |
| `1F 11 0E` | `1A 3B …` + `1A 09 0C` | Fähigkeiten, Abschaltzeit 60 min |
| `1F 11 38` | `1A 17 03` | Chip-Typ 3 |
| nach jedem Druck | `1A 0F 0C` | **Druck fertig** (kommt erst nach vollständigem Druck) |

**Akku:**
- Direkt nach vier Druckaufträgen meldete der T02 **34 %**, eine Minute später wieder **63 %**. Der Wert folgt offenbar der Spannung, die beim Drucken einbricht.
- → Die Anzeige muss **geglättet** werden. Vorschlag: gleitender Mittelwert, und nach einem Druck 60 s lang den letzten Wert vor dem Druck anzeigen.

**Papier und Deckel:**
- Rolle entnehmen → sofort „kein Papier“, Rolle einlegen → sofort „Papier vorhanden“.
- Deckel öffnen erzeugt **keine** eigene Meldung. Der T02 hat offenbar keinen Deckelsensor; ein offener Deckel ohne Rolle zeigt sich als „kein Papier“.

## 3. Drucken (T4, T5, T6)

**Maße:**
- Quadrat 40 × 40 mm: vom Benutzer gemessen **genau 40 × 40 mm**, auch im Gesamttest (Foto).
- Die Werte **8 Punkte/mm** und **384 Punkte Druckbreite** sind damit bestätigt.
- Bandbreite laut Benutzer **50 mm** (bisher 52 mm angenommen). Die volle Druckbreite von 48 mm wird vollständig gedruckt.

**Ende des Ausdrucks:**
- Ohne Nachlauf fehlte die letzte Seite des Rahmens in Vorschubrichtung: Sie blieb zwischen Druckkopf und Abreißkante im Drucker.
- → Nach jedem Bild werden **7 mm Leerzeilen** angehängt (`T02Befehle.Nachlaufzeilen`). 12 mm funktionierten; der Benutzer hat 7 mm als ausreichend festgelegt.
- Die Verbindung darf erst nach dem Druckende geschlossen werden (`AufDruckendeWarten`). Besser ist das Warten auf die Meldung `1A 0F 0C`; das kommt in M2.

**Druckdichte:**
- Die Befehle für Stufe 1–4 werden angenommen.
- Die Dichte lässt sich **innerhalb eines Auftrags umschalten** (Gesamttest: vier Felder mit verschiedener Stufe in einem Auftrag).
- Sichtbar: **Stufe 1 deutlich heller und körnig**, Stufe 2, 3 und 4 kaum zu unterscheiden.

**Papier sparen (Wunsch des Benutzers):**
- Testmuster werden künftig in **einem** Auftrag zusammengefasst (`T02Probe gesamt`).

## 4. Offene Punkte – Entscheidung des Benutzers nötig

### E-1 Signalstärke (FR-021)

**Befund:** Windows meldet für den T02 nur feste Werte (0 dBm bzw. −10 dBm), auch bei bestehender Verbindung. Eine echte Messung (RSSI) ist für Bluetooth-Classic-Geräte über die Windows-Schnittstellen nicht verfügbar. Die Abfrage blieb zudem nach zwei Messungen hängen.

| Option | Beschreibung |
|---|---|
| A | **Verbindungsqualität statt Signalstärke:** % und Balken aus Antwortzeit und Fehlerquote der Statusabfragen. Zeigt, ob die Verbindung gut ist, ist aber keine Funkmessung. |
| B | Anzeige nur „verbunden / nicht verbunden“, FR-021 streichen |
| C | FR-021 auf später verschieben |

**Empfehlung:** A, weil sie dem Benutzer dieselbe Aussage liefert („reicht die Verbindung?“).

### E-2 Papierstau und Deckel (FR-020)

**Befund:** Der T02 meldet nur „kein Papier“. Für Papierstau und Deckel gibt es keine Meldung.

| Option | Beschreibung |
|---|---|
| A | Anzeige „kein Papier oder Deckel offen“; bei ausbleibender Meldung „Druck fertig“ (`1A 0F 0C`) Hinweis „Druck nicht abgeschlossen – Papierstau?“ |
| B | Nur „kein Papier“ anzeigen, Papierstau aus FR-020 streichen |

**Empfehlung:** A, weil sie Papierstau indirekt erkennbar macht.

### E-3 Druckdichte (FR-007)

**Befund:** Nur Stufe 1 ist sichtbar heller.

| Option | Beschreibung |
|---|---|
| A | Regler bleibt 0–100 % mit 4 Stufen wie geplant |
| B | Regler mit 2 sinnvollen Stufen (hell / normal), Rest entfällt |
| C | Weitere Dichtewerte testen (z. B. Koeffizient > 150), dann entscheiden |

**Empfehlung:** C, weil ein kurzer Test auf einem Streifen klären kann, ob der T02 feinere Abstufungen kann.

## 5. Übernahme in den Code

| Befund | Umsetzung |
|---|---|
| Port-Zuordnung über Adresse | `T02Suche` |
| Wiederholversuche beim Öffnen | `SppVerbindung.Oeffnen` |
| Mehrbyte-Meldungen (07, 08, 3B) | `T02Antwortleser` |
| Akku = `1F 11 08` | `T02Befehle.AbfrageAkku` |
| Nachlauf 7 mm | `T02Befehle.Nachlaufzeilen` |
| Warten bis Druckende | `SppVerbindung.AufDruckendeWarten` |
| Seriennummer ausblenden | `T02Meldung.ToString` |

Tests: 18 Unit-Tests, alle grün (u. a. echte Antwortfolge des T02 mit Platzhalter statt Seriennummer).
