using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AcadSign.Desktop.Services.Authentication;
using AcadSign.Desktop.Services.Navigation;
using AcadSign.Desktop.Services.Storage;
using System;
using System.Threading.Tasks;

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
            
            // Note: Ce ViewModel n'est plus utilisé - LoginControl gère maintenant la connexion
            var result = await _authService.LoginAsync("demo", "demo");
            
            if (result.IsSuccess)
            {
                await _tokenStorage.SaveTokensAsync(result.AccessToken, result.RefreshToken);
                
                StatusMessage = "Connexion réussie!";
                
                _navigationService.NavigateTo<MainViewModel>();
            }
            else
            {
                StatusMessage = result.ErrorMessage ?? "Échec de la connexion";
            }
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
