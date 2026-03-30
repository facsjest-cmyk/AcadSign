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
        // Utiliser HTTP vers le backend FSJEST
        _httpClient = new HttpClient();

        // Forcer l'URL de base de l'API FSJEST (pas de port 18080)
        var endpoint = "http://10.2.22.210";
        _apiBaseUrl = $"{endpoint.TrimEnd('/')}/api/v1";
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
            // Appeler le nouvel endpoint FSJEST pour l'authentification
            // POST /api/v1/auth/token
            var loginRequest = new TokenRequest
            {
                Email = username,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync($"{_apiBaseUrl}/auth/token", loginRequest);

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

            // Lire le JSON brut et extraire data.accessToken / data.expiresIn
            var json = await response.Content.ReadAsStringAsync();
            string? accessToken = null;
            int expiresIn = 0;

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.TryGetProperty("data", out var dataElement))
                {
                    if (dataElement.TryGetProperty("accessToken", out var accessTokenProp))
                    {
                        accessToken = accessTokenProp.GetString();
                    }

                    if (dataElement.TryGetProperty("expiresIn", out var expiresInProp)
                        && expiresInProp.TryGetInt32(out var exp))
                    {
                        expiresIn = exp;
                    }
                }
            }
            catch (JsonException)
            {
                // On traitera comme une réponse invalide plus bas
            }

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Réponse de connexion invalide."
                };
            }

            // Retourner le résultat de connexion réussie avec le token JWT
            return new AuthenticationResult
            {
                IsSuccess = true,
                AccessToken = accessToken,
                RefreshToken = string.Empty,
                ExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn > 0 ? expiresIn : 7200)
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

    // DTOs pour la nouvelle API de token FSJEST
    private class TokenRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    private class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
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
