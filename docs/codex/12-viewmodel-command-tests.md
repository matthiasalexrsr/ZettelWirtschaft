# Arbeitspaket 12 – MainViewModel Command-Tests

## Ziel
Die Interaktionslogik des `MainViewModel` soll nicht nur indirekt über Strukturtests abgesichert sein, sondern direkt über echte Command-Tests.

## Ziel-Commands
- `SearchCommand`
- `ImportCommand`
- `BackupCommand`
- `DraftMailCommand`

## Problemstellen im aktuellen Stand
Für belastbare Unit-Tests müssen zwei statische UI-Abhängigkeiten testbar gemacht werden:
- `OpenFileDialog`
- `MessageBox`

## Kleine, sichere Produktivcode-Seams
1. `IImportFileDialogService`
   - liefert ausgewählte Import-Dateien
2. `IUserNotificationService`
   - kapselt Information/Warnung statt direkter `MessageBox.Show`-Aufrufe
3. `RelayCommand`
   - für asynchrone Commands eine `ExecutionTask` oder vergleichbare Await-Möglichkeit bereitstellen
4. optional: Auto-Initialisierung des `MainViewModel` für Tests deaktivierbar machen

## Testprojekt
Lege ein eigenes Testprojekt an, z. B.:
- `tests/DocuDesk.Desktop.Tests/`

## Mindestabdeckung
### SearchCommand
- ruft Suche mit aktuellem `SearchText` auf
- füllt `Documents`
- wählt erstes Dokument aus

### ImportCommand
- importiert alle vom Dialog gelieferten Dateien
- verarbeitet Jobs danach erneut
- lädt Dokumente/Jobs erneut

### BackupCommand
- ruft Backup-Service auf
- zeigt danach eine Information an

### DraftMailCommand
- ist ohne `SelectedDocument` nicht ausführbar
- erzeugt bei ausgewähltem Dokument den erwarteten MailDraftRequest
- zeigt bei Fehlerfall eine Warnung an

## Technische Leitplanken
- nur kleine, sichere Änderungen
- keine Architekturänderung
- keine neue UI-Bibliothek
- vorhandene Commands und Bindings dürfen nicht beschädigt werden

## Abnahme
Dokumentieren:
- geänderte Dateien
- welche Test-Seams ergänzt wurden
- welche Command-Tests existieren
- welche Command-Wege jetzt testbar sind
- welche Restlücken ggf. noch offen sind
