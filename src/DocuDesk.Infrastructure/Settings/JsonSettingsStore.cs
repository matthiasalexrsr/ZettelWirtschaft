using System.Text.Json;
using DocuDesk.Application;
using DocuDesk.Application.Interfaces;

namespace DocuDesk.Infrastructure.Settings;

public sealed class JsonSettingsStore : ISettingsStore
{
    private readonly string _settingsFilePath;

    public JsonSettingsStore(string settingsFilePath)
    {
        _settingsFilePath = settingsFilePath;
        EnsureParentDirectoryExists(settingsFilePath);
    }

    private static void EnsureParentDirectoryExists(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_settingsFilePath))
        {
            var defaults = CreateDefaultSettings();
            await SaveAsync(defaults, cancellationToken);
            return defaults;
        }

        var json = await File.ReadAllTextAsync(_settingsFilePath, cancellationToken);
        var settings = JsonSerializer.Deserialize<AppSettings>(json) ?? CreateDefaultSettings();
        EnsureDerivedPaths(settings);
        return settings;
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        EnsureDerivedPaths(settings);
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_settingsFilePath, json, cancellationToken);
    }

    private static AppSettings CreateDefaultSettings()
    {
        var appRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DocuDesk");
        var settings = new AppSettings
        {
            AppDataRoot = appRoot,
            RepositoryRoot = Path.Combine(appRoot, "Repository"),
            DatabasePath = Path.Combine(appRoot, "db", "app.db"),
            BackupsPath = Path.Combine(appRoot, "backups"),
            LogsPath = Path.Combine(appRoot, "logs"),
            TesseractExecutablePath = "tesseract",
            OcrLanguage = "deu+eng"
        };
        EnsureDerivedPaths(settings);
        return settings;
    }

    private static void EnsureDerivedPaths(AppSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.AppDataRoot))
        {
            settings.AppDataRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DocuDesk");
        }

        settings.RepositoryRoot = string.IsNullOrWhiteSpace(settings.RepositoryRoot)
            ? Path.Combine(settings.AppDataRoot, "Repository")
            : settings.RepositoryRoot;
        settings.DatabasePath = string.IsNullOrWhiteSpace(settings.DatabasePath)
            ? Path.Combine(settings.AppDataRoot, "db", "app.db")
            : settings.DatabasePath;
        settings.BackupsPath = string.IsNullOrWhiteSpace(settings.BackupsPath)
            ? Path.Combine(settings.AppDataRoot, "backups")
            : settings.BackupsPath;
        settings.LogsPath = string.IsNullOrWhiteSpace(settings.LogsPath)
            ? Path.Combine(settings.AppDataRoot, "logs")
            : settings.LogsPath;
        settings.CacheRoot = string.IsNullOrWhiteSpace(settings.CacheRoot)
            ? Path.Combine(settings.AppDataRoot, "cache")
            : settings.CacheRoot;
        settings.OcrArtifactsRoot = string.IsNullOrWhiteSpace(settings.OcrArtifactsRoot)
            ? Path.Combine(settings.AppDataRoot, "ocr")
            : settings.OcrArtifactsRoot;
        settings.TesseractExecutablePath = string.IsNullOrWhiteSpace(settings.TesseractExecutablePath)
            ? "tesseract"
            : settings.TesseractExecutablePath;
        settings.OcrLanguage = string.IsNullOrWhiteSpace(settings.OcrLanguage)
            ? "deu+eng"
            : settings.OcrLanguage;
    }
}
