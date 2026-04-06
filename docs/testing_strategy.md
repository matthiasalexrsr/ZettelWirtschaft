# Teststrategie

Die Testbasis ist in eine kleine Hilfsbibliothek und zwei Layer-spezifische Testprojekte aufgeteilt:

- `src/DocuDesk.Testing` für wiederverwendbare Test-Helfer wie temporäre Verzeichnisse und PDF-Fixtures
- `src/DocuDesk.Infrastructure.Tests` für Dateispeicher-, Hashing-, Import- und DI-Tests
- `src/DocuDesk.Persistence.Tests` für SQLite-Factory-, Migrations- und Persistence-DI-Tests

## Ausführen

```bash
dotnet test src/DocuDesk.Tests.sln
```

## Abgedeckte Bereiche

- Dateibasierte Infrastrukturkomponenten
- MIME-/Import-Inspektion mit PDF-Seitenzählung
- SHA256-Hashing und ID-Erzeugung
- SQLite-Verbindungsaufbau und Migrationslauf
- Strukturtests für Dependency-Injection-Registrierungen
