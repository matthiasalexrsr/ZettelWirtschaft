# Arbeitspaket 09 – Dokumentliste und Suchergebnis-UX

## Ziel
Die linke Seite der Anwendung soll sich von einer einfachen Ergebnisliste zu einer klar lesbaren, effizienten Arbeitsliste für Dokumente und Suchtreffer entwickeln.

## Arbeitsbasis
- `src/DocuDesk.Desktop/MainWindow.xaml`
- `src/DocuDesk.Desktop/MainWindow.xaml.cs`
- `src/DocuDesk.Desktop/ViewModels/MainViewModel.cs`
- `src/DocuDesk.Desktop/Styles/DocuDesk.DesignTokens.xaml`
- `src/DocuDesk.Desktop/Styles/DocuDesk.ModernTheme.xaml`

## Fokus
### 1. Bessere Informationshierarchie pro Listeneintrag
Ein Dokumenteintrag soll auf einen Blick verständlich machen:
- Titel
- Dateiname oder Anzeige-Name
- Importdatum oder relevante Zeitangabe
- OCR-Status
- optional Snippet / Trefferkontext

Weniger wichtige Informationen sollen visuell zurücktreten.

### 2. Suchtreffer besser lesbar machen
- Treffer-Snippets sollen klar vom übrigen Text getrennt sein
- Suchkontext soll schnell erfassbar sein
- lange Treffertexte sollen ruhig und sauber umbrechen
- Suchergebnisse sollen nicht wie rohe Debug-Listen wirken

### 3. Selektions- und Hover-Zustände verbessern
- klarer aktiver Eintrag
- professioneller Hover-Zustand
- visuell konsistenter Fokus auf das aktuell ausgewählte Dokument

### 4. Leere und randständige Zustände
Berücksichtigen:
- keine Dokumente vorhanden
- keine Suchtreffer
- nur ein einzelner Treffer
- sehr lange Titel oder Dateinamen

### 5. Vorbereitung für spätere Listenfunktionen
Ohne große Architekturänderung soll das Layout so gestaltet sein, dass später möglich bleiben:
- Status-Badges
- Filterhinweise
- Sortierinformationen
- Mehrfachauswahl oder Schnellaktionen

## Grenzen
- keine Änderung an Kernarchitektur oder ViewModel-Verträgen
- keine neuen externen UI-Frameworks
- bestehende Bindings und Commands müssen erhalten bleiben
- nur kleine, sichere UI-Schritte

## Abnahme
Am Ende dokumentieren:
- geänderte Dateien
- welche Verbesserungen an Dokumentliste und Suchergebnissen umgesetzt wurden
- wie sich Selektions- und Hover-Zustände verändert haben
- welche leeren Zustände verbessert wurden
- ob alle Bindings/Commands erhalten blieben
- warum die Liste jetzt professioneller und arbeitsfähiger wirkt
- nächster sinnvoller UI-Schritt
