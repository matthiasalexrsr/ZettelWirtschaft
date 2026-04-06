namespace DocuDesk.Infrastructure.Ocr;

public sealed class TesseractOcrOptions
{
    public string TesseractExecutablePath { get; set; } = "tesseract";
    public string PdfToPpmExecutablePath { get; set; } = "pdftoppm";
    public int PdfRasterDpi { get; set; } = 200;
    public string DefaultLanguage { get; set; } = "deu+eng";
}
