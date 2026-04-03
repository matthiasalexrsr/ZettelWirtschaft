using DocuDesk.Application.Abstractions.Services;

namespace DocuDesk.Infrastructure.Ids;

public sealed class GuidIdGenerator : IIdGenerator
{
    public string NewId() => Guid.NewGuid().ToString("N");
}
