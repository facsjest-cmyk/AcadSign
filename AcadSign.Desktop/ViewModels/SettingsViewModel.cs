using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AcadSign.Desktop.Services.Navigation;
using System.Collections.ObjectModel;

namespace AcadSign.Desktop.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    
    [ObservableProperty]
    private string _apiEndpoint = "http://localhost:5000";
    
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
        ApiEndpoint = Properties.Settings.Default.ApiEndpoint ?? "http://localhost:5000";
        AutoDetectDongle = Properties.Settings.Default.AutoDetectDongle;
        SelectedLanguage = Properties.Settings.Default.Language ?? "Français";
    }
    
    [RelayCommand]
    private void Save()
    {
        Properties.Settings.Default.ApiEndpoint = ApiEndpoint;
        Properties.Settings.Default.AutoDetectDongle = AutoDetectDongle;
        Properties.Settings.Default.Language = SelectedLanguage;
        Properties.Settings.Default.Save();
        
        _navigationService.NavigateTo<MainViewModel>();
    }
    
    [RelayCommand]
    private void Cancel()
    {
        _navigationService.NavigateTo<MainViewModel>();
    }
}
