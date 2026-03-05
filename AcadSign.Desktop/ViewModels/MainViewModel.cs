using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AcadSign.Desktop.Services.Api;
using AcadSign.Desktop.Services.Navigation;
using AcadSign.Desktop.Models;
using System.Collections.ObjectModel;

namespace AcadSign.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IApiClientService _apiClient;
    private readonly INavigationService _navigationService;
    
    [ObservableProperty]
    private ObservableCollection<DocumentDto> _documents = new();
    
    [ObservableProperty]
    private DocumentDto? _selectedDocument;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _statusText = "Prêt";
    
    public MainViewModel(
        IApiClientService apiClient,
        INavigationService navigationService)
    {
        _apiClient = apiClient;
        _navigationService = navigationService;
        
        _ = LoadDocumentsAsync();
    }
    
    [RelayCommand]
    private async Task LoadDocumentsAsync()
    {
        try
        {
            IsLoading = true;
            StatusText = "Chargement des documents...";
            
            var docs = await _apiClient.GetPendingDocumentsAsync();
            Documents.Clear();
            foreach (var doc in docs)
            {
                Documents.Add(doc);
            }
            
            StatusText = $"{Documents.Count} document(s) en attente";
        }
        catch (Exception ex)
        {
            StatusText = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private void SignDocument(DocumentDto document)
    {
        _navigationService.NavigateTo<SigningViewModel>(document);
    }
    
    [RelayCommand]
    private void NavigateToSettings()
    {
        _navigationService.NavigateTo<SettingsViewModel>();
    }
    
    [RelayCommand]
    private void Logout()
    {
        _navigationService.NavigateTo<LoginViewModel>();
    }
}