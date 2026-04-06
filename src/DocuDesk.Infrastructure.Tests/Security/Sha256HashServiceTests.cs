using System.Security.Cryptography;
using System.Text;
using DocuDesk.Infrastructure.Security;

namespace DocuDesk.Infrastructure.Tests.Security;

public sealed class Sha256HashServiceTests
{
    [Fact]
    public async Task ComputeSha256Async_ReturnsExpectedLowerCaseHash()
    {
        using var scope = new TemporaryDirectoryScope();
        var filePath = scope.CreateFile("fixtures/sample.txt", "Hallo DocuDesk");
        var sut = new Sha256HashService();

        var hash = await sut.ComputeSha256Async(filePath, CancellationToken.None);

        var expected = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("Hallo DocuDesk")))
            .ToLowerInvariant();

        hash.Should().Be(expected);
    }
}
