# Arbeitspaket 11 – Testing-Strategie

## Ziel
Für DocuDesk sollen belastbare Test-Mechanismen aufgebaut werden, damit Refactorings, UI-Modernisierung und Startup-Fixes nicht unbemerkt neue Fehler erzeugen.

## Testpyramide
### 1. Unit-Tests
Fokus auf:
- Domain-Objekte
- Enum-/Status-Konventionen
- kleine Services ohne externe Abhängigkeiten
- Hilfsmethoden und Konvertierungslogik

### 2. Integrations-Tests
Fokus auf:
- SQLite/Persistenz
- Migrationen
- Repository-Verhalten
- Settings- und Dateipfade

### 3. Smoke-/Startup-Validierung
Fokus auf:
- Restore / Build / Test
- App-Startup unter Windows
- DI-Auflösung
- MainWindow-Erstellung
- Migration beim Start

## Erste Ausbaustufe in diesem Branch
- dediziertes Testprojekt auf xUnit-Basis
- erste Domain-Tests
- CI-Workflow für Restore/Build/Test auf Windows mit .NET 9
- Test-Runsettings für Coverage-Vorbereitung

## Weitere sinnvolle Ausbaustufen
- Integrations-Tests für SQLite-Migrationen
- Tests für `JsonSettingsStore`
- Tests für `MainViewModel` mit gemockten Interfaces
- Smoke-Test für DI-Container und Startup-Pfad
- Snapshot-/golden-master-artige Viewer-/Overlay-Tests, sofern später praktikabel

## Abnahme
Dokumentieren:
- welche Testprojekte angelegt wurden
- welche Bereiche jetzt testbar sind
- welche Bereiche noch ungetestet sind
- wie Tests lokal und in CI gestartet werden
