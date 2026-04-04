# Arbeitspaket 06 – UI-Abnahme und Screenshot-Checkliste

## Ziel
Nach der Modernisierung von `MainWindow.xaml` soll die Oberfläche nicht nur technisch funktionieren, sondern auch visuell konsistent, modern und professionell wirken.

## Arbeitsbasis
- `src/DocuDesk.Desktop/MainWindow.xaml`
- `src/DocuDesk.Desktop/MainWindow.xaml.cs`
- `src/DocuDesk.Desktop/Styles/DocuDesk.DesignTokens.xaml`
- `src/DocuDesk.Desktop/Styles/DocuDesk.ModernTheme.xaml`

## Visuelle Abnahme
### 1. Gesamteindruck
- Wirkt die Oberfläche wie eine echte Desktop-App und nicht wie ein Standard-WPF-Formular?
- Ist die Hierarchie auf den ersten Blick klar?
- Gibt es ausreichend Weißraum?
- Wirkt die Oberfläche ruhig und professionell?

### 2. Linke Navigation
- Klarer App-/Produktkopf sichtbar
- Navigation gut lesbar
- Hover-/Selektionszustände wirken konsistent
- Navigation optisch ruhiger als der Arbeitsbereich

### 3. Such- und Aktionenleiste
- Titel und Beschreibung wirken wie ein sauberer Workspace-Header
- Suchfeld ist gut sichtbar
- Primäraktion klar erkennbar
- Sekundäraktionen wirken konsistent und nicht überbetont

### 4. Dokumentliste
- Einträge haben klare visuelle Gruppierung
- Selektionszustand ist deutlich sichtbar
- OCR-Status oder Badges sind lesbar und nicht zu dominant
- Listenbereich wirkt ordentlich und nicht gedrängt

### 5. Viewer-Bereich
- Zentraler Arbeitsbereich klar als Hauptfokus erkennbar
- Überschrift und ggf. Badge sind sauber ausgerichtet
- Viewer hat genügend Platz
- Keine visuellen Kollisionen mit Rand/Padding

### 6. Detailbereich
- Informationen logisch gegliedert
- Labels und Werte gut unterscheidbar
- Lange Pfade umbrechen sauber
- Bereich wirkt nicht wie rohe Formularausgabe

### 7. Jobs & Diagnose
- Liste lesbar auch bei längeren Fehlertexten
- Statusspalte hebt sich sinnvoll ab
- Einträge wirken als zusammengehörige Karten/Listenelemente

## Funktionale Abnahme
- Alle bisherigen Bindings bleiben intakt
- Alle bisherigen Commands bleiben intakt
- `DocumentViewer` mit `x:Name="DocumentViewer"` bleibt vorhanden
- MainWindow lädt ohne XAML-Fehler
- Auswahl eines Dokuments aktualisiert Viewer und Detailbereich weiter korrekt
- Suche, Import, Backup, Mail-Entwurf und Jobs verarbeiten bleiben weiterhin klickbar

## Screenshot-Checkliste
Bitte nach der Umsetzung Screenshots oder Beschreibungen zu diesen Zuständen liefern:
1. Gesamtes MainWindow direkt nach App-Start
2. MainWindow mit ausgewähltem Dokument in der Liste
3. MainWindow mit sichtbarem Viewer und Detailbereich
4. MainWindow mit gefüllter Jobs-/Diagnose-Liste
5. Optional: Hover- oder Selektionszustand eines Dokumentlisteneintrags

## Ergebnisdokumentation
Am Ende dokumentieren:
- geänderte Dateien
- welche UI-Bereiche modernisiert wurden
- ob alle Bindings/Commands erhalten blieben
- kurze Beschreibung des finalen Layouts
- offene UI-Kanten oder bekannte Einschränkungen
- nächster sinnvoller UI-Schritt
