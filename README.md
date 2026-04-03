# ZettelWirtschaft / DocuDesk Recovery Workspace

Dieses Repository enthält mehrere ZIP-Archive mit Quellcode-Teilen von **DocuDesk**.

## Ziel
Aus den gelieferten Code-Snippets und Projektteilen einen lauffähigen Stand herstellen.

## Enthaltene Archive
- `DocuDeskStarter_v6.zip` (WinUI-basierter Starter)
- `DocuDesk_V1_Starter_v6.zip` (WPF-basierter V1-Stand)
- `DocuDesk_V1_Starter_v6_fresh.zip` (frische Vergleichskopie)

## Schnellstart
1. Archive entpacken und Struktur prüfen:
   ```bash
   ./bootstrap_docudesk.sh
   ```
2. Auf Windows den WPF-Stand öffnen:
   - `unpacked/DocuDeskV1/DocuDeskV1_v6/DocuDesk.sln`
3. In Visual Studio:
   - NuGet Restore
   - `DocuDesk.Desktop` als Startprojekt
   - Build/Run

## Warum `DocuDeskV1` als erster funktionsfähiger Zielstand?
Der WPF-Stand enthält bereits einen durchgehenden Pfad für:
- Import
- SQLite-Persistenz
- OCR-Integration (Tesseract CLI)
- Viewer + Job-Verarbeitung

Damit ist er als „funktionierender“ Startpunkt besser geeignet als der neuere, aber plattformabhängigere WinUI-Stand.
