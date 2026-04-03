namespace DocuDesk.Application.Abstractions.Services;

public interface IHashService
{
    Task<string> ComputeSha256Async(string filePath, CancellationToken ct);
}
