using DocuDesk.Application.Abstractions.Services;
using System.Security.Cryptography;

namespace DocuDesk.Infrastructure.Security;

public sealed class Sha256HashService : IHashService
{
    public async Task<string> ComputeSha256Async(string filePath, CancellationToken ct)
    {
        await using var stream = File.OpenRead(filePath);
        using var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(stream, ct);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
