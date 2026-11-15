<p align="center">
  <img src="docs/bilder/drucker.svg" alt="PhoMiniPrint – Mini-Thermodrucker mit Papierstreifen, 40 mm Maßangabe und Bluetooth-Verbindung zum PC" width="720">
</p>

<h1 align="center">
  <img src="docs/bilder/icon.svg" alt="" width="40" align="center"> PhoMiniPrint
</h1>

<p align="center">
  Deutsches Windows-Programm zum Drucken in <b>exakter Größe</b> auf dem Mini-Thermodrucker <b>Phomemo T02</b> über Bluetooth.
</p>

---

## Ziel

- Bilder **millimetergenau** drucken (±1 mm), Seitenverhältnis bleibt erhalten
- **Bandansicht**: Bild wird ins Band eingepasst und mit der Maus angepasst
- Drehen um 0°, 90°, 180° und 270°
- Ränder links, rechts, oben und unten einzeln einstellbar
- **Kalibrierung** mit Testseite, Werte werden gespeichert
- **Text mit Symbolen** drucken
- Verbindung über **Bluetooth**, Oberfläche komplett auf **Deutsch**
- Windows 11, Auslieferung als **Installer**, C# mit .NET (WPF)

## Stand

📋 **Anforderungsphase** – noch kein lauffähiges Programm.

| Dokument | Inhalt |
|---|---|
| [Eingabe](docs/REQUIREMENTS_INPUT.md) | Idee und Mindestanforderungen |
| [Analyse](docs/ANALYSIS_SUMMARY.md) | Funktionsbereiche, Abläufe, Risiken |
| [Offene Fragen](docs/OPEN_QUESTIONS.md) | Rückfragen und Antworten |
| [Anforderungsentwurf](docs/REQUIREMENTS_DRAFT.md) | Anforderungen mit Status |

## Hintergrund

[TiMini-Print](https://github.com/Dejniel/TiMini-Print) druckt bereits auf dem T02, kann aber nicht in exakter Größe drucken. Es dient als Referenz für das Bluetooth-Druckprotokoll (Apache-2.0).

## Mitarbeit

Branches nur zweistufig `xxx/yyy` (z. B. `feat/bandansicht`), Änderungen per Pull Request nach `main`. Hooks einmal je Klon aktivieren: `tools\git_hooks_einrichten.bat`.

---

<sub>Phomemo ist eine Marke des jeweiligen Inhabers. PhoMiniPrint ist ein unabhängiges Projekt.</sub>
