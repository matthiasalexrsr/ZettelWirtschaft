using DocuDesk.Application;
using DocuDesk.Infrastructure.Settings;

namespace DocuDesk.Tests.Infrastructure;

public sealed class JsonSettingsStoreTests
{
    [Fact]
    public async Task LoadAsync_CreatesDefaults_WhenSettingsFileDoesNotExist()
    {
        using var temp = new TempDirectory();
        var settingsPath = Path.Combine(temp.Path, "config", "settings.json");
        var store = new JsonSettingsStore(settingsPath);

        var settings = await store.LoadAsync();

        Assert.True(File.Exists(settingsPath));
        Assert.False(string.IsNullOrWhiteSpace(settings.AppDataRoot));
        Assert.False(string.IsNullOrWhiteSpace(settings.RepositoryRoot));
        Assert.False(string.IsNullOrWhiteSpace(settings.DatabasePath));
        Assert.False(string.IsNullOrWhiteSpace(settings.BackupsPath));
        Assert.False(string.IsNullOrWhiteSpace(settings.LogsPath));
        Assert.False(string.IsNullOrWhiteSpace(settings.CacheRoot));
        Assert.False(string.IsNullOrWhiteSpace(settings.OcrArtifactsRoot));
    }

    [Fact]
    public async Task LoadAsync_PreservesExplicitValues_AndFillsDerivedValues()
    {
        using var temp = new TempDirectory();
        var appRoot = Path.Combine(temp.Path, "app-data");
        var settingsPath = Path.Combine(temp.Path, "settings.json");

        var json = """
            {
              "AppDataRoot": "",
              "RepositoryRoot": "C:/repo/custom",
              "DatabasePath": "",
              "BackupsPath": "",
              "LogsPath": "",
              "CacheRoot": "",
              "OcrArtifactsRoot": "",
              "TesseractExecutablePath": "",
              "OcrLanguage": ""
            }
            """;
        await File.WriteAllTextAsync(settingsPath, json);

        var store = new JsonSettingsStore(settingsPath);
        var settings = await store.LoadAsync();

        Assert.False(string.IsNullOrWhiteSpace(settings.AppDataRoot));
        Assert.Equal("C:/repo/custom", settings.RepositoryRoot);
        Assert.False(string.IsNullOrWhiteSpace(settings.DatabasePath));
        Assert.False(string.IsNullOrWhiteSpace(settings.BackupsPath));
        Assert.False(string.IsNullOrWhiteSpace(settings.LogsPath));
        Assert.False(string.IsNullOrWhiteSpace(settings.CacheRoot));
        Assert.False(string.IsNullOrWhiteSpace(settings.OcrArtifactsRoot));
        Assert.Equal("tesseract", settings.TesseractExecutablePath);
        Assert.Equal("deu+eng", settings.OcrLanguage);
        Assert.NotEqual(appRoot, settings.AppDataRoot);
    }

    [Fact]
    public async Task SaveAsync_ThenLoadAsync_RoundTripsConfiguredValues()
    {
        using var temp = new TempDirectory();
        var settingsPath = Path.Combine(temp.Path, "settings", "settings.json");
        var store = new JsonSettingsStore(settingsPath);

        var original = new AppSettings
        {
            AppDataRoot = Path.Combine(temp.Path, "custom"),
            RepositoryRoot = Path.Combine(temp.Path, "custom", "repo"),
            DatabasePath = Path.Combine(temp.Path, "custom", "db", "app.db"),
            BackupsPath = Path.Combine(temp.Path, "custom", "backups"),
            LogsPath = Path.Combine(temp.Path, "custom", "logs"),
            CacheRoot = Path.Combine(temp.Path, "custom", "cache"),
            OcrArtifactsRoot = Path.Combine(temp.Path, "custom", "ocr"),
            TesseractExecutablePath = "tesseract-custom",
            OcrLanguage = "eng"
        };

        await store.SaveAsync(original);
        var loaded = await store.LoadAsync();

        Assert.Equal(original.AppDataRoot, loaded.AppDataRoot);
        Assert.Equal(original.RepositoryRoot, loaded.RepositoryRoot);
        Assert.Equal(original.DatabasePath, loaded.DatabasePath);
        Assert.Equal(original.BackupsPath, loaded.BackupsPath);
        Assert.Equal(original.LogsPath, loaded.LogsPath);
        Assert.Equal(original.CacheRoot, loaded.CacheRoot);
        Assert.Equal(original.OcrArtifactsRoot, loaded.OcrArtifactsRoot);
        Assert.Equal(original.TesseractExecutablePath, loaded.TesseractExecutablePath);
        Assert.Equal(original.OcrLanguage, loaded.OcrLanguage);
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"docudesk-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Path, recursive: true);
            }
            catch
            {
                // Best-effort cleanup for tests.
            }
        }
    }
}
