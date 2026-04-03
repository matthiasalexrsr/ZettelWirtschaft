# DocuDesk Starter

Dies ist ein realer Starter für die geplante Windows-Standalone-Anwendung **DocuDesk** zur Verwaltung gescannter Briefe.

Enthalten sind:

- WinUI-3-App-Skelett auf **.NET 10**
- getrennte Projekte für **Domain**, **Application**, **Persistence**, **Infrastructure**
- SQLite-Migrationen mit **FTS5**
- erste Repository-Implementierungen mit **Dapper**
- Grundgerüst für **Thunderbird Native Messaging Host**
- einfache WinUI-Shell mit Seiten für **Inbox**, **Suche** und **Dokumentdetail**

## Wichtiger Hinweis

Die Dateien wurden in einer Linux-Containerumgebung erzeugt. Ich konnte den Windows-/WinUI-Build hier **nicht lokal kompilieren oder starten**, weil weder Windows noch das .NET-/Visual-Studio-Werkzeugset verfügbar sind. Das Projekt ist deshalb als **sauberes, plausibles Starter-Repository** angelegt, aber nicht build-verifiziert.

## Zielplattform

- Windows 10 ab 1809 oder Windows 11
- Visual Studio mit WinUI-/Windows-App-SDK-Workload
- .NET 10 SDK
- Windows App SDK 1.8.x

Das Starterprojekt ist bewusst **unpackaged** angelegt, um die frühe Entwicklung einfacher zu halten.

## Enthaltene Kernbausteine

- `src/DocuDesk.App` – WinUI-3-Oberfläche
- `src/DocuDesk.Domain` – Entitäten, Enums, Value Objects
- `src/DocuDesk.Application` – Use Cases, Abstraktionen, Read Models
- `src/DocuDesk.Persistence` – SQLite, Dapper, Migrationen, Repositories
- `src/DocuDesk.Infrastructure` – Hashing, Zeit, IDs, Mail-Adapter
- `src/DocuDesk.ThunderbirdHost` – Native-Messaging-Host-Skelett
- `docs/thunderbird` – Sample-Manifest und Add-on-Startpunkt

## Erste Schritte auf Windows

1. Visual Studio mit WinUI-/Windows-App-SDK-Workload installieren.
2. Ordner in Visual Studio öffnen oder `DocuDesk.sln` laden.
3. NuGet-Restore ausführen.
4. `DocuDesk.App` als Startprojekt setzen.
5. Unpackaged-Startprofil auswählen.
6. Beim ersten Start werden die SQLite-Migrationen in `%ProgramData%\DocuDesk\db\app.db` ausgeführt.

## Aktueller Stand der Anwendung

Die aktuelle Lieferung enthält bereits erste nutzbare V1-Pfade:

- Import einzelner PDF-/Bilddateien über die Inbox-Oberfläche
- lokale Dateiablage im Repository unter `%ProgramData%\DocuDesk\files`
- Datenbankeintrag in SQLite inklusive Seitenhülle und Inbox-Kategorie
- einfache Dokumentliste in der Inbox mit Status, Importdatum und Seitenzahl
- Dokumentdetailansicht mit Metadaten, Seitenliste, Repository-Pfad und OCR-Textvorschau
- einfache FTS-basierte Suchoberfläche mit Trefferliste und Öffnen eines Dokuments aus den Suchergebnissen

## Bekannte Grenzen dieser Ausbaustufe

- OCR ist strukturell vorbereitet, aber noch nicht an den UI-Importpfad angeschlossen.
- Die Seitenermittlung bei PDFs ist bewusst nur heuristisch und wird in der nächsten Stufe durch einen robusteren PDF-Adapter ersetzt.
- Die Dokumentdetailansicht ist noch kein echter PDF-/Bild-Viewer.
- Annotationen und Thunderbird-Aktionen sind weiterhin nur strukturell vorbereitet.

## Nächste sinnvolle Schritte

- OCR-Service mit Tesseract anbinden
- echte Viewer-Komponente für PDF/Bild integrieren
- Annotationen-Editor ergänzen
- Thunderbird-Add-on tatsächlich implementieren
- automatisierte Tests und CI ergänzen


## Aktueller Stand (v4)

- Inbox mit Dateiimport in lokales Repository
- Suche über SQLite FTS5
- Dokumentdetailseite
- Eingebetteter Viewer:
  - PDF-Vorschau über WebView2
  - Bildvorschau für PNG/JPG/BMP/TIFF
  - Seitenauswahl sowie Vor/Zurück-Navigation
  - Fallback: Originaldatei im Standardprogramm öffnen


## Stand v5

- Metadaten können in der Detailansicht bearbeitet und gespeichert werden.
- Kategorien lassen sich aus den Seed-Daten auswählen.
- Tags können direkt am Dokument gesetzt oder entfernt werden.
- Das Dokumentdatum akzeptiert z. B. `31.12.2025` oder `2025-12-31`.

## Stand v6

Diese Ausbaustufe ergänzt einen ersten lokalen OCR-Pfad:

- OCR direkt aus der Detailansicht per Schaltfläche **„OCR ausführen“**
- Anbindung an **Tesseract** als externen Prozess
- PDF-OCR optional über **pdftoppm** (Poppler) zur Rasterisierung
- Speicherung von OCR-Text und OCR-Blöcken in SQLite
- Aktualisierung der FTS-Suche über die bestehenden Trigger
- Persistenz der OCR-Artefaktpfade im Repository

### Hinweise

- Für Bilddateien (`png`, `jpg`, `bmp`, `tif`, `tiff`) kann OCR direkt ausgeführt werden.
- Für PDFs wird zusätzlich `pdftoppm` benötigt.
- Standardmäßig erwartet die App `tesseract` und `pdftoppm` im `PATH`.
- Die Konfiguration liegt in `src/DocuDesk.App/appsettings.json`.

