using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AcadSign.Desktop.Services.Authentication;
using AcadSign.Desktop.Services.Navigation;
using AcadSign.Desktop.Services.Storage;

namespace AcadSign.Desktop.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;
    private readonly ITokenStorageService _tokenStorage;
    
    [ObservableProperty]
    private bool _isLoggingIn;
    
    [ObservableProperty]
    private string _statusMessage = string.Empty;
    
    public bool IsNotLoggingIn => !IsLoggingIn;
    
    public LoginViewModel(
        IAuthenticationService authService,
        INavigationService navigationService,
        ITokenStorageService tokenStorage)
    {
        _authService = authService;
        _navigationService = navigationService;
        _tokenStorage = tokenStorage;
    }
    
    [RelayCommand]
    private async Task LoginAsync()
    {
        try
        {
            IsLoggingIn = true;
            StatusMessage = "Connexion en cours...";
            
            var result = await _authService.LoginAsync();
            
            await _tokenStorage.SaveTokensAsync(result.AccessToken, result.RefreshToken);
            
            StatusMessage = "Connexion réussie!";
            
            _navigationService.NavigateTo<MainViewModel>();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsLoggingIn = false;
            OnPropertyChanged(nameof(IsNotLoggingIn));
        }
    }
}
