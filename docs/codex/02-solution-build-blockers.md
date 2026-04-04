# Arbeitspaket 02 – Solution analysieren und typische Build-Blocker systematisch beheben

## Arbeitsbasis
Arbeite auf diesem Branch in:

- `src/DocuDesk.sln`
- `src/DocuDesk.Desktop/`
- `src/DocuDesk.Application/`
- `src/DocuDesk.Persistence/`
- `src/DocuDesk.Infrastructure/`
- `src/DocuDesk.Viewer/`
- `src/DocuDesk.Worker/`

## Ziel
Die Root-Solution erfolgreich wiederherstellen, Build-Blocker systematisch isolieren und nur kleine, sichere Korrekturen vornehmen.

## Typische Prüfpunkte
1. fehlende oder falsche Projektverweise
2. inkonsistente Namespaces nach Ordner-/Repo-Umbauten
3. fehlende NuGet-Pakete oder falsche Paketversionen
4. Altpfade in `.csproj`, Workflows oder Content-Includes
5. Typen/Interfaces, die aus einem alten Stand übernommen wurden, aber in der Root-Solution nicht mehr konsistent sind

## Vorgehen
1. Restore der Solution ausführen.
2. Build der Solution ausführen.
3. Fehler gruppieren nach:
   - Projektverweise
   - Namespaces / using-Direktiven
   - fehlende Dateien / Ressourcen
   - API-Mismatch zwischen Interfaces und Implementierungen
4. Änderungen in kleinen Schritten vornehmen.
5. Nach jedem Fix erneut Build ausführen.

## Abnahme
Am Ende dokumentieren:
- konkrete Build-Fehler
- geänderte Dateien
- welche Fehler behoben wurden
- ob die Solution jetzt buildet
- welche Restfehler ggf. noch offen sind
