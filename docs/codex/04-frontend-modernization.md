# Arbeitspaket 04 – Frontend / GUI modernisieren

## Zielbild
Die WPF-Oberfläche soll modern, übersichtlich und professionell wirken.

Nicht Ziel dieses Pakets ist eine große Architekturänderung. Es geht um einen klareren App-Shell-Look, bessere visuelle Hierarchie und konsistente Controls.

## Arbeitsbasis
- `src/DocuDesk.Desktop/MainWindow.xaml`
- `src/DocuDesk.Desktop/MainWindow.xaml.cs`
- `src/DocuDesk.Desktop/Styles/DocuDesk.DesignTokens.xaml`
- `src/DocuDesk.Desktop/Styles/DocuDesk.ModernTheme.xaml`

## Gestaltungsprinzipien
1. klare visuelle Hierarchie
2. ruhige, helle Flächen mit sauberem Kontrast
3. konsistente Abstände und Radien
4. eine starke Primäraktion, zurückhaltende Sekundäraktionen
5. weniger „Formularoptik“, mehr App-Shell / Workspace-Charakter
6. Dokumentliste, Viewer und Detailseite müssen als zusammengehöriger Arbeitsbereich lesbar sein

## Erste konkrete Umsetzung
1. ResourceDictionaries in `App.xaml` oder `MainWindow.xaml` einbinden.
2. `MainWindow.xaml` auf die neuen Styles umstellen.
3. Folgende Bereiche visuell überarbeiten:
   - linke Navigation
   - Kopfbereich mit Suche und Primäraktionen
   - Dokumentliste mit klarer Selektion
   - Viewer-Container
   - Detailpanel
   - Job-/Diagnosebereich
4. Keine Bindings oder Commands kaputtmachen.

## Kleine, sichere UI-Verbesserungen
- bessere Schriftgrößen-Hierarchie
- modernere Buttons
- Cards statt harter Standardflächen
- sauberer Selektionszustand in Listen
- Navigation optisch leichter und professioneller
- mehr Weißraum, weniger visuelle Dichte

## Abnahme
Am Ende dokumentieren:
- geänderte Dateien
- welche UI-Elemente modernisiert wurden
- ob alle bisherigen Bindings/Commands erhalten blieben
- Screenshot oder kurze Beschreibung des neuen Layouts
- nächster sinnvoller UI-Schritt
