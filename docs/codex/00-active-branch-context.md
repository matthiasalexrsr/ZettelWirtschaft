# Aktiver Branch-Kontext

## Wichtig
Auf dem Branch `claude/fix-app-startup-C5MzU` wurde der aktive WPF-Stand bereits aus `unpacked/...` an die Repo-Wurzel verlagert.

## Tatsächliche Arbeitsbasis auf diesem Branch
Arbeite auf diesem Branch **nicht mehr** in:

`unpacked/DocuDeskV1/DocuDeskV1_v6/`

Sondern in:

- `src/DocuDesk.sln`
- `src/DocuDesk.Desktop/`
- `src/DocuDesk.Application/`
- `src/DocuDesk.Persistence/`
- `src/DocuDesk.Infrastructure/`
- `src/DocuDesk.Viewer/`
- `src/DocuDesk.Worker/`

## Konsequenz
Falls ältere Arbeitsanweisungen noch auf `unpacked/...` verweisen, gilt auf diesem Branch **dieser Kontext als maßgeblich**.

## Ziel
1. Build-Fähigkeit der Root-Solution wiederherstellen bzw. absichern.
2. App-Startup der WPF-Desktop-App nachvollziehen.
3. Kleine, sichere Fixes bevorzugen.
4. Ergebnisse verständlich dokumentieren.
