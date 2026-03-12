using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace AcadSign.Desktop.Services.Dongle;

public interface IDongleService
{
    Task<bool> IsDongleConnectedAsync();
    Task<DongleInfo> GetDongleInfoAsync();
    Task<X509Certificate2> GetCertificateAsync(string pin);
}
