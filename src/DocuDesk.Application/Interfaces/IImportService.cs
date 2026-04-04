using DocuDesk.Contracts.Documents;

namespace DocuDesk.Application.Interfaces;

public interface IImportService
{
    Task<DocumentImportResult> ImportAsync(DocumentImportRequest request, CancellationToken cancellationToken = default);
}
