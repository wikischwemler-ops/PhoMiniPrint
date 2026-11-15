# Prozessabläufe – PhoMiniPrint

- **Projektstatus:** TECHNISCHE PLANUNG
- **Bezug:** [FUNCTIONAL_DESIGN.md](FUNCTIONAL_DESIGN.md) (F-01 … F-12), [REQUIREMENTS.md](REQUIREMENTS.md)

Die Diagramme sind in Mermaid geschrieben und werden auf GitHub direkt angezeigt.

---

## 1. Gesamtablauf

Vom Start bis zum fertigen Ausdruck.

```mermaid
flowchart TD
    A([Programmstart]) --> B[Einstellungen laden<br/>F-10]
    B --> C{Zuletzt verwendeter<br/>T02 bekannt?}
    C -- ja --> D[Automatisch verbinden<br/>F-01]
    C -- nein --> E[Drucker auswählen<br/>F-01]
    E --> D
    D --> F[Status anzeigen<br/>Akku · Papier · Signal<br/>F-02]
    F --> G[Etikett einrichten<br/>Länge, Ränder · F-03]
    G --> H{Was einfügen?}
    H -- Bild --> I[Bild einfügen<br/>automatisch maximal einpassen<br/>F-04]
    H -- Text --> J[Text mit Symbolen<br/>F-06]
    I --> K[Mit der Maus anpassen,<br/>drehen · F-05]
    J --> K
    K --> L[Regler Schwarz / Dichte<br/>F-07]
    L --> M{Weiteres Element?}
    M -- ja --> H
    M -- nein --> N[Drucken<br/>F-08]
    N --> O([Ausdruck in exakter Größe])
```

## 2. Drucker verbinden (F-01)

```mermaid
flowchart TD
    A([Verbinden]) --> B[Gekoppelte Bluetooth-Geräte lesen]
    B --> C{T02 / T02E / C02E<br/>gefunden?}
    C -- nein --> C1[Meldung: Bitte T02 in<br/>Windows koppeln] --> Z([Ende])
    C -- mehrere --> C2[Auswahlliste] --> D
    C -- einer --> D[Zugehörigen Bluetooth-COM-Port<br/>ermitteln]
    D --> E[Port öffnen<br/>Status: verbindet …]
    E --> F{Port offen?}
    F -- nein --> F1[Status: Fehler<br/>Erneut versuchen] --> Z
    F -- ja --> G[Statusabfrage senden]
    G --> H{Antwort in 5 s?}
    H -- nein --> F1
    H -- ja --> I[Status: bereit<br/>Drucker merken]
    I --> Z2([verbunden])
```

## 3. Statusüberwachung (F-02)

```mermaid
sequenceDiagram
    participant UI as Statusleiste
    participant App as PhoMiniPrint
    participant BT as Windows-Bluetooth
    participant T02 as T02

    loop alle 10 s, außer während eines Drucks
        App->>T02: Abfrage Akku
        T02-->>App: Akkustand
        App->>T02: Abfrage Papier / Deckel
        T02-->>App: Papier ok / kein Papier / Deckel offen
        App->>BT: Signalstärke abfragen
        BT-->>App: RSSI (falls verfügbar)
        App->>UI: Akku % + Balken, Papier, Signal % + Balken
    end
    T02--)App: unaufgeforderte Meldung (z. B. kein Papier)
    App->>UI: sofort anzeigen
```

## 4. Bild einfügen und anpassen (F-04, F-05)

```mermaid
flowchart TD
    A([Bild einfügen]) --> B[Datei wählen]
    B --> C{PNG oder JPG?}
    C -- nein --> C1[Meldung: Format nicht unterstützt] --> Z([Ende])
    C -- ja --> D[Bild laden,<br/>Seitenverhältnis bestimmen]
    D --> E[Druckbereich =<br/>Etikett minus Ränder]
    E --> F[Bild maximal einpassen,<br/>Seitenverhältnis fest, zentriert]
    F --> G[Bandansicht: Bandrahmen,<br/>Ränder, Bild mit Anfassern]
    G --> H{Benutzeraktion}
    H -- Ecke ziehen --> I[Größe ändern,<br/>Verhältnis bleibt]
    H -- Mitte ziehen --> J[Position ändern]
    H -- Drehen 0/90/180/270° --> K[Drehen, auf vorgegebene<br/>Größe skalieren]
    I --> L[Auf 1/8 mm runden]
    J --> L
    K --> L
    L --> M{Innerhalb der Ränder?}
    M -- nein --> M1[Rot markieren,<br/>Drucken gesperrt] --> H
    M -- ja --> G
```

## 5. Drucken (F-08)

```mermaid
flowchart TD
    A([Drucken]) --> B{Alle Elemente im<br/>Druckbereich?}
    B -- nein --> B1[Meldung: Element ragt über den Rand] --> Z([Ende])
    B -- ja --> C[Status abfragen]
    C --> D{Papier ok,<br/>Deckel zu?}
    D -- nein --> D1[Meldung: kein Papier / Deckel offen] --> Z
    D -- ja --> E[Etikett rendern:<br/>384 Punkte quer ×<br/>Länge × 8 × Faktor Zeilen]
    E --> F[Für Laufrichtung drehen]
    F --> G[Schwarz-Weiß mit<br/>Schwelle aus Regler]
    G --> H[Druckauftrag bauen:<br/>Init · Dichte · Raster · Vorschub]
    H --> I[In Paketen senden,<br/>Fortschritt anzeigen]
    I --> J{Fehlermeldung<br/>vom Drucker?}
    J -- ja --> J1[Abbruch, Meldung,<br/>kein Neudruck] --> Z
    J -- nein --> K([Gedruckt])
```

## 6. Kalibrieren (F-09)

```mermaid
flowchart TD
    A([Menü Kalibrierung]) --> B[Testseite drucken:<br/>Rahmen 80 × 40 mm mit Maßen]
    B --> C[Benutzer misst<br/>Länge und Höhe]
    C --> D[Messwerte eingeben]
    D --> E{Abweichung<br/>≤ 10 %?}
    E -- nein --> E1[Hinweis: bitte nachmessen] --> C
    E -- ja --> F[Faktoren berechnen:<br/>Soll ÷ Ist]
    F --> G[Faktoren speichern]
    G --> H{Kontrolldruck?}
    H -- ja --> B
    H -- nein --> Z([Fertig])
```

## 7. Datenfluss beim Drucken

Wie aus dem Etikett in Millimetern Druckerpunkte werden.

```mermaid
flowchart LR
    A[Etikett in mm<br/>Elemente, Ränder] --> B[mm → Punkte<br/>× 8 × Kalibrierfaktor]
    B --> C[Rendern<br/>Bild + Text]
    C --> D[Drehen auf<br/>Laufrichtung]
    D --> E[Schwelle →<br/>1 Bit pro Punkt]
    E --> F[T02-Befehle<br/>Raster-Blöcke]
    F --> G[Bluetooth SPP<br/>Pakete à 128 Byte]
    G --> H[T02]
```
