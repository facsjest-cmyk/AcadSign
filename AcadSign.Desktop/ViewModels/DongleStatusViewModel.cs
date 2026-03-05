using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AcadSign.Desktop.Services.Dongle;
using System.Timers;

namespace AcadSign.Desktop.ViewModels;

public partial class DongleStatusViewModel : ObservableObject
{
    private readonly IDongleService _dongleService;
    private readonly System.Timers.Timer _healthCheckTimer;
    
    [ObservableProperty]
    private string _statusMessage = "Vérification en cours...";
    
    [ObservableProperty]
    private string _statusIcon = "⏳";
    
    [ObservableProperty]
    private bool _isDongleConnected;
    
    [ObservableProperty]
    private DongleInfo? _dongleInfo;
    
    public DongleStatusViewModel(IDongleService dongleService)
    {
        _dongleService = dongleService;
        
        _healthCheckTimer = new System.Timers.Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
        _healthCheckTimer.Elapsed += async (s, e) => await CheckDongleStatusAsync();
        _healthCheckTimer.Start();
        
        _ = CheckDongleStatusAsync();
    }
    
    [RelayCommand]
    private async Task CheckDongleStatusAsync()
    {
        try
        {
            DongleInfo = await _dongleService.GetDongleInfoAsync();
            IsDongleConnected = DongleInfo.IsConnected;
            
            if (!DongleInfo.IsConnected)
            {
                StatusIcon = "⚠️";
                StatusMessage = "Dongle non détecté - Veuillez brancher votre dongle USB";
            }
            else if (DongleInfo.IsCertificateExpired)
            {
                StatusIcon = "❌";
                StatusMessage = $"Certificat expiré le {DongleInfo.CertificateExpiryDate:dd/MM/yyyy} - Veuillez renouveler votre certificat";
            }
            else
            {
                StatusIcon = "✅";
                StatusMessage = $"Dongle connecté - Certificat valide jusqu'au {DongleInfo.CertificateExpiryDate:dd/MM/yyyy}";
            }
        }
        catch (Exception ex)
        {
            StatusIcon = "❌";
            StatusMessage = $"Erreur: {ex.Message}";
            IsDongleConnected = false;
        }
    }
    
    public void Dispose()
    {
        _healthCheckTimer?.Stop();
        _healthCheckTimer?.Dispose();
    }
}
