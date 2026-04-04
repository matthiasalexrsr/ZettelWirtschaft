# Arbeitspaket 05 – Windows Build- und Startup-Validierung

## Ziel
In einer echten Windows-Umgebung mit installiertem .NET 9 SDK die Root-Solution wiederherstellen, bauen und den ersten reproduzierbaren Startup-Fehler der WPF-App isolieren.

## Arbeitsbasis
- `src/DocuDesk.sln`
- `src/DocuDesk.Desktop/App.xaml.cs`
- `src/DocuDesk.Desktop/MainWindow.xaml`
- `src/DocuDesk.Desktop/MainWindow.xaml.cs`
- `src/DocuDesk.Desktop/ViewModels/MainViewModel.cs`
- `src/DocuDesk.Persistence/DependencyInjection/ServiceCollectionExtensions.cs`
- `src/DocuDesk.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs`
- `src/DocuDesk.Infrastructure/Settings/JsonSettingsStore.cs`

## Befehle
```powershell
dotnet restore src/DocuDesk.sln
dotnet build src/DocuDesk.sln
dotnet test src/DocuDesk.sln
```

Falls keine Testprojekte vorhanden sind, `dotnet test` dokumentieren und nicht künstlich ergänzen.

## Startup-Validierung
1. `DocuDesk.Desktop` als Startprojekt verwenden.
2. App lokal unter Windows starten.
3. Falls Startup fehlschlägt:
   - Exception/Stacktrace sichern
   - `crash.log` prüfen
   - erste auslösende Klasse/Zeile dokumentieren
4. Falls App startet:
   - prüfen, ob `MainWindow` erscheint
   - prüfen, ob `MainViewModel` initialisiert
   - prüfen, ob Migrationen erfolgreich laufen

## Ergebnisdokumentation
Am Ende dokumentieren:
- geänderte Dateien
- konkrete Restore-/Build-Fehler gruppiert
- behobene Probleme
- erster reproduzierbarer Startup-Fehler mit Stacktrace und Klasse/Zeile
- was jetzt funktioniert
- nächster sinnvoller Schritt
