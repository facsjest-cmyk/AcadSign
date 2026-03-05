using System.Security.Cryptography.X509Certificates;

namespace AcadSign.Desktop.Services.Dongle;

public class DongleInfo
{
    public bool IsConnected { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public X509Certificate2? Certificate { get; set; }
    public DateTime? CertificateExpiryDate { get; set; }
    public bool IsCertificateExpired { get; set; }
    public DongleDetectionMethod DetectionMethod { get; set; }
}

public enum DongleDetectionMethod
{
    None,
    PKCS11,
    WindowsCSP
}
