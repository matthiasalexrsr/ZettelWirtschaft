# Arbeitspaket 08 – Dokumentdetailseite und Metadaten-Editor

## Ziel
Der rechte Bereich der Oberfläche soll sich von einer reinen Textausgabe zu einem professionellen Dokument-Inspector mit klar gegliederten Informationen und einem gut vorbereiteten Metadaten-Editor entwickeln.

## Arbeitsbasis
- `src/DocuDesk.Desktop/MainWindow.xaml`
- `src/DocuDesk.Desktop/MainWindow.xaml.cs`
- `src/DocuDesk.Desktop/ViewModels/MainViewModel.cs`
- `src/DocuDesk.Desktop/Styles/DocuDesk.DesignTokens.xaml`
- `src/DocuDesk.Desktop/Styles/DocuDesk.ModernTheme.xaml`

## Fokus
### 1. Detailbereich besser strukturieren
Der rechte Bereich soll logisch gegliedert sein, z. B. in:
- Dokumentübersicht
- Metadaten
- technische Informationen
- Pfad / Ablage / Status

### 2. Bessere Lesbarkeit
- Labels und Werte klar unterscheiden
- lange Pfade und längere Texte sauber umbrechen
- gleiche Abstände zwischen Gruppen
- visuell ruhige Gruppenbildung mit Cards oder Untersektionen

### 3. Vorbereitung für Editierbarkeit
Auch wenn noch nicht alle Felder sofort editierbar gemacht werden:
- Layout so gestalten, dass spätere Editierfelder natürlich hineinpassen
- Metadatenbereich nicht wie reine Diagnoseausgabe wirken lassen
- klare „Ansicht vs. Bearbeitung“-Logik mitdenken

### 4. Sinnvolle Informationshierarchie
Besonders wichtig und oben sichtbar:
- Titel
- Dokumenttyp
- Absender
- Importdatum / OCR-Status / Statushinweis

Weniger wichtig und weiter unten:
- Repository-Pfad
- technische Zusatzinfos

### 5. Kleine UX-Verbesserungen
- hilfreiche Platzhalter oder leere Zustände für fehlende Werte
- Status/Badges sinnvoll einsetzen
- visuell saubere Gruppierung statt langer Textspalten

## Grenzen
- keine große Architekturänderung
- bestehende Bindings nicht brechen
- keine neuen externen UI-Frameworks
- nur kleine, sichere Schritte

## Abnahme
Am Ende dokumentieren:
- geänderte Dateien
- welche Bereiche der Detailansicht verbessert wurden
- welche Informationen anders gruppiert oder priorisiert wurden
- ob alle bestehenden Bindings erhalten blieben
- ob der Bereich jetzt klarer wie ein professioneller Inspector wirkt
- nächster sinnvoller Schritt
