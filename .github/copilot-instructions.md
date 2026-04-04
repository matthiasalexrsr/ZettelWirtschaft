# Arbeitsanweisungen für Codex / Copilot

## Ziel dieses Repositories
Dieses Repository dient aktuell als **Recovery- und Aufbau-Workspace** für die Windows-Anwendung **DocuDesk**.

Der wichtigste aktive Entwicklungsstand ist derzeit:

`unpacked/DocuDeskV1/DocuDeskV1_v6/`

## Verbindliche Arbeitsbasis
Arbeite standardmäßig **nur** in diesem Ordner:

- `unpacked/DocuDeskV1/DocuDeskV1_v6/`

Bevorzuge diesen WPF-Stand ausdrücklich gegenüber:

- ZIP-Archiven im Repository
- älteren/frischen Vergleichskopien
- alternativen WinUI-Starterständen

## Technische Leitplanken
- Bestehende Architektur zunächst respektieren.
- Kleine, sichere Änderungen bevorzugen.
- Zuerst Stabilität und Build-Fähigkeit herstellen, erst danach neue Features.
- Keine unnötigen Umbenennungen oder großflächigen Verschiebungen.
- Änderungen so dokumentieren, dass auch Nicht-Programmierer sie nachvollziehen können.

## Prioritätenreihenfolge
1. Solution muss wiederherstellbar und buildbar sein.
2. Desktop-Startpfad muss konsistent sein.
3. Persistenz / SQLite / Settings / Viewer / Worker dürfen keine offensichtlichen Broken Links enthalten.
4. Erst danach neue Funktionen erweitern.

## Bei Unklarheiten
Wenn mehrere mögliche Codepfade vorhanden sind, wähle standardmäßig den Pfad

`unpacked/DocuDeskV1/DocuDeskV1_v6/DocuDesk.sln`

und dokumentiere kurz, warum andere Pfade nicht verwendet wurden.

## Erwartete Ergebnisdokumentation
Bei jeder relevanten Änderung nach Möglichkeit dokumentieren:
- geänderte Dateien
- behobene Fehler
- noch offene Probleme
- nächster sinnvoller Schritt
