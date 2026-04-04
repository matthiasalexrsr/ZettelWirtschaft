# ZettelWirtschaft / DocuDesk

Lokale Windows-Dokumentenverwaltung mit OCR, Volltextsuche und WebView2-Viewer.

## Projektstruktur

```
src/
  DocuDesk.sln                          -- Unified Solution
  Directory.Build.props                 -- Gemeinsame Build-Einstellungen
  DocuDesk.Domain/                      -- Entities, Enums, Value Objects
  DocuDesk.Application/                 -- Interfaces, Services, Abstractions
  DocuDesk.Contracts/                   -- DTOs (Import, Search, Viewer, Mail)
  DocuDesk.Persistence/                 -- SQLite-Repos, Migrations (FTS5)
  DocuDesk.Infrastructure/              -- OCR (Tesseract), Storage, Thumbnails, Backup, Settings
  DocuDesk.Viewer/                      -- WebView2-basierter PDF-/Bild-Viewer
  DocuDesk.Worker/                      -- Job-Verarbeitung (OCR, Thumbnails)
  DocuDesk.Integrations.Mail/           -- Thunderbird Mail-Adapter
  DocuDesk.Integrations.Thunderbird.Host/ -- Native Messaging Host
  DocuDesk.Desktop/                     -- WPF-App (Startprojekt)
database/
  ddl_v1.sql                            -- Referenz-DDL (auch als Migration in Persistence)
docs/
  next_steps.md                         -- Geplante Erweiterungen
  pdfjs_setup.md                        -- PDF.js-Einrichtung
  viewer_bridge_spec.md                 -- Viewer-Bridge-Spezifikation
```

## Voraussetzungen

- Windows 10/11
- .NET 10 SDK
- Visual Studio 2022/2025 mit WPF-Workload
- WebView2 Runtime
- Optional: Tesseract OCR im PATH
- Optional: Thunderbird

## Schnellstart

1. `src/DocuDesk.sln` in Visual Studio offnen
2. NuGet-Pakete wiederherstellen
3. `DocuDesk.Desktop` als Startprojekt setzen
4. Build und Run

Beim ersten Start werden die SQLite-Migrationen automatisch ausgefuhrt.

## Enthaltene Features

### Import
- Datei-Import uber Dateidialog (PDF, PNG, JPG, TIFF, TXT)
- SHA256-Duplikaterkennung
- Automatische Job-Erzeugung (Thumbnail + OCR)

### OCR
- Tesseract-CLI-Anbindung mit TSV-Block-Persistenz
- PDF-Rasterisierung uber pdftoppm (Poppler)
- Embedded-Text-Extraktion uber PdfPig als Fallback
- OCR-Blöcke, Seiten und Konfidenz in SQLite gespeichert

### Viewer
- WebView2-basierter Dokumentviewer
- PDF-Vorschau (mit PDF.js-Unterstutzung)
- Bildvorschau fur PNG/JPG/BMP/TIFF
- OCR-Overlay mit anklickbaren, kopierbaren Blöcken
- Annotationen (Highlight, Rechteck, Notiz)
- Suchbegriff-Hervorhebung

### Suche
- SQLite FTS5 Volltextsuche
- BM25-Ranking mit gewichteten Feldern
- Snippet-Anzeige in Suchergebnissen

### Jobs
- Hintergrund-Jobverarbeitung (Thumbnail, OCR)
- Job-Status-Tracking in SQLite
- Job-Diagnose in der UI

### Weiteres
- Metadaten-Bearbeitung (Titel, Datum, Sender, Empfanger, Kategorie, Tags)
- Backup-Funktion (SQLite-Kopie)
- Mail-Entwurf-Funktion (Thunderbird-Integration vorbereitet)
- Einstellungen in `%LOCALAPPDATA%\DocuDesk\settings.json`

## Architektur

Dieses Projekt vereint zwei Entwicklungsstrange:

- **DocuDeskV1 (WPF)**: Vollstandige Pipeline mit Viewer, Jobs, Thumbnails, Backup, Mail
- **DocuDeskStarter (WinUI)**: Bessere Architekturmuster (Handler-Pattern, Migration-System, DI-Extensions)

Das Ergebnis nutzt WPF als bewährte UI-Basis und ubernimmt die architektonischen Verbesserungen des Starters:

- **Migrations-System**: `EmbeddedSqliteMigrator` statt manueller DDL-Ausfuhrung
- **DI-Extensions**: `AddDocuDeskInfrastructure()` / `AddDocuDeskPersistence()` fur saubere Service-Registrierung
- **Service-Abstraktionen**: `IClock`, `IHashService`, `IIdGenerator`, `IDocumentStorageService`, `IDocumentImportInspector`
- **Interface-basierte Persistence**: `ISqliteConnectionFactory` fur Testbarkeit

## Datenpfade (Standard)

- App-Daten: `%LOCALAPPDATA%\DocuDesk\`
- Dokument-Repository: `%LOCALAPPDATA%\DocuDesk\Repository\`
- Datenbank: `%LOCALAPPDATA%\DocuDesk\db\docudesk.db`
- Backups: `%LOCALAPPDATA%\DocuDesk\backups\`
