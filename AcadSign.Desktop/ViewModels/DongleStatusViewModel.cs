using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AcadSign.Desktop.Services.Dongle;
using System.Timers;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace AcadSign.Desktop.ViewModels;

public partial class DongleStatusViewModel : ObservableObject
{
    private readonly IDongleService _dongleService;
    private readonly System.Timers.Timer _healthCheckTimer;

    public IAsyncRelayCommand VerifyUiCommand { get; }
    
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

        VerifyUiCommand = new AsyncRelayCommand(VerifyUiAsync);
        
        _healthCheckTimer = new System.Timers.Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
        _healthCheckTimer.Elapsed += async (s, e) => await CheckDongleStatusAsync();
        _healthCheckTimer.Start();
        
        _ = CheckDongleStatusAsync();
    }

    private async Task VerifyUiAsync()
    {
        StatusIcon = "⏳";
        StatusMessage = "Vérification en cours...";
        await Task.Yield();
        await CheckDongleStatusAsync();
    }
    
    [RelayCommand]
    private async Task CheckDongleStatusAsync()
    {
        try
        {
            var info = await _dongleService.GetDongleInfoAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                DongleInfo = info;
                IsDongleConnected = info.IsConnected;

                if (!info.IsConnected)
                {
                    StatusIcon = "⚠️";
                    StatusMessage = "Dongle non détecté - Veuillez brancher votre dongle USB";
                }
                else if (info.IsCertificateExpired)
                {
                    StatusIcon = "❌";
                    StatusMessage = $"Certificat expiré le {info.CertificateExpiryDate:dd/MM/yyyy} - Veuillez renouveler votre certificat";
                }
                else
                {
                    StatusIcon = "✅";
                    StatusMessage = $"Dongle connecté - Certificat valide jusqu'au {info.CertificateExpiryDate:dd/MM/yyyy}";
                }
            });
        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusIcon = "❌";
                StatusMessage = $"Erreur: {ex.Message}";
                IsDongleConnected = false;
            });
        }
    }
    
    public void Dispose()
    {
        _healthCheckTimer?.Stop();
        _healthCheckTimer?.Dispose();
    }
}
