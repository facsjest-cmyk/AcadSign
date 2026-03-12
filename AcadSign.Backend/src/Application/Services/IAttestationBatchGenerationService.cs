using AcadSign.Backend.Application.Common.Models;
using AcadSign.Backend.Domain.Enums;

namespace AcadSign.Backend.Application.Services;

public interface IAttestationBatchGenerationService
{
    Task<SisAttestationBatchGenerationResult> GenerateFromSisAsync(
        DocumentType documentType,
        CancellationToken cancellationToken = default);
}
