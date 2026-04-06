# Tests

## Lokale Ausführung
```powershell
dotnet restore src/DocuDesk.sln
dotnet restore tests/DocuDesk.Domain.Tests/DocuDesk.Domain.Tests.csproj
dotnet build tests/DocuDesk.Domain.Tests/DocuDesk.Domain.Tests.csproj --configuration Release --no-restore
dotnet test tests/DocuDesk.Domain.Tests/DocuDesk.Domain.Tests.csproj --configuration Release --no-build --settings tests/DocuDesk.runsettings --collect:"XPlat Code Coverage"
```

## Enthalten
- xUnit-Testprojekt für Domain-Tests
- FluentAssertions für lesbare Assertions
- Coverlet-Collector für Coverage
- Windows-GitHub-Workflow für Restore/Build/Test

## Nächste Ausbaustufen
- Integrations-Tests für SQLite-Migrationen
- Tests für `JsonSettingsStore`
- Tests für `MainViewModel`
- Startup-/DI-Smoke-Tests
