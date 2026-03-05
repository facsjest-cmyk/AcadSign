using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AcadSign.Desktop.Services.Navigation;
using AcadSign.Desktop.Services.Signature;
using AcadSign.Desktop.Models;

namespace AcadSign.Desktop.ViewModels;

public partial class SigningViewModel : ObservableObject, INavigationAware
{
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
    
    public SigningViewModel(
        ISignatureService signatureService,
        INavigationService navigationService)
    {
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
            StatusMessage = "Détection du dongle USB...";
            Progress = 10;
            ProgressText = "10%";
            await Task.Delay(500);
            
            StatusMessage = "Lecture du certificat...";
            Progress = 30;
            ProgressText = "30%";
            await Task.Delay(500);
            
            StatusMessage = "Téléchargement du document...";
            Progress = 50;
            ProgressText = "50%";
            await Task.Delay(500);
            
            StatusMessage = "Signature en cours...";
            Progress = 70;
            ProgressText = "70%";
            
            if (_document != null)
            {
                await _signatureService.SignDocumentAsync(_document.Id);
            }
            
            StatusMessage = "Upload du document signé...";
            Progress = 90;
            ProgressText = "90%";
            await Task.Delay(500);
            
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
