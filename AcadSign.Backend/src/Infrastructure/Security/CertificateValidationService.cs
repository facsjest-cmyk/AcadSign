using System.Security.Cryptography.X509Certificates;
using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Domain.Enums;

namespace AcadSign.Backend.Infrastructure.Security;

public class CertificateValidationService : ICertificateValidationService
{
    public Task<CertificateStatus> ValidateCertificateAsync(byte[] certificateData, CancellationToken cancellationToken = default)
    {
        try
        {
            var cert = X509CertificateLoader.LoadCertificate(certificateData);
            if (cert.NotAfter < DateTime.UtcNow)
            {
                return Task.FromResult(CertificateStatus.Expired);
            }

            return Task.FromResult(CertificateStatus.Valid);
        }
        catch
        {
            return Task.FromResult(CertificateStatus.Invalid);
        }
    }

    public async Task<bool> IsCertificateValidAsync(byte[] certificateData, CancellationToken cancellationToken = default)
    {
        return await ValidateCertificateAsync(certificateData, cancellationToken) == CertificateStatus.Valid;
    }

    public Task<CertificateStatus> CheckCertificateStatusAsync(string certificateThumbprint, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CertificateStatus.Unknown);
    }
}
