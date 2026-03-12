using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AcadSign.Desktop.Services.Authentication;

namespace AcadSign.Desktop.Controls;

public partial class LoginControl : UserControl, INotifyPropertyChanged
{
    private readonly IAuthenticationService _authService;
    private bool _isLoggingIn;
    private bool _hasError;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<LoginSuccessEventArgs>? LoginSuccess;

    public LoginControl()
    {
        InitializeComponent();
        DataContext = this;
        LoadSavedLoginPreferences();
        
        // Pour le design-time, créer un service mock
        _authService = null!;
    }

    public LoginControl(IAuthenticationService authService)
    {
        InitializeComponent();
        DataContext = this;
        _authService = authService;
        LoadSavedLoginPreferences();
    }

    public bool IsLoggingIn
    {
        get => _isLoggingIn;
        set
        {
            _isLoggingIn = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotLoggingIn));
        }
    }

    public bool IsNotLoggingIn => !IsLoggingIn;

    public bool HasError
    {
        get => _hasError;
        set
        {
            _hasError = value;
            OnPropertyChanged();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        await SubmitLoginAsync();
    }

    private async Task SubmitLoginAsync()
    {
        if (IsLoggingIn)
        {
            return;
        }

        var username = UsernameTextBox.Text?.Trim();
        var password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(username))
        {
            ShowError("Veuillez entrer votre nom d'utilisateur.");
            UsernameTextBox.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ShowError("Veuillez entrer votre mot de passe.");
            PasswordBox.Focus();
            return;
        }

        await PerformLoginAsync(username, password, RememberMeCheckBox.IsChecked == true);
    }

    private async Task PerformLoginAsync(string username, string password, bool rememberMe)
    {
        try
        {
            IsLoggingIn = true;
            HasError = false;
            ErrorBorder.Visibility = Visibility.Collapsed;
            StatusMessage = "Connexion en cours...";

            // Simuler un délai de connexion pour l'UX
            await Task.Delay(500);

            // Tentative de connexion
            var result = await _authService.LoginAsync(username, password);

            if (result.IsSuccess)
            {
                SaveLoginPreferences(username, rememberMe);
                StatusMessage = "Connexion réussie !";
                await Task.Delay(300);

                // Déclencher l'événement de succès
                LoginSuccess?.Invoke(this, new LoginSuccessEventArgs(username, result.AccessToken, result.RefreshToken, result.ExpiresAt, rememberMe));
            }
            else
            {
                ShowError(result.ErrorMessage ?? "Échec de la connexion. Veuillez vérifier vos identifiants.");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur de connexion : {ex.Message}");
        }
        finally
        {
            IsLoggingIn = false;
            StatusMessage = string.Empty;
        }
    }

    private void ShowError(string message)
    {
        ErrorMessage = message;
        HasError = true;
        ErrorTextBlock.Text = message;
        ErrorBorder.Visibility = Visibility.Visible;
    }

    private void LoadSavedLoginPreferences()
    {
        var settings = AcadSign.Desktop.Properties.Settings.Default;
        RememberMeCheckBox.IsChecked = settings.RememberMe;
        UsernameTextBox.Text = settings.RememberedUsername ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(UsernameTextBox.Text))
        {
            PasswordBox.Focus();
            return;
        }

        UsernameTextBox.Focus();
    }

    private static void SaveLoginPreferences(string username, bool rememberMe)
    {
        var settings = AcadSign.Desktop.Properties.Settings.Default;
        settings.RememberMe = rememberMe;
        settings.RememberedUsername = rememberMe ? username : string.Empty;
        settings.Save();
    }

    private async void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        e.Handled = true;
        await SubmitLoginAsync();
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class LoginSuccessEventArgs : EventArgs
{
    public string Username { get; }
    public string AccessToken { get; }
    public string RefreshToken { get; }
    public DateTime ExpiresAt { get; }
    public bool RememberMe { get; }

    public LoginSuccessEventArgs(string username, string accessToken, string refreshToken, DateTime expiresAt, bool rememberMe)
    {
        Username = username;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresAt = expiresAt;
        RememberMe = rememberMe;
    }
}
