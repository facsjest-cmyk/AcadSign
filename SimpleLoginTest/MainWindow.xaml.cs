using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Http;
using System.Net.Http.Json;

namespace SimpleLoginTest;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly HttpClient _httpClient = new();

    public MainWindow()
    {
        InitializeComponent();
        StatusText.Text = "Backend: http://localhost:5000";
    }

    private async void LoginBtn_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Visibility = Visibility.Collapsed;
        LoginBtn.IsEnabled = false;
        StatusText.Text = "Connexion en cours...";

        try
        {
            var request = new { Username = UsernameBox.Text, Password = PasswordBox.Password };
            var response = await _httpClient.PostAsJsonAsync("http://localhost:5000/api/auth/login", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result?.Success == true)
                {
                    StatusText.Text = "✅ Connexion réussie !";
                    MessageBox.Show($"Bienvenue {result.User?.Username} !\nToken: {result.AccessToken?.Substring(0, 50)}...", 
                        "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    ShowError(result?.ErrorMessage ?? "Erreur inconnue");
                }
            }
            else
            {
                ShowError("Échec de la connexion");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur: {ex.Message}");
        }
        finally
        {
            LoginBtn.IsEnabled = true;
        }
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
        StatusText.Text = "Erreur";
    }

    private class LoginResponse
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? AccessToken { get; set; }
        public UserInfo? User { get; set; }
    }

    private class UserInfo
    {
        public string? Username { get; set; }
    }
}