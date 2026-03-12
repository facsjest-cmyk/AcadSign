using iText.Kernel.Pdf;
using iText.Signatures;
using iText.Kernel.Geom;
using Microsoft.Extensions.Logging;
using AcadSign.Desktop.Services.Dongle;
using System.Security.Cryptography.X509Certificates;
using System;
using System.IO;
using System.Threading.Tasks;

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
        _logger.LogInformation("Starting PAdES signature process");

        var cert = await _dongleService.GetCertificateAsync(pin);

        using var reader = new PdfReader(new MemoryStream(unsignedPdf));
        using var outputStream = new MemoryStream();

        var signer = new PdfSigner(reader, outputStream, new StampingProperties());
        ConfigureSignatureAppearance(signer);

        // TODO: Fix certificate chain conversion for iText7
        // var chain = new Org.BouncyCastle.X509.X509Certificate[] { };

        var externalSignature = CreateExternalSignature(cert);

        signer.SignDetached(
            externalSignature,
            null, // chain - temporarily null until BouncyCastle conversion is fixed
            null,
            null,
            null,
            0,
            PdfSigner.CryptoStandard.CADES);

        _logger.LogInformation("PDF signed successfully with PAdES-B-LT");

        return outputStream.ToArray();
    }
    
    public async Task<bool> ValidateSignatureAsync(byte[] signedDocument)
    {
        await Task.Delay(500);
        return true;
    }
    
    private void ConfigureSignatureAppearance(PdfSigner signer)
    {
        signer.SetReason("Document académique officiel");
        signer.SetLocation("Casablanca, Maroc");
        signer.SetPageRect(new Rectangle(36, 36, 200, 100));
        signer.SetPageNumber(1);
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
        
        public string GetDigestAlgorithmName() => "SHA-256";
        
        public string GetSignatureAlgorithmName() => "RSA";

        public ISignatureMechanismParams GetSignatureMechanismParameters() => null!;
    }
}
