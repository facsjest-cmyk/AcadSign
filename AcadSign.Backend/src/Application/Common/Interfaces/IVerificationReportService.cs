namespace AcadSign.Backend.Application.Common.Interfaces;

public interface IVerificationReportService
{
    Task<byte[]> GenerateVerificationReportAsync(
        AcadSign.Backend.Application.Common.Models.DocumentVerificationResult verification,
        CancellationToken cancellationToken = default);
}
