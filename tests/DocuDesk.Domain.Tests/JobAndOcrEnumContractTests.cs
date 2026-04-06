using DocuDesk.Domain.Enums;
using FluentAssertions;

namespace DocuDesk.Domain.Tests;

public sealed class JobAndOcrEnumContractTests
{
    [Fact]
    public void OcrStatus_values_should_remain_stable()
    {
        ((int)OcrStatus.NotStarted).Should().Be(0);
        ((int)OcrStatus.ExtractedEmbeddedText).Should().Be(1);
        ((int)OcrStatus.Completed).Should().Be(2);
        ((int)OcrStatus.Failed).Should().Be(3);
    }

    [Fact]
    public void JobStatus_values_should_cover_expected_processing_states()
    {
        Enum.GetNames<JobStatus>().Should().Contain(new[]
        {
            nameof(JobStatus.Pending),
            nameof(JobStatus.Running),
            nameof(JobStatus.Succeeded),
            nameof(JobStatus.Failed),
            nameof(JobStatus.RetryScheduled),
            nameof(JobStatus.Cancelled)
        });
    }
}
