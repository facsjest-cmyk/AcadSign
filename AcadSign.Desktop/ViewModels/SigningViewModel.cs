using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AcadSign.Desktop.Services.Api;
using AcadSign.Desktop.Services.Dongle;
using AcadSign.Desktop.Services.Navigation;
using AcadSign.Desktop.Services.Signature;
using AcadSign.Desktop.Services.Validation;
using AcadSign.Desktop.Models;
using AcadSign.Desktop.Views;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace AcadSign.Desktop.ViewModels;

public partial class SigningViewModel : ObservableObject, INavigationAware
{
    private readonly IApiClientService _apiClient;
    private readonly IDongleService _dongleService;
    private readonly ICertificateValidationService _certificateValidationService;
    private readonly ISignatureService _signatureService;
    private readonly INavigationService _navigationService;
    
    [ObservableProperty]
    private string _documentName = string.Empty;
    
    [ObservableProperty]
    private string _statusMessage = "Initialisation...";
    
    [ObservableProperty]
    private double _progress;
    
    [ObservableProperty]
    private string _progressText = "0%";
    
    [ObservableProperty]
    private bool _canCancel = true;
    
    [ObservableProperty]
    private bool _isCompleted;

    private DocumentDto? _document;
    private string? _pin;
    
    public SigningViewModel(
        IApiClientService apiClient,
        IDongleService dongleService,
        ICertificateValidationService certificateValidationService,
        ISignatureService signatureService,
        INavigationService navigationService)
    {
        _apiClient = apiClient;
        _dongleService = dongleService;
        _certificateValidationService = certificateValidationService;
        _signatureService = signatureService;
        _navigationService = navigationService;
    }
    
    public void OnNavigatedTo(object parameter)
    {
        if (parameter is DocumentDto document)
        {
            _document = document;
            DocumentName = $"{document.DocumentType} - {document.StudentName}";
            _ = StartSigningAsync();
        }
    }
    
    private async Task StartSigningAsync()
    {
        try
        {
            if (_document == null)
            {
                throw new InvalidOperationException("Document manquant");
            }

            StatusMessage = "Saisie du PIN...";
            Progress = 5;
            ProgressText = "5%";

            _pin = await GetPinFromUserAsync();
            if (string.IsNullOrWhiteSpace(_pin))
            {
                StatusMessage = "Signature annulée (PIN requis)";
                CanCancel = false;
                IsCompleted = true;
                return;
            }

            StatusMessage = "Détection du dongle USB...";
            Progress = 15;
            ProgressText = "15%";

            var connected = await _dongleService.IsDongleConnectedAsync();
            if (!connected)
            {
                throw new InvalidOperationException("Dongle non détecté");
            }

            StatusMessage = "Lecture du certificat...";
            Progress = 25;
            ProgressText = "25%";

            var cert = await _dongleService.GetCertificateAsync(_pin);

            StatusMessage = "Validation du certificat (OCSP/CRL)...";
            Progress = 35;
            ProgressText = "35%";

            var validation = await _certificateValidationService.ValidateCertificateAsync(cert);
            if (validation.Status == CertificateStatus.Revoked)
            {
                throw new InvalidOperationException($"Certificat révoqué - {validation.Message}");
            }
            if (validation.Status == CertificateStatus.Expired)
            {
                throw new InvalidOperationException($"Certificat expiré - {validation.Message}");
            }

            StatusMessage = "Téléchargement du document...";
            Progress = 50;
            ProgressText = "50%";

            var unsignedPdf = await _apiClient.DownloadDocumentAsync(_document.Id);

            StatusMessage = "Signature en cours...";
            Progress = 75;
            ProgressText = "75%";

            var signedPdf = await _signatureService.SignPdfAsync(unsignedPdf, _pin);

            StatusMessage = "Upload du document signé...";
            Progress = 90;
            ProgressText = "90%";

            await _apiClient.UploadSignedDocumentAsync(_document.Id, signedPdf);

            StatusMessage = "Signature terminée avec succès!";
            Progress = 100;
            ProgressText = "100%";

            CanCancel = false;
            IsCompleted = true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur: {ex.Message}";
            CanCancel = false;
            IsCompleted = true;
        }
    }

    private Task<string?> GetPinFromUserAsync()
    {
        var tcs = new TaskCompletionSource<string?>();

        Application.Current.Dispatcher.Invoke(() =>
        {
            var dialog = new PinDialog
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var ok = dialog.ShowDialog();
            if (ok == true)
            {
                tcs.SetResult(dialog.Pin);
            }
            else
            {
                tcs.SetResult(null);
            }
        });

        return tcs.Task;
    }
    
    [RelayCommand]
    private void Cancel()
    {
        _navigationService.NavigateTo<MainViewModel>();
    }
    
    [RelayCommand]
    private void GoBack()
    {
        _navigationService.NavigateTo<MainViewModel>();
    }
}
