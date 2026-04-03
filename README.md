# ZettelWirtschaft / DocuDesk Recovery Workspace

Dieses Repository enthält die gelieferten ZIP-Archive plus eine lauffähige **DocuDesk-Lite-CLI**, die aus den vorhandenen Datenbank-/Domänenbausteinen zusammengesetzt wurde.

## Enthaltene Archive
- `DocuDeskStarter_v6.zip` (WinUI-basierter Starter)
- `DocuDesk_V1_Starter_v6.zip` (WPF-basierter V1-Stand)
- `DocuDesk_V1_Starter_v6_fresh.zip` (frische Vergleichskopie)

## Unpacked Source Trees
- `unpacked/DocuDeskStarter/DocuDeskStarter`
- `unpacked/DocuDeskV1/DocuDeskV1_v6`
- `unpacked/DocuDeskV1_fresh/DocuDeskV1_v6`

## DocuDesk Lite (funktionierende CLI)
Die CLI nutzt `ddl_v1.sql` + SQLite FTS5 und bietet:
- Datenbank-Initialisierung
- Dokumentimport aus lokalen Dateien
- Dokumentliste
- Volltextsuche

### Start
```bash
python -m app.docudesk_lite --db data/docudesk_lite.db --schema ddl_v1.sql init-db
python -m app.docudesk_lite --db data/docudesk_lite.db import ./beispiel.txt --title "Meine Notiz"
python -m app.docudesk_lite --db data/docudesk_lite.db list
python -m app.docudesk_lite --db data/docudesk_lite.db search notiz
```

## Hinweis zu den Desktop-Projekten
Die vollständigen WinUI/WPF-Lösungen liegen weiterhin unter `unpacked/` und können auf Windows in Visual Studio mit .NET 10 gebaut werden.
