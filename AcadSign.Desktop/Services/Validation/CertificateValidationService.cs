using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Security;
using System.Security.Cryptography.X509Certificates;

namespace AcadSign.Desktop.Services.Validation;

public class CertificateValidationService : ICertificateValidationService
{
    private readonly ILogger<CertificateValidationService> _logger;
    private readonly HttpClient _httpClient;
    private const string OCSP_URL = "http://ocsp.baridmb.ma";
    private const string CRL_URL = "http://crl.baridmb.ma/barid.crl";
    
    public CertificateValidationService(
        ILogger<CertificateValidationService> logger,
        HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }
    
    public async Task<CertificateValidationResult> ValidateCertificateAsync(X509Certificate2 cert)
    {
        if (DateTime.Now > cert.NotAfter)
        {
            return new CertificateValidationResult
            {
                Status = CertificateStatus.Expired,
                Message = $"Certificat expiré le {cert.NotAfter:dd/MM/yyyy}",
                Method = ValidationMethod.None
            };
        }
        
        try
        {
            var ocspResult = await ValidateViaOcspAsync(cert);
            if (ocspResult.Status != CertificateStatus.Unknown)
            {
                return ocspResult;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OCSP validation failed, trying CRL");
        }
        
        try
        {
            return await ValidateViaCrlAsync(cert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CRL validation failed");
            return new CertificateValidationResult
            {
                Status = CertificateStatus.Unknown,
                Message = "Impossible de valider le certificat",
                Method = ValidationMethod.None
            };
        }
    }
    
    private async Task<CertificateValidationResult> ValidateViaOcspAsync(X509Certificate2 cert)
    {
        var bcCert = DotNetUtilities.FromX509Certificate(cert);
        var issuerCert = GetIssuerCertificate(cert);
        var bcIssuerCert = DotNetUtilities.FromX509Certificate(issuerCert);
        
        var ocspReqGen = new OcspReqGenerator();
        var certId = new CertificateID(
            CertificateID.HashSha1,
            bcIssuerCert,
            bcCert.SerialNumber);
        ocspReqGen.AddRequest(certId);
        
        var ocspReq = ocspReqGen.Generate();
        var encodedReq = ocspReq.GetEncoded();
        var content = new ByteArrayContent(encodedReq);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/ocsp-request");
        
        var response = await _httpClient.PostAsync(OCSP_URL, content);
        response.EnsureSuccessStatusCode();
        
        var encodedResp = await response.Content.ReadAsByteArrayAsync();
        var ocspResp = new OcspResp(encodedResp);
        
        if (ocspResp.Status == OcspRespStatus.Successful)
        {
            var basicResp = (BasicOcspResp)ocspResp.GetResponseObject();
            var singleResp = basicResp.Responses[0];
            var certStatus = singleResp.GetCertStatus();
            
            if (certStatus == Org.BouncyCastle.Ocsp.CertificateStatus.Good)
            {
                return new CertificateValidationResult
                {
                    Status = CertificateStatus.Valid,
                    Message = "Certificat valide (OCSP)",
                    Method = ValidationMethod.OCSP
                };
            }
            else if (certStatus is RevokedStatus revokedStatus)
            {
                return new CertificateValidationResult
                {
                    Status = CertificateStatus.Revoked,
                    Message = "Certificat révoqué",
                    RevocationDate = revokedStatus.RevocationTime,
                    Method = ValidationMethod.OCSP
                };
            }
        }
        
        return new CertificateValidationResult
        {
            Status = CertificateStatus.Unknown,
            Message = "Statut OCSP inconnu",
            Method = ValidationMethod.OCSP
        };
    }
    
    private async Task<CertificateValidationResult> ValidateViaCrlAsync(X509Certificate2 cert)
    {
        var crlBytes = await _httpClient.GetByteArrayAsync(CRL_URL);
        var crlParser = new X509CrlParser();
        var crl = crlParser.ReadCrl(crlBytes);
        
        var bcCert = DotNetUtilities.FromX509Certificate(cert);
        var isRevoked = crl.IsRevoked(bcCert);
        
        if (isRevoked)
        {
            var revokedCert = crl.GetRevokedCertificate(bcCert.SerialNumber);
            return new CertificateValidationResult
            {
                Status = CertificateStatus.Revoked,
                Message = "Certificat révoqué",
                RevocationDate = revokedCert.RevocationDate,
                Method = ValidationMethod.CRL
            };
        }
        
        return new CertificateValidationResult
        {
            Status = CertificateStatus.Valid,
            Message = "Certificat valide (CRL)",
            Method = ValidationMethod.CRL
        };
    }
    
    private X509Certificate2 GetIssuerCertificate(X509Certificate2 cert)
    {
        var chain = new X509Chain();
        chain.Build(cert);
        
        foreach (var element in chain.ChainElements)
        {
            if (element.Certificate.Subject == cert.Issuer)
            {
                return element.Certificate;
            }
        }
        
        throw new InvalidOperationException("Issuer certificate not found");
    }
}
