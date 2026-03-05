# Stockage Sécurisé des Tokens OAuth - Guide d'Utilisation

## Vue d'ensemble

Ce document explique comment utiliser le système de stockage sécurisé des tokens OAuth 2.0 dans AcadSign Desktop App en utilisant **Windows Credential Manager**.

Les tokens d'authentification (access_token et refresh_token) sont stockés de manière sécurisée sur le poste de travail de l'utilisateur, permettant une connexion persistante entre les sessions sans compromettre la sécurité.

## Architecture du Stockage

### Composants Principaux

1. **ITokenStorageService** - Interface de stockage des tokens
2. **TokenStorageService** - Implémentation utilisant Windows Credential Manager
3. **TokenValidator** - Validation et extraction des informations JWT
4. **TokenData** - Modèle de données pour les tokens

### Flux de Stockage

```
┌─────────────────┐
│  Login Success  │
│  (OAuth 2.0)    │
└────────┬────────┘
         │ Save Tokens
         ▼
┌─────────────────────────┐
│ TokenStorageService     │
│ (Serialize to JSON)     │
└────────┬────────────────┘
         │ Store Encrypted
         ▼
┌─────────────────────────┐
│ Windows Credential      │
│ Manager (DPAPI)         │
└─────────────────────────┘
```

## Configuration

### 1. Package NuGet

Le package `CredentialManagement` est utilisé pour accéder au Windows Credential Manager:

```xml
<PackageReference Include="CredentialManagement" Version="1.0.2" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.16.0" />
```

### 2. Enregistrement dans DI

Les services doivent être enregistrés dans `App.xaml.cs`:

```csharp
private void ConfigureServices(IServiceCollection services)
{
    // Storage
    services.AddSingleton<ITokenStorageService, TokenStorageService>();
    
    // Authentication
    services.AddSingleton<TokenValidator>();
    
    // ... autres services
}
```

## Utilisation

### Sauvegarder les Tokens après Login

Après une authentification réussie via OAuth 2.0 Authorization Code + PKCE:

```csharp
public class LoginViewModel
{
    private readonly ITokenStorageService _tokenStorage;
    
    private async Task LoginAsync()
    {
        // 1. Authentifier via OAuth 2.0
        var result = await _authService.LoginAsync();
        
        // 2. Sauvegarder les tokens dans Windows Credential Manager
        await _tokenStorage.SaveTokensAsync(
            result.AccessToken, 
            result.RefreshToken
        );
        
        // 3. Naviguer vers la page principale
        NavigateToMainWindow();
    }
}
```

### Charger les Tokens au Démarrage

Au démarrage de l'application, vérifier si des tokens valides existent:

```csharp
public class StartupService
{
    private readonly ITokenStorageService _tokenStorage;
    private readonly TokenValidator _tokenValidator;
    
    public async Task InitializeAsync()
    {
        // 1. Récupérer les tokens stockés
        var (accessToken, refreshToken) = await _tokenStorage.GetTokensAsync();
        
        if (string.IsNullOrEmpty(accessToken) && string.IsNullOrEmpty(refreshToken))
        {
            // Aucun token → afficher login
            ShowLoginWindow();
            return;
        }
        
        // 2. Vérifier si l'access token est valide
        if (!string.IsNullOrEmpty(accessToken) && 
            !_tokenValidator.IsTokenExpired(accessToken))
        {
            // Token valide → utiliser directement
            SetCurrentUser(accessToken);
            ShowMainWindow();
            return;
        }
        
        // 3. Access token expiré → essayer de rafraîchir
        if (!string.IsNullOrEmpty(refreshToken))
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(refreshToken);
                
                // Sauvegarder les nouveaux tokens
                await _tokenStorage.SaveTokensAsync(
                    result.AccessToken, 
                    result.RefreshToken
                );
                
                SetCurrentUser(result.AccessToken);
                ShowMainWindow();
                return;
            }
            catch (RefreshTokenExpiredException)
            {
                // Refresh token expiré → supprimer et afficher login
                await _tokenStorage.DeleteTokensAsync();
            }
        }
        
        // 4. Aucun token valide → afficher login
        ShowLoginWindow();
    }
}
```

### Vérifier l'Expiration d'un Token

Utiliser `TokenValidator` pour vérifier si un token est expiré:

```csharp
var tokenValidator = new TokenValidator();

// Vérifier l'expiration (avec marge de 5 minutes)
bool isExpired = tokenValidator.IsTokenExpired(accessToken);

// Obtenir la date d'expiration
DateTime? expiration = tokenValidator.GetTokenExpiration(accessToken);

// Extraire l'email de l'utilisateur
string? email = tokenValidator.GetUserEmail(accessToken);

// Extraire le nom de l'utilisateur
string? name = tokenValidator.GetUserName(accessToken);

// Extraire le rôle de l'utilisateur
string? role = tokenValidator.GetUserRole(accessToken);
```

### Supprimer les Tokens à la Déconnexion

Lors de la déconnexion, supprimer les tokens du Credential Manager:

```csharp
public class MainViewModel
{
    private readonly ITokenStorageService _tokenStorage;
    
    private async Task LogoutAsync()
    {
        // 1. Supprimer les tokens du Credential Manager
        await _tokenStorage.DeleteTokensAsync();
        
        // 2. Réinitialiser l'état de l'application
        CurrentUser.Clear();
        
        // 3. Naviguer vers la page de login
        NavigateToLoginWindow();
    }
}
```

## Stockage dans Windows Credential Manager

### Détails du Stockage

Les tokens sont stockés avec les paramètres suivants:

| Paramètre | Valeur |
|-----------|--------|
| **Target** | `AcadSign.Desktop.Tokens` |
| **Username** | `AcadSignUser` |
| **Password** | JSON: `{"AccessToken":"...","RefreshToken":"...","SavedAt":"..."}` |
| **Type** | Generic |
| **Persistence** | LocalComputer |

### Chiffrement DPAPI

Windows Credential Manager utilise **DPAPI (Data Protection API)** pour chiffrer automatiquement les credentials:

- ✅ Chiffrement au niveau utilisateur Windows
- ✅ Clés liées au profil utilisateur
- ✅ Protection contre l'accès par d'autres utilisateurs
- ✅ Protection contre l'accès depuis d'autres machines

### Visualiser les Credentials

Pour voir les credentials stockés dans Windows:

1. Ouvrir **Panneau de configuration**
2. Aller dans **Comptes d'utilisateurs**
3. Cliquer sur **Gestionnaire d'identification**
4. Sélectionner **Informations d'identification Windows**
5. Chercher `AcadSign.Desktop.Tokens`

## Sécurité

### Bonnes Pratiques

#### ✅ À Faire

- Toujours utiliser `TokenStorageService` pour stocker les tokens
- Vérifier l'expiration avant d'utiliser un access token
- Rafraîchir automatiquement les tokens expirés
- Supprimer les tokens lors de la déconnexion
- Ne jamais logger les tokens en clair
- Utiliser HTTPS pour toutes les communications API

#### ❌ À Éviter

- Ne jamais stocker les tokens dans des fichiers texte
- Ne jamais logger les tokens dans les logs
- Ne jamais afficher les tokens dans l'UI
- Ne jamais partager les tokens entre utilisateurs
- Ne jamais stocker les tokens dans le code source
- Ne jamais envoyer les tokens par email ou chat

### Protection des Données Sensibles

Les tokens ne doivent **jamais** apparaître dans:

- ❌ Logs de l'application
- ❌ Messages d'erreur affichés à l'utilisateur
- ❌ Fichiers de configuration
- ❌ Variables d'environnement
- ❌ Clipboard (presse-papiers)

### Filtre de Données Sensibles

Implémenter un filtre pour les logs:

```csharp
public class SensitiveDataFilter
{
    private static readonly string[] SensitiveKeys = 
    {
        "access_token",
        "refresh_token",
        "password",
        "client_secret"
    };
    
    public static string FilterSensitiveData(string logMessage)
    {
        foreach (var key in SensitiveKeys)
        {
            if (logMessage.Contains(key, StringComparison.OrdinalIgnoreCase))
            {
                var pattern = $@"{key}[""']?\s*[:=]\s*[""']?([^""',\s}}]+)";
                logMessage = Regex.Replace(
                    logMessage, 
                    pattern, 
                    $"{key}: [REDACTED]"
                );
            }
        }
        
        return logMessage;
    }
}
```

## Refresh Token Automatique

### Implémentation du Refresh

Lorsque l'access token est expiré mais le refresh token est valide:

```csharp
public class AuthenticationService
{
    public async Task<AuthenticationResult> RefreshTokenAsync(string refreshToken)
    {
        using var httpClient = new HttpClient();
        
        var request = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("client_id", "acadsign-desktop"),
            new KeyValuePair<string, string>("refresh_token", refreshToken)
        });
        
        var response = await httpClient.PostAsync(
            $"{_apiBaseUrl}/connect/token", 
            request
        );
        
        if (!response.IsSuccessStatusCode)
        {
            throw new RefreshTokenExpiredException(
                "Refresh token is invalid or expired"
            );
        }
        
        var tokenResponse = await response.Content
            .ReadFromJsonAsync<TokenResponse>();
        
        return new AuthenticationResult
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            ExpiresIn = tokenResponse.ExpiresIn
        };
    }
}
```

### Gestion des Erreurs

Gérer les différents cas d'erreur:

```csharp
try
{
    var result = await _authService.RefreshTokenAsync(refreshToken);
    await _tokenStorage.SaveTokensAsync(result.AccessToken, result.RefreshToken);
}
catch (RefreshTokenExpiredException)
{
    // Refresh token expiré → supprimer et demander login
    await _tokenStorage.DeleteTokensAsync();
    ShowLoginWindow();
}
catch (HttpRequestException ex)
{
    // Erreur réseau → réessayer plus tard
    _logger.LogError(ex, "Network error during token refresh");
    ShowErrorMessage("Erreur de connexion. Veuillez réessayer.");
}
catch (Exception ex)
{
    // Erreur inattendue → logger et afficher login
    _logger.LogError(ex, "Unexpected error during token refresh");
    await _tokenStorage.DeleteTokensAsync();
    ShowLoginWindow();
}
```

## Exemples Complets

### Exemple 1: Login et Sauvegarde

```csharp
public class LoginViewModel : ObservableObject
{
    private readonly IAuthenticationService _authService;
    private readonly ITokenStorageService _tokenStorage;
    
    [RelayCommand]
    private async Task LoginAsync()
    {
        try
        {
            IsLoggingIn = true;
            StatusMessage = "Connexion en cours...";
            
            // Authentifier
            var result = await _authService.LoginAsync();
            
            // Sauvegarder les tokens
            await _tokenStorage.SaveTokensAsync(
                result.AccessToken, 
                result.RefreshToken
            );
            
            StatusMessage = "Connexion réussie!";
            NavigateToMainWindow();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsLoggingIn = false;
        }
    }
}
```

### Exemple 2: Vérification au Démarrage

```csharp
public class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        var tokenStorage = ServiceProvider.GetRequiredService<ITokenStorageService>();
        var tokenValidator = ServiceProvider.GetRequiredService<TokenValidator>();
        
        var (accessToken, refreshToken) = await tokenStorage.GetTokensAsync();
        
        if (!string.IsNullOrEmpty(accessToken) && 
            !tokenValidator.IsTokenExpired(accessToken))
        {
            // Token valide → ouvrir MainWindow
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        else
        {
            // Token invalide → ouvrir LoginWindow
            var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }
    }
}
```

### Exemple 3: Déconnexion

```csharp
public class MainViewModel : ObservableObject
{
    private readonly ITokenStorageService _tokenStorage;
    private readonly INavigationService _navigationService;
    
    [RelayCommand]
    private async Task LogoutAsync()
    {
        try
        {
            // Supprimer les tokens
            await _tokenStorage.DeleteTokensAsync();
            
            // Réinitialiser l'état
            CurrentUser.Clear();
            
            // Naviguer vers login
            _navigationService.NavigateTo<LoginViewModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
        }
    }
}
```

## Dépannage

### Problème: Tokens non persistants après redémarrage

**Symptôme:** Les tokens sont supprimés après redémarrage de l'application.

**Causes possibles:**
- `PersistanceType` est `Session` au lieu de `LocalComputer`
- Credential Manager est désactivé par GPO
- Profil utilisateur Windows corrompu

**Solution:**
```csharp
// Vérifier que PersistanceType est LocalComputer
PersistanceType = PersistanceType.LocalComputer
```

### Problème: Erreur "Failed to save tokens"

**Symptôme:** Exception lors de `SaveTokensAsync()`.

**Causes possibles:**
- Permissions insuffisantes
- Credential Manager non disponible
- Nom de target invalide

**Solution:**
```csharp
try
{
    await _tokenStorage.SaveTokensAsync(accessToken, refreshToken);
}
catch (InvalidOperationException ex)
{
    _logger.LogError(ex, "Failed to save tokens to Credential Manager");
    // Fallback: utiliser un stockage alternatif ou demander login à chaque fois
}
```

### Problème: Token expiré immédiatement

**Symptôme:** Le token est considéré comme expiré juste après le login.

**Causes possibles:**
- Horloge système désynchronisée
- Marge d'expiration trop grande (5 minutes)

**Solution:**
```csharp
// Réduire la marge d'expiration
return now.AddMinutes(1) >= expirationTime; // Au lieu de 5 minutes
```

## Conformité NFR-S6

✅ **NFR-S6: Stockage Sécurisé des Tokens**

- Tokens stockés dans Windows Credential Manager
- Chiffrement automatique via DPAPI
- Persistence LocalComputer (accessible uniquement sur ce PC)
- Tokens jamais loggés ou affichés en clair
- Suppression automatique à la déconnexion
- Refresh automatique des tokens expirés

## Références

### Documentation Technique

- **Windows Credential Manager:** https://docs.microsoft.com/en-us/windows/win32/secauthn/credential-manager
- **DPAPI:** https://docs.microsoft.com/en-us/dotnet/standard/security/how-to-use-data-protection
- **OAuth 2.0 Refresh Token:** https://oauth.net/2/grant-types/refresh-token/

### Architecture AcadSign

- **Architecture Document:** `_bmad-output/planning-artifacts/architecture.md`
- **Story 2.6:** `_bmad-output/implementation-artifacts/2-6-implementer-stockage-securise-tokens-desktop-app-windows-credential-manager.md`

### Fichiers Source

- **Interface:** `Services/Storage/ITokenStorageService.cs`
- **Service:** `Services/Storage/TokenStorageService.cs`
- **Validator:** `Services/Authentication/TokenValidator.cs`
- **Model:** `Models/TokenData.cs`
