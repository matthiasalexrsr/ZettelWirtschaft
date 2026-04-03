using DocuDesk.Application.Abstractions.Services;

namespace DocuDesk.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
