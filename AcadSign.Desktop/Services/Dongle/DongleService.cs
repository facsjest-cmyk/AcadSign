using System;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace AcadSign.Desktop.Services.Dongle;

public class DongleService : IDongleService
{
    public async Task<bool> IsDongleConnectedAsync()
    {
        await Task.Delay(500);
        return true;
    }
    
    public async Task<DongleInfo> GetDongleInfoAsync()
    {
        await Task.Delay(300);
        return new DongleInfo
        {
            IsConnected = true,
            SerialNumber = "MOCK-12345",
            Manufacturer = "Mock Manufacturer"
        };
    }
    
    public async Task<X509Certificate2> GetCertificateAsync(string pin)
    {
        await Task.Delay(200);
        // Mock implementation - returns a self-signed certificate
        throw new NotImplementedException("Mock dongle service - certificate retrieval not implemented");
    }
}
