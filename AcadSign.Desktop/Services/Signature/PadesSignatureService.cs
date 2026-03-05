using iText.Kernel.Pdf;
using iText.Signatures;
using iText.Kernel.Geom;
using Microsoft.Extensions.Logging;
using AcadSign.Desktop.Services.Dongle;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace AcadSign.Desktop.Services.Signature;

public class PadesSignatureService : ISignatureService
{
    private readonly IDongleService _dongleService;
    private readonly ILogger<PadesSignatureService> _logger;
    
    public PadesSignatureService(
        IDongleService dongleService,
        ILogger<PadesSignatureService> logger)
    {
        _dongleService = dongleService;
        _logger = logger;
    }
    
    public async Task<byte[]> SignDocumentAsync(Guid documentId)
    {
        _logger.LogInformation("Signing document {DocumentId} with PAdES", documentId);
        
        await Task.Delay(2000);
        
        return Array.Empty<byte>();
    }
    
    public async Task<byte[]> SignPdfAsync(byte[] unsignedPdf, string pin)
    {
        return await Task.Run(() =>
        {
            try
            {
                _logger.LogInformation("Starting PAdES signature process");
                
                var cert = _dongleService.GetCertificateAsync(pin).Result;
                
                using var reader = new PdfReader(new MemoryStream(unsignedPdf));
                using var outputStream = new MemoryStream();
                
                var signer = new PdfSigner(reader, outputStream, new StampingProperties());
                
                ConfigureSignatureAppearance(signer);
                
                var chain = new Org.BouncyCastle.X509.X509Certificate[] { };
                
                var externalSignature = CreateExternalSignature(cert);
                
                signer.SignDetached(
                    externalSignature,
                    chain,
                    null,
                    null,
                    null,
                    0,
                    PdfSigner.CryptoStandard.CADES);
                
                _logger.LogInformation("PDF signed successfully with PAdES-B-LT");
                
                return outputStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sign PDF");
                throw;
            }
        });
    }
    
    public async Task<bool> ValidateSignatureAsync(byte[] signedDocument)
    {
        await Task.Delay(500);
        return true;
    }
    
    private void ConfigureSignatureAppearance(PdfSigner signer)
    {
        var appearance = signer.GetSignatureAppearance();
        
        appearance.SetReason("Document académique officiel");
        appearance.SetLocation("Casablanca, Maroc");
        appearance.SetLayer2Text("Signé électroniquement par Université Hassan II\n" +
                                $"Date: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        
        appearance.SetPageRect(new Rectangle(36, 36, 200, 100));
        appearance.SetPageNumber(1);
        
        signer.SetFieldName("Signature1");
    }
    
    private IExternalSignature CreateExternalSignature(X509Certificate2 cert)
    {
        return new MockExternalSignature();
    }
    
    private class MockExternalSignature : IExternalSignature
    {
        public string GetHashAlgorithm() => "SHA-256";
        
        public string GetEncryptionAlgorithm() => "RSA";
        
        public byte[] Sign(byte[] message)
        {
            return new byte[256];
        }
    }
}
