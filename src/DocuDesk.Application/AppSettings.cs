namespace DocuDesk.Application;

public sealed class AppSettings
{
    public string AppDataRoot { get; set; } = string.Empty;
    public string RepositoryRoot { get; set; } = string.Empty;
    public string DatabasePath { get; set; } = string.Empty;
    public string BackupsPath { get; set; } = string.Empty;
    public string LogsPath { get; set; } = string.Empty;
    public string CacheRoot { get; set; } = string.Empty;
    public string OcrArtifactsRoot { get; set; } = string.Empty;
    public string TesseractExecutablePath { get; set; } = "tesseract";
    public string? TesseractDataPath { get; set; }
    public string OcrLanguage { get; set; } = "deu+eng";
}
