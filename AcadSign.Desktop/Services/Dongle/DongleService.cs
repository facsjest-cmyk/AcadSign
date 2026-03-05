namespace AcadSign.Desktop.Services.Dongle;

public class DongleService : IDongleService
{
    public async Task<bool> DetectDongleAsync()
    {
        await Task.Delay(500);
        return true;
    }
    
    public async Task<string> GetCertificateAsync()
    {
        await Task.Delay(300);
        return "Certificate Data";
    }
    
    public async Task<bool> ValidatePinAsync(string pin)
    {
        await Task.Delay(200);
        return !string.IsNullOrEmpty(pin);
    }
}
