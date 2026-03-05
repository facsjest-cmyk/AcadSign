using Microsoft.Extensions.Logging;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System.Security.Cryptography.X509Certificates;

namespace AcadSign.Desktop.Services.Dongle;

public class Pkcs11DongleService : IDongleService
{
    private readonly ILogger<Pkcs11DongleService> _logger;
    private const string PKCS11_LIBRARY_PATH = "baridmb.dll";
    private const string ISSUER_NAME = "Barid Al-Maghrib";
    
    public Pkcs11DongleService(ILogger<Pkcs11DongleService> logger)
    {
        _logger = logger;
    }
    
    public async Task<bool> IsDongleConnectedAsync()
    {
        return await Task.Run(() =>
        {
            if (TryDetectViaPKCS11())
            {
                _logger.LogInformation("Dongle detected via PKCS#11");
                return true;
            }
            
            if (TryDetectViaCSP())
            {
                _logger.LogInformation("Dongle detected via Windows CSP");
                return true;
            }
            
            _logger.LogWarning("No dongle detected");
            return false;
        });
    }
    
    public async Task<DongleInfo> GetDongleInfoAsync()
    {
        return await Task.Run(() =>
        {
            if (TryGetInfoViaPKCS11(out var pkcs11Info))
            {
                pkcs11Info.DetectionMethod = DongleDetectionMethod.PKCS11;
                _logger.LogInformation("Dongle info retrieved via PKCS#11");
                return pkcs11Info;
            }
            
            if (TryGetInfoViaCSP(out var cspInfo))
            {
                cspInfo.DetectionMethod = DongleDetectionMethod.WindowsCSP;
                _logger.LogInformation("Dongle info retrieved via Windows CSP");
                return cspInfo;
            }
            
            _logger.LogWarning("No dongle info available");
            return new DongleInfo
            {
                IsConnected = false,
                DetectionMethod = DongleDetectionMethod.None
            };
        });
    }
    
    public async Task<X509Certificate2> GetCertificateAsync(string pin)
    {
        return await Task.Run(() =>
        {
            try
            {
                var cert = GetCertificateViaPKCS11(pin);
                _logger.LogInformation("Certificate retrieved via PKCS#11");
                return cert;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get certificate via PKCS#11, trying CSP");
            }
            
            var cspCert = GetCertificateViaCSP();
            _logger.LogInformation("Certificate retrieved via Windows CSP");
            return cspCert;
        });
    }
    
    private bool TryDetectViaPKCS11()
    {
        try
        {
            using var pkcs11 = new Pkcs11InteropFactories().Pkcs11LibraryFactory
                .LoadPkcs11Library(new Pkcs11InteropFactories(), PKCS11_LIBRARY_PATH, AppType.MultiThreaded);
            
            var slots = pkcs11.GetSlotList(SlotsType.WithTokenPresent);
            return slots.Count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "PKCS#11 detection failed");
            return false;
        }
    }
    
    private bool TryDetectViaCSP()
    {
        try
        {
            using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            
            var certs = store.Certificates.Find(
                X509FindType.FindByIssuerName,
                ISSUER_NAME,
                false);
            
            return certs.Count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "CSP detection failed");
            return false;
        }
    }
    
    private bool TryGetInfoViaPKCS11(out DongleInfo info)
    {
        info = new DongleInfo();
        
        try
        {
            using var pkcs11 = new Pkcs11InteropFactories().Pkcs11LibraryFactory
                .LoadPkcs11Library(new Pkcs11InteropFactories(), PKCS11_LIBRARY_PATH, AppType.MultiThreaded);
            
            var slots = pkcs11.GetSlotList(SlotsType.WithTokenPresent);
            if (slots.Count == 0)
            {
                return false;
            }
            
            var slot = slots[0];
            var tokenInfo = slot.GetTokenInfo();
            
            info.IsConnected = true;
            info.SerialNumber = tokenInfo.SerialNumber;
            info.Label = tokenInfo.Label;
            info.Manufacturer = tokenInfo.ManufacturerId;
            
            using var session = slot.OpenSession(SessionType.ReadOnly);
            var searchTemplate = new List<IObjectAttribute>
            {
                session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_CERTIFICATE)
            };
            
            var certObjects = session.FindAllObjects(searchTemplate);
            
            if (certObjects.Count > 0)
            {
                var certAttrs = session.GetAttributeValue(certObjects[0], new List<CKA> { CKA.CKA_VALUE });
                var certBytes = certAttrs[0].GetValueAsByteArray();
                
                info.Certificate = new X509Certificate2(certBytes);
                info.CertificateExpiryDate = info.Certificate.NotAfter;
                info.IsCertificateExpired = DateTime.Now > info.Certificate.NotAfter;
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to get info via PKCS#11");
            return false;
        }
    }
    
    private bool TryGetInfoViaCSP(out DongleInfo info)
    {
        info = new DongleInfo();
        
        try
        {
            using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            
            var certs = store.Certificates.Find(
                X509FindType.FindByIssuerName,
                ISSUER_NAME,
                false);
            
            if (certs.Count == 0)
            {
                return false;
            }
            
            var cert = certs[0];
            
            info.IsConnected = true;
            info.Certificate = cert;
            info.SerialNumber = cert.SerialNumber;
            info.CertificateExpiryDate = cert.NotAfter;
            info.IsCertificateExpired = DateTime.Now > cert.NotAfter;
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to get info via CSP");
            return false;
        }
    }
    
    private X509Certificate2 GetCertificateViaPKCS11(string pin)
    {
        using var pkcs11 = new Pkcs11InteropFactories().Pkcs11LibraryFactory
            .LoadPkcs11Library(new Pkcs11InteropFactories(), PKCS11_LIBRARY_PATH, AppType.MultiThreaded);
        
        var slots = pkcs11.GetSlotList(SlotsType.WithTokenPresent);
        if (slots.Count == 0)
        {
            throw new InvalidOperationException("No dongle detected");
        }
        
        var slot = slots[0];
        using var session = slot.OpenSession(SessionType.ReadOnly);
        
        session.Login(CKU.CKU_USER, pin);
        
        var searchTemplate = new List<IObjectAttribute>
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_CERTIFICATE)
        };
        
        var certObjects = session.FindAllObjects(searchTemplate);
        
        if (certObjects.Count == 0)
        {
            throw new InvalidOperationException("No certificate found on dongle");
        }
        
        var certAttrs = session.GetAttributeValue(certObjects[0], new List<CKA> { CKA.CKA_VALUE });
        var certBytes = certAttrs[0].GetValueAsByteArray();
        
        session.Logout();
        
        return new X509Certificate2(certBytes);
    }
    
    private X509Certificate2 GetCertificateViaCSP()
    {
        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly);
        
        var certs = store.Certificates.Find(
            X509FindType.FindByIssuerName,
            ISSUER_NAME,
            false);
        
        if (certs.Count == 0)
        {
            throw new InvalidOperationException("No certificate found");
        }
        
        return certs[0];
    }
}
