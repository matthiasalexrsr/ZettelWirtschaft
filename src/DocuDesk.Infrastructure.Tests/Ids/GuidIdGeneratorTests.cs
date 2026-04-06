using DocuDesk.Infrastructure.Ids;

namespace DocuDesk.Infrastructure.Tests.Ids;

public sealed class GuidIdGeneratorTests
{
    [Fact]
    public void NewId_ReturnsCompactLowerCaseGuid()
    {
        var sut = new GuidIdGenerator();

        var value = sut.NewId();

        value.Should().MatchRegex("^[a-f0-9]{32}$");
    }

    [Fact]
    public void NewId_ReturnsUniqueValuesAcrossMultipleCalls()
    {
        var sut = new GuidIdGenerator();

        var values = Enumerable.Range(0, 128)
            .Select(_ => sut.NewId())
            .ToArray();

        values.Should().OnlyHaveUniqueItems();
    }
}
