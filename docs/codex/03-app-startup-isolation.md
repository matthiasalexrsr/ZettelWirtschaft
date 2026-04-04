# Arbeitspaket 03 – App-Startup nachvollziehen und Startfehler isolieren

## Arbeitsbasis
Arbeite auf diesem Branch in:

- `src/DocuDesk.Desktop/App.xaml`
- `src/DocuDesk.Desktop/App.xaml.cs`
- `src/DocuDesk.Desktop/MainWindow.xaml`
- `src/DocuDesk.Desktop/ViewModels/MainViewModel.cs`
- `src/DocuDesk.Infrastructure/Settings/JsonSettingsStore.cs`
- `src/DocuDesk.Persistence/DependencyInjection/ServiceCollectionExtensions.cs`
- `src/DocuDesk.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs`

## Ziel
Den WPF-Startpfad reproduzierbar verstehen, Startfehler eingrenzen und nur kleine, sichere Fixes vornehmen.

## Zu prüfen
1. Wird `OnStartup()` zuverlässig erreicht?
2. Werden Settings-Datei und Verzeichnisse korrekt angelegt?
3. Werden DI-Services vollständig registriert?
4. Lässt sich `MainWindow` samt `MainViewModel` ohne Auflösungsfehler erzeugen?
5. Führen Migrationen oder Repository-Initialisierung zu Startabbrüchen?
6. Gibt es Fehler in asynchroner Initialisierung (`InitializeAsync`) direkt nach dem Fensterstart?

## Konkretes Vorgehen
1. App lokal unter Windows starten.
2. Falls sie nicht startet:
   - Exception/Stacktrace sichern
   - `crash.log` prüfen
   - ersten auslösenden Fehler isolieren
3. Falls sie startet, aber leer/instabil ist:
   - Initialisierung des `MainViewModel` prüfen
   - erste Datenladepfade prüfen
4. Nur kleine, zielgerichtete Fixes vornehmen.

## Abnahme
Am Ende dokumentieren:
- erster reproduzierbarer Startfehler
- geänderte Dateien
- behobene Ursache
- was jetzt beim Start funktionieren sollte
- nächster sinnvoller technischer Schritt
