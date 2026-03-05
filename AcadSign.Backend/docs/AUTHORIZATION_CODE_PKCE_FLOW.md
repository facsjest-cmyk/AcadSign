# Authorization Code + PKCE Flow - Guide d'Utilisation

## Vue d'ensemble

Ce document explique comment utiliser le flow OAuth 2.0 Authorization Code + PKCE pour permettre aux utilisateurs de se connecter à la Desktop App AcadSign de manière sécurisée.

## Prérequis

- OpenIddict configuré (Story 2.1)
- PostgreSQL en cours d'exécution
- Backend API démarré
- Desktop App développée (Story 1.2)

## Qu'est-ce que PKCE?

**PKCE (Proof Key for Code Exchange)** est une extension de sécurité pour OAuth 2.0 qui protège contre les attaques d'interception du code d'autorisation. C'est **obligatoire** pour les applications publiques (comme les Desktop Apps) qui ne peuvent pas stocker de secret client de manière sécurisée.

### Fonctionnement de PKCE

1. **Code Verifier:** Chaîne aléatoire générée par le client (43-128 caractères)
2. **Code Challenge:** `BASE64URL(SHA256(code_verifier))`
3. Le client envoie le `code_challenge` lors de la demande d'autorisation
4. Le client envoie le `code_verifier` lors de l'échange du code contre un token
5. Le serveur vérifie que `SHA256(code_verifier) == code_challenge`

## Configuration Backend

### 1. Client Desktop App

Le client Desktop App est automatiquement créé au démarrage de l'application :

```
===========================================
Desktop App Client Created
Client ID: acadsign-desktop
Type: Public Client (no secret)
PKCE: Required
===========================================
```

**Caractéristiques:**
- **Client ID:** `acadsign-desktop`
- **Type:** Public (pas de client secret)
- **Grant Types:** Authorization Code, Refresh Token
- **Redirect URI:** `http://localhost:7890/callback`
- **Scopes:** `openid`, `profile`, `email`, `api.documents.sign`
- **PKCE:** Obligatoire

### 2. Scopes Disponibles

| Scope | Description |
|-------|-------------|
| `openid` | Identifiant OpenID Connect |
| `profile` | Informations de profil utilisateur |
| `email` | Adresse email de l'utilisateur |
| `api.documents.sign` | Permission de signer des documents académiques |

## Flow Complet

### Diagramme de Séquence

```
Desktop App          Navigateur          Backend API
    |                    |                    |
    | 1. Générer PKCE    |                    |
    |   code_verifier    |                    |
    |   code_challenge   |                    |
    |------------------->|                    |
    |                    |                    |
    | 2. Ouvrir navigateur                    |
    |    /connect/authorize                   |
    |------------------->|                    |
    |                    | 3. GET /authorize  |
    |                    |------------------->|
    |                    |                    |
    |                    | 4. Page login      |
    |                    |<-------------------|
    |                    |                    |
    |                    | 5. POST credentials|
    |                    |------------------->|
    |                    |                    |
    |                    | 6. Redirect + code |
    |                    |<-------------------|
    | 7. Callback + code |                    |
    |<-------------------|                    |
    |                    |                    |
    | 8. POST /token (code + verifier)        |
    |---------------------------------------->|
    |                    |                    |
    | 9. access_token + refresh_token         |
    |<----------------------------------------|
```

### Étape 1: Générer PKCE

**Desktop App génère:**

```csharp
// Code Verifier (43-128 caractères)
var randomBytes = new byte[32];
using var rng = RandomNumberGenerator.Create();
rng.GetBytes(randomBytes);
var codeVerifier = Base64UrlEncode(randomBytes);

// Code Challenge = BASE64URL(SHA256(code_verifier))
using var sha256 = SHA256.Create();
var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
var codeChallenge = Base64UrlEncode(challengeBytes);
```

### Étape 2: Demande d'Autorisation

**Desktop App ouvre le navigateur:**

```
https://localhost:5001/connect/authorize?
  response_type=code
  &client_id=acadsign-desktop
  &redirect_uri=http://localhost:7890/callback
  &scope=openid profile email api.documents.sign
  &code_challenge=E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM
  &code_challenge_method=S256
```

### Étape 3: Authentification Utilisateur

L'utilisateur entre ses credentials dans le navigateur:
- **Email:** `fatima@university.edu`
- **Password:** `[password]`

### Étape 4: Redirection avec Code

Après authentification réussie, le Backend redirige vers:

```
http://localhost:7890/callback?code=eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Étape 5: Échange Code → Tokens

**Desktop App appelle /connect/token:**

```http
POST https://localhost:5001/connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=authorization_code
&client_id=acadsign-desktop
&code=eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
&redirect_uri=http://localhost:7890/callback
&code_verifier=dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk
```

**Réponse:**

```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refresh_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "scope": "openid profile email api.documents.sign"
}
```

### Étape 6: Utiliser l'Access Token

**Appeler l'API avec le token:**

```http
POST https://localhost:5001/api/v1/documents/sign
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "documentId": "123e4567-e89b-12d3-a456-426614174000",
  "signature": "..."
}
```

## Refresh Token Flow

Lorsque l'access token expire (après 1 heure), utilisez le refresh token pour obtenir un nouveau token:

**Requête:**

```http
POST https://localhost:5001/connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=refresh_token
&client_id=acadsign-desktop
&refresh_token=eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Réponse:**

```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refresh_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_in": 3600
}
```

**Note:** Le refresh token est valide pendant 7 jours.

## Claims JWT

L'access token contient les claims suivants:

```json
{
  "sub": "550e8400-e29b-41d4-a716-446655440000",
  "email": "fatima@university.edu",
  "name": "Fatima Ahmed",
  "role": "Registrar",
  "institutionId": "univ-001",
  "scope": "openid profile email api.documents.sign",
  "exp": 1709577600,
  "iat": 1709574000
}
```

## Gestion des Erreurs

### 401 Unauthorized

Le token est expiré ou invalide:

```json
{
  "error": "invalid_token",
  "error_description": "The access token is invalid or expired"
}
```

**Solution:** Utilisez le refresh token pour obtenir un nouveau token.

### 403 Forbidden

Le token ne possède pas les scopes requis:

```json
{
  "error": "insufficient_scope",
  "error_description": "The token does not have the required scope"
}
```

**Solution:** Demandez un nouveau token avec les scopes appropriés.

### Refresh Token Expiré

Le refresh token est expiré (après 7 jours):

```json
{
  "error": "invalid_grant",
  "error_description": "The refresh token is invalid or expired"
}
```

**Solution:** L'utilisateur doit se reconnecter via le flow Authorization Code.

## Implémentation Desktop App (.NET)

### Service d'Authentification

```csharp
public class AuthenticationService : IAuthenticationService
{
    private readonly HttpListener _httpListener;
    private string _codeVerifier;
    
    public async Task<AuthenticationResult> LoginAsync()
    {
        // 1. Générer PKCE
        var (codeVerifier, codeChallenge) = GeneratePkce();
        _codeVerifier = codeVerifier;
        
        // 2. Construire l'URL d'autorisation
        var authUrl = $"https://localhost:5001/connect/authorize?" +
            $"response_type=code&" +
            $"client_id=acadsign-desktop&" +
            $"redirect_uri=http://localhost:7890/callback&" +
            $"scope=openid profile email api.documents.sign&" +
            $"code_challenge={codeChallenge}&" +
            $"code_challenge_method=S256";
        
        // 3. Démarrer le listener HTTP local
        _httpListener = new HttpListener();
        _httpListener.Prefixes.Add("http://localhost:7890/");
        _httpListener.Start();
        
        // 4. Ouvrir le navigateur
        Process.Start(new ProcessStartInfo
        {
            FileName = authUrl,
            UseShellExecute = true
        });
        
        // 5. Attendre le callback
        var context = await _httpListener.GetContextAsync();
        var code = context.Request.QueryString["code"];
        
        // 6. Répondre au navigateur
        var response = context.Response;
        var html = "<html><body><h1>Authentification réussie!</h1><p>Vous pouvez fermer cette fenêtre.</p></body></html>";
        var buffer = Encoding.UTF8.GetBytes(html);
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer);
        response.Close();
        _httpListener.Stop();
        
        // 7. Échanger le code contre un token
        return await ExchangeCodeForTokenAsync(code, _codeVerifier);
    }
    
    private (string codeVerifier, string codeChallenge) GeneratePkce()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var codeVerifier = Base64UrlEncode(randomBytes);
        
        using var sha256 = SHA256.Create();
        var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
        var codeChallenge = Base64UrlEncode(challengeBytes);
        
        return (codeVerifier, codeChallenge);
    }
    
    private string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
    
    private async Task<AuthenticationResult> ExchangeCodeForTokenAsync(string code, string codeVerifier)
    {
        using var httpClient = new HttpClient();
        
        var request = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("client_id", "acadsign-desktop"),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", "http://localhost:7890/callback"),
            new KeyValuePair<string, string>("code_verifier", codeVerifier)
        });
        
        var response = await httpClient.PostAsync("https://localhost:5001/connect/token", request);
        response.EnsureSuccessStatusCode();
        
        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
        
        return new AuthenticationResult
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            ExpiresIn = tokenResponse.ExpiresIn
        };
    }
}
```

## Sécurité

### Bonnes Pratiques

1. **Toujours utiliser PKCE** pour les applications publiques
2. **Utiliser HTTPS** en production
3. **Stocker les tokens de manière sécurisée** (voir Story 2.6 - Windows Credential Manager)
4. **Valider le state parameter** pour prévenir les attaques CSRF
5. **Limiter la durée de vie des tokens** (1h pour access token, 7 jours pour refresh token)
6. **Implémenter la rotation des refresh tokens**
7. **Monitorer les tentatives de connexion** pour détecter les abus

### Pourquoi PKCE est Important

Sans PKCE, un attaquant pourrait:
1. Intercepter le code d'autorisation dans la redirection
2. Échanger le code contre un token avant l'application légitime
3. Accéder aux ressources de l'utilisateur

Avec PKCE:
- L'attaquant ne peut pas échanger le code sans le `code_verifier`
- Le `code_verifier` n'est jamais transmis dans l'URL
- Seule l'application qui a généré le `code_challenge` peut échanger le code

## Support

Pour toute question ou problème, consultez:
- Documentation OpenIddict: https://documentation.openiddict.com/
- RFC 7636 (PKCE): https://tools.ietf.org/html/rfc7636
- Architecture AcadSign: `_bmad-output/planning-artifacts/architecture.md`
