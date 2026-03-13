using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AcadSign.Desktop.Services.Navigation;
using System.Collections.ObjectModel;

namespace AcadSign.Desktop.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    
    [ObservableProperty]
    private string _apiEndpoint = "http://10.2.22.210:18080";
    
    [ObservableProperty]
    private bool _autoDetectDongle = true;
    
    [ObservableProperty]
    private string _selectedLanguage = "Français";
    
    [ObservableProperty]
    private string _appVersion = "1.0.0";
    
    [ObservableProperty]
    private string _currentUser = "Non connecté";
    
    public ObservableCollection<string> AvailableLanguages { get; }
    
    public SettingsViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        
        AvailableLanguages = new ObservableCollection<string>
        {
            "Français",
            "العربية (Arabe)"
        };
        
        LoadSettings();
    }
    
    private void LoadSettings()
    {
        // TODO: Implémenter la persistance des paramètres
        // Pour l'instant, utiliser les valeurs par défaut
        ApiEndpoint = "http://10.2.22.210:18080";
        AutoDetectDongle = true;
        SelectedLanguage = "Français";
    }
    
    [RelayCommand]
    private void Save()
    {
        // TODO: Implémenter la sauvegarde des paramètres
        // Pour l'instant, juste naviguer vers MainViewModel
        _navigationService.NavigateTo<MainViewModel>();
    }
    
    [RelayCommand]
    private void Cancel()
    {
        _navigationService.NavigateTo<MainViewModel>();
    }
}
