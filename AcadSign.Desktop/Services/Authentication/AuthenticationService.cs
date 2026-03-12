using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace AcadSign.Desktop.Services.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;

    public AuthenticationService()
    {
        // Utiliser HTTP au lieu de HTTPS pour éviter les problèmes de certificat
        _httpClient = new HttpClient();
        _apiBaseUrl = $"{AcadSign.Desktop.Properties.Settings.Default.ApiEndpoint.TrimEnd('/')}/api/v1";
    }

    public async Task<AuthenticationResult> LoginAsync(string username, string password)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return new AuthenticationResult
            {
                IsSuccess = false,
                ErrorMessage = "Nom d'utilisateur et mot de passe requis."
            };
        }

        try
        {
            // Appeler l'API backend pour l'authentification
            var loginRequest = new { Username = username, Password = password };
            var response = await _httpClient.PostAsJsonAsync($"{_apiBaseUrl}/auth/login", loginRequest);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Nom d'utilisateur ou mot de passe incorrect."
                };
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorMessage = await ReadErrorMessageAsync(response, "Requête de connexion invalide.");

                return new AuthenticationResult
                {
                    IsSuccess = false,
                    ErrorMessage = errorMessage
                };
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await ReadErrorMessageAsync(response, $"Erreur serveur lors de la connexion ({(int)response.StatusCode}).");

                return new AuthenticationResult
                {
                    IsSuccess = false,
                    ErrorMessage = errorMessage
                };
            }

            var loginResponse = await response.Content.ReadFromJsonAsync<ApiLoginResponse>();

            if (loginResponse == null || !loginResponse.Success)
            {
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    ErrorMessage = loginResponse?.ErrorMessage ?? "Erreur lors de la connexion."
                };
            }

            // Retourner le résultat de connexion réussie avec le token JWT
            return new AuthenticationResult
            {
                IsSuccess = true,
                AccessToken = loginResponse.AccessToken ?? string.Empty,
                RefreshToken = loginResponse.RefreshToken ?? string.Empty,
                ExpiresAt = loginResponse.ExpiresAt ?? DateTime.UtcNow.AddHours(8)
            };
        }
        catch (HttpRequestException ex)
        {
            return new AuthenticationResult
            {
                IsSuccess = false,
                ErrorMessage = $"Impossible de se connecter au serveur : {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new AuthenticationResult
            {
                IsSuccess = false,
                ErrorMessage = $"Erreur lors de la connexion : {ex.Message}"
            };
        }
    }

    // Classe pour désérialiser la réponse de l'API
    private class ApiLoginResponse
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    private class ApiErrorResponse
    {
        public string? ErrorMessage { get; set; }
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response, string fallbackMessage)
    {
        var responseContent = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(responseContent))
        {
            return fallbackMessage;
        }

        try
        {
            var errorResponse = JsonSerializer.Deserialize<ApiErrorResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (!string.IsNullOrWhiteSpace(errorResponse?.ErrorMessage))
            {
                return errorResponse.ErrorMessage;
            }
        }
        catch (JsonException)
        {
        }

        return responseContent.Trim();
    }
    
    public async Task LogoutAsync()
    {
        await Task.Delay(100);
        // Ici on pourrait invalider le token
    }
    
    public async Task<string> RefreshTokenAsync(string refreshToken)
    {
        await Task.Delay(500);
        return "new_access_token";
    }
}
