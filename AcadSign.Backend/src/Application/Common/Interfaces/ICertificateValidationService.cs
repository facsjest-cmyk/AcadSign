using AcadSign.Backend.Domain.Enums;

namespace AcadSign.Backend.Application.Common.Interfaces;

public interface ICertificateValidationService
{
    Task<CertificateStatus> ValidateCertificateAsync(byte[] certificateData, CancellationToken cancellationToken = default);
    Task<bool> IsCertificateValidAsync(byte[] certificateData, CancellationToken cancellationToken = default);
    Task<CertificateStatus> CheckCertificateStatusAsync(string certificateThumbprint, CancellationToken cancellationToken = default);
}
