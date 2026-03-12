using AcadSign.Backend.Application.Models;

namespace AcadSign.Backend.Application.Interfaces;

public interface ISisAttestationExportClient
{
    Task<SisAttestationExportResult> GetStudentsAsync(CancellationToken cancellationToken = default);
}
