# DocuDesk V1 Starter

Dieses Paket ist ein **reales Quellcode-Starterprojekt** für die lokale Windows-Dokumentenverwaltung **DocuDesk**.

## Enthalten
- WPF-Desktop-App (`DocuDesk.Desktop`)
- Application-/Domain-/Persistence-/Infrastructure-Layer
- SQLite-DDL mit FTS5- und Jobtabellen
- SQLite-Implementierung für Dokumente, Suche und Jobs
- lokales Repository für Originaldateien
- einfache Importfunktion über Dateidialog
- WebView2-basierter Dokumentviewer mit lokalen Viewer-Assets
- Tesseract-CLI-Anbindung für OCR
- Thumbnail-Erzeugung über ImageSharp
- erste Shell für Suche, Dokumentliste, Metadaten und Jobdiagnose
- Mail-Adapter-Grundlage für Thunderbird

## Wichtiger Status
- Der Code wurde **in dieser Umgebung nicht kompiliert**, weil hier kein .NET-SDK installiert war.
- Die Struktur und die Dateien sind jedoch so angelegt, dass sie in **Visual Studio 2022/2025** oder per `dotnet build` weiterverarbeitet werden können.

## Empfohlene lokale Voraussetzungen
- Windows 10/11
- .NET 10 SDK
- Visual Studio mit WPF-Workload
- WebView2 Runtime
- Optional: Thunderbird
- Optional: Tesseract OCR im PATH oder als Pfad in `settings.json`

## Start lokal
1. ZIP entpacken
2. `DocuDesk.sln` in Visual Studio öffnen
3. NuGet-Pakete wiederherstellen
4. Build starten
5. `DocuDesk.Desktop` als Startprojekt ausführen

## Was jetzt real angebunden ist
### OCR
- `TesseractCliOcrEngine` ruft `tesseract` per CLI auf
- erzeugt OCR-Artefakte im lokalen AppData-Bereich
- liest eingebetteten PDF-Text zuerst über PdfPig aus
- fällt bei Textdateien auf direkten Dateiinhalt zurück

### Viewer
- `ViewerHostControl` verwendet WebView2
- lokale Viewer-Assets werden über virtuelle Host-Zuordnung geladen
- PDFs und Bilder werden direkt angezeigt
- andere Dateitypen werden mit technischer Info angezeigt

### Thumbnailing
- Bilder werden als echte Thumbnails skaliert
- PDF- und sonstige Typen erhalten ein stabiles Platzhalter-Thumbnail

### Jobs
- Import legt `GenerateThumbnailJob` und `RunOcrJob` an
- `Jobs verarbeiten` führt ausstehende Jobs aus
- Jobstatus bleibt in SQLite erhalten

## Noch als nächstes zu bauen
- PDF.js statt Browser-PDF-Host für feinere Treffer-Overlays
- OCR-Blockpersistenz (`ocr_blocks`) aus TSV/hOCR
- Annotationen-Persistenz und UI
- echte Treffer-Navigation im Viewer
- Thunderbird Native Host und Add-on

## Einstellungen
Die App legt eine `settings.json` unter `%LOCALAPPDATA%\DocuDesk\` an. Relevante Felder:
- `TesseractExecutablePath`
- `TesseractDataPath`
- `OcrLanguage`
- `RepositoryRoot`
- `CacheRoot`
- `OcrArtifactsRoot`

## Repository- und Datenpfade (Standard)
- technische Daten unter `%LOCALAPPDATA%\DocuDesk\`
- Standard-Repository unter `%LOCALAPPDATA%\DocuDesk\Repository\`
- freier Wechsel des Repository-Pfads später vorgesehen


## Neu in v4

- OCR-TSV wird in `pages` und `ocr_blocks` persistiert
- WPF-ViewModel liefert OCR-Overlay-Daten an den Viewer
- WebView2-Viewer besitzt jetzt einen PDF.js-fähigen Renderpfad mit Fallback
- PDF.js-Dateien können lokal unter `src/DocuDesk.Viewer/viewer-assets/pdfjs/` hinterlegt werden
