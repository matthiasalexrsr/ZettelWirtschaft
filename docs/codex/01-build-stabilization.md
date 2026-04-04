# Arbeitspaket 01 – Build stabilisieren

## Arbeitsbasis
Arbeite ausschließlich in:

`unpacked/DocuDeskV1/DocuDeskV1_v6/`

## Ziel
Den vorhandenen WPF-Stand als primären Entwicklungsstand stabilisieren und kompilierbar machen.

## Zu prüfen
- `DocuDesk.sln`
- `src/DocuDesk.Desktop`
- `src/DocuDesk.Persistence`
- `src/DocuDesk.Infrastructure`
- `src/DocuDesk.Viewer`
- `src/DocuDesk.Worker`

## Aufgaben
1. Prüfe die Solution auf:
   - fehlende Dateien
   - fehlerhafte Projektverweise
   - inkonsistente Namespaces
   - offensichtliche Build-Blocker
   - Startpfad-Probleme in `App.xaml.cs`
2. Nimm nur kleine, sichere Änderungen vor.
3. Verändere noch keine große Architektur.
4. Wenn mehrere Alternativen möglich sind, bevorzuge die bestehende WPF-Struktur.

## Abnahme
Am Ende klar dokumentieren:
- welche Fehler gefunden wurden
- welche Dateien geändert wurden
- was jetzt funktionieren sollte
- was noch offen ist
- was als Nächstes empfohlen wird
