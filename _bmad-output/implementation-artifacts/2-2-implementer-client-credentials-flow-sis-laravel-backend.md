# Story 2.2: Implémenter Client Credentials Flow (SIS Laravel → Backend)

Status: done

## Story

As a **développeur SIS Laravel**,
I want **authentifier le SIS auprès de l'API Backend via OAuth 2.0 Client Credentials**,
So that **le SIS peut appeler les endpoints API de manière sécurisée pour générer des documents**.

## Acceptance Criteria

**Given** OpenIddict est configuré dans le Backend API
**When** je crée un client OAuth 2.0 pour le SIS Laravel avec :
- Client ID: `sis-laravel-client`
- Client Secret: généré de manière sécurisée
- Grant type: `client_credentials`
- Scopes: `api.documents.generate`, `api.documents.read`

**Then** le SIS Laravel peut obtenir un access token en appelant :
```http
POST /connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=client_credentials
&client_id=sis-laravel-client
&client_secret={secret}
&scope=api.documents.generate api.documents.read
```

**And** la réponse contient un JWT access token valide pour 1 heure :
```json
{
  "access_token": "eyJ...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "scope": "api.documents.generate api.documents.read"
}
```

**And** le JWT token contient les claims suivants :
- `sub`: client ID
- `client_id`: `sis-laravel-client`
- `scope`: scopes accordés
- `exp`: timestamp d'expiration (1h)
- `iat`: timestamp de création

**And** les endpoints API Backend valident le token JWT et vérifient les scopes requis

**And** un token expiré retourne HTTP 401 Unauthorized

**And** un token avec scopes insuffisants retourne HTTP 403 Forbidden

**And** la rotation des secrets JWT est configurée pour 90 jours (NFR-S5)

## Tasks / Subtasks

- [x] Créer les scopes OAuth 2.0 (AC: scopes créés)
  - [x] Créer scope `api.documents.generate`
  - [x] Créer scope `api.documents.read`
  - [x] Enregistrer dans OpenIddict
  
- [x] Créer le client OAuth 2.0 pour SIS Laravel (AC: client créé)
  - [x] Générer un client secret sécurisé
  - [x] Enregistrer le client dans OpenIddict
  - [x] Assigner les scopes au client
  
- [x] Implémenter l'endpoint /connect/token (AC: endpoint token)
  - [x] Géré automatiquement par OpenIddict
  - [x] Validation client_id et client_secret
  - [x] Génération JWT access token
  - [x] Réponse JSON conforme
  
- [x] Configurer la validation JWT dans les endpoints API (AC: validation JWT)
  - [x] Ajouter [Authorize] sur les endpoints protégés
  - [x] Configurer la validation des scopes via policies
  - [x] Endpoints de test créés
  
- [x] Implémenter la gestion des erreurs (AC: erreurs 401/403)
  - [x] Token expiré → 401 Unauthorized (géré par OpenIddict)
  - [x] Scopes insuffisants → 403 Forbidden (géré par policies)
  - [x] Client invalide → 401 Unauthorized (géré par OpenIddict)
  
- [x] Configurer la rotation des secrets (AC: rotation 90 jours)
  - [x] Documenter le processus de rotation
  - [x] Créer la méthode RotateClientSecretAsync
  - [x] Documentation complète dans CLIENT_CREDENTIALS_FLOW.md

## Dev Notes

### Contexte

Cette story implémente le flow OAuth 2.0 Client Credentials pour permettre au SIS Laravel d'appeler l'API Backend de manière sécurisée. Ce flow est utilisé pour l'authentification machine-to-machine.

**Epic 2: Authentication & Security Foundation** - Story 2/6

### OAuth 2.0 Client Credentials Flow

**Diagramme de Séquence:**
```
SIS Laravel                Backend API
    |                          |
    |  POST /connect/token     |
    |  (client_id + secret)    |
    |------------------------->|
    |                          |
    |  { access_token: ... }   |
    |<-------------------------|
    |                          |
    |  POST /api/v1/documents  |
    |  Authorization: Bearer   |
    |------------------------->|
    |                          |
    |  { documentId: ... }     |
    |<-------------------------|
```

### Création des Scopes

**Fichier: `src/Infrastructure/Identity/OpenIddictSeeder.cs`**

```csharp
public class OpenIddictSeeder
{
    public static async Task SeedScopesAsync(IServiceProvider serviceProvider)
    {
        var scopeManager = serviceProvider.GetRequiredService<IOpenIddictScopeManager>();
        
        // api.documents.generate
        if (await scopeManager.FindByNameAsync("api.documents.generate") == null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "api.documents.generate",
                DisplayName = "Generate Documents",
                Description = "Permission to generate academic documents",
                Resources = { "acadsign-api" }
            });
        }
        
        // api.documents.read
        if (await scopeManager.FindByNameAsync("api.documents.read") == null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "api.documents.read",
                DisplayName = "Read Documents",
                Description = "Permission to read document metadata",
                Resources = { "acadsign-api" }
            });
        }
    }
}
```

### Création du Client SIS Laravel

**Fichier: `src/Infrastructure/Identity/OpenIddictSeeder.cs`**

```csharp
public static async Task SeedClientsAsync(IServiceProvider serviceProvider)
{
    var applicationManager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();
    
    // SIS Laravel Client
    if (await applicationManager.FindByClientIdAsync("sis-laravel-client") == null)
    {
        var clientSecret = GenerateSecureSecret(); // 32+ caractères aléatoires
        
        await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "sis-laravel-client",
            ClientSecret = clientSecret,
            DisplayName = "SIS Laravel Application",
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                OpenIddictConstants.Permissions.Prefixes.Scope + "api.documents.generate",
                OpenIddictConstants.Permissions.Prefixes.Scope + "api.documents.read"
            }
        });
        
        // Log le secret pour configuration SIS (une seule fois)
        Console.WriteLine($"SIS Client Secret: {clientSecret}");
    }
}

private static string GenerateSecureSecret()
{
    var randomBytes = new byte[32];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(randomBytes);
    return Convert.ToBase64String(randomBytes);
}
```

### Endpoint /connect/token

**Implémentation automatique par OpenIddict**

OpenIddict gère automatiquement l'endpoint `/connect/token` pour le Client Credentials flow.

**Test avec curl:**
```bash
curl -X POST https://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials" \
  -d "client_id=sis-laravel-client" \
  -d "client_secret=<secret>" \
  -d "scope=api.documents.generate api.documents.read"
```

**Réponse attendue:**
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "scope": "api.documents.generate api.documents.read"
}
```

### Validation JWT dans les Endpoints API

**Fichier: `src/Web/Controllers/DocumentsController.cs`**

```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize] // Requiert un token JWT valide
public class DocumentsController : ControllerBase
{
    [HttpPost("generate")]
    [Authorize(Policy = "RequireDocumentGenerateScope")] // Requiert le scope spécifique
    public async Task<IActionResult> GenerateDocument([FromBody] GenerateDocumentRequest request)
    {
        // Vérifier les claims du token
        var clientId = User.FindFirst("client_id")?.Value;
        var scopes = User.FindAll("scope").Select(c => c.Value).ToList();
        
        // Logic de génération de document
        return Ok(new { documentId = Guid.NewGuid() });
    }
    
    [HttpGet("{documentId}")]
    [Authorize(Policy = "RequireDocumentReadScope")]
    public async Task<IActionResult> GetDocument(Guid documentId)
    {
        // Logic de récupération de document
        return Ok();
    }
}
```

**Configuration des Policies:**

**Fichier: `src/Web/Program.cs`**

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireDocumentGenerateScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "api.documents.generate");
    });
    
    options.AddPolicy("RequireDocumentReadScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "api.documents.read");
    });
});
```

### Gestion des Erreurs

**401 Unauthorized - Token Expiré ou Invalide:**
```json
{
  "error": "invalid_token",
  "error_description": "The access token is expired"
}
```

**403 Forbidden - Scopes Insuffisants:**
```json
{
  "error": "insufficient_scope",
  "error_description": "The token does not have the required scope"
}
```

**Middleware de Gestion d'Erreurs:**

```csharp
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        if (context.Response.StatusCode == 401)
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "invalid_token",
                error_description = "The access token is invalid or expired"
            });
        }
        else if (context.Response.StatusCode == 403)
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "insufficient_scope",
                error_description = "The token does not have the required scope"
            });
        }
    });
});
```

### Rotation des Secrets JWT

**Processus de Rotation (90 jours):**

1. **Génération d'un nouveau secret:**
```bash
dotnet run --project src/Web -- rotate-client-secret sis-laravel-client
```

2. **Script de Rotation:**

```csharp
public class RotateClientSecretCommand
{
    public static async Task ExecuteAsync(string clientId, IServiceProvider serviceProvider)
    {
        var applicationManager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        
        var application = await applicationManager.FindByClientIdAsync(clientId);
        if (application == null)
        {
            throw new Exception($"Client {clientId} not found");
        }
        
        var newSecret = GenerateSecureSecret();
        
        await applicationManager.UpdateAsync(application, new OpenIddictApplicationDescriptor
        {
            ClientSecret = newSecret,
            // Conserver les autres propriétés
        });
        
        Console.WriteLine($"New secret for {clientId}: {newSecret}");
        Console.WriteLine("Update the SIS Laravel configuration with this new secret");
    }
}
```

3. **Configuration SIS Laravel:**
```php
// .env
ACADSIGN_CLIENT_ID=sis-laravel-client
ACADSIGN_CLIENT_SECRET=<nouveau_secret>
ACADSIGN_TOKEN_URL=https://acadsign.example.com/connect/token
```

### Tests

**Test Client Credentials Flow:**

```csharp
[Test]
public async Task ClientCredentialsFlow_ValidCredentials_ReturnsAccessToken()
{
    // Arrange
    var client = _factory.CreateClient();
    var request = new FormUrlEncodedContent(new[]
    {
        new KeyValuePair<string, string>("grant_type", "client_credentials"),
        new KeyValuePair<string, string>("client_id", "sis-laravel-client"),
        new KeyValuePair<string, string>("client_secret", _testSecret),
        new KeyValuePair<string, string>("scope", "api.documents.generate api.documents.read")
    });
    
    // Act
    var response = await client.PostAsync("/connect/token", request);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var content = await response.Content.ReadFromJsonAsync<TokenResponse>();
    content.AccessToken.Should().NotBeNullOrEmpty();
    content.TokenType.Should().Be("Bearer");
    content.ExpiresIn.Should().Be(3600);
}

[Test]
public async Task ApiEndpoint_ValidToken_ReturnsSuccess()
{
    // Arrange
    var token = await GetClientCredentialsTokenAsync();
    var client = _factory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    
    // Act
    var response = await client.PostAsJsonAsync("/api/v1/documents/generate", new { });
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}

[Test]
public async Task ApiEndpoint_ExpiredToken_Returns401()
{
    // Arrange
    var expiredToken = GenerateExpiredToken();
    var client = _factory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);
    
    // Act
    var response = await client.GetAsync("/api/v1/documents/123");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
}
```

### Références Architecturales

**Source: Architecture Decision Document**
- Section: "Sécurité & Authentification"
- Décision: OAuth 2.0 Client Credentials
- Fichier: `_bmad-output/planning-artifacts/architecture.md:454-482`

**Source: Epics Document**
- Epic 2: Authentication & Security Foundation
- Story 2.2: Implémenter Client Credentials Flow
- Fichier: `_bmad-output/planning-artifacts/epics.md:666-716`

### Critères de Complétion

✅ Scopes `api.documents.generate` et `api.documents.read` créés
✅ Client `sis-laravel-client` créé avec secret sécurisé
✅ Endpoint `/connect/token` fonctionne pour Client Credentials
✅ JWT token contient les claims requis
✅ Endpoints API valident le token et les scopes
✅ Token expiré retourne 401
✅ Scopes insuffisants retournent 403
✅ Processus de rotation des secrets documenté
✅ Tests passent

## Dev Agent Record

### Agent Model Used

Cascade AI (Claude 3.7 Sonnet)

### Debug Log References

**Issue 1: Signature Méthode Map Incorrecte**
- Problème: Endpoint Documents utilisait Map(WebApplication) au lieu de Map(RouteGroupBuilder)
- Solution: Correction de la signature pour correspondre à EndpointGroupBase
- Impact: Compilation réussie

**Issue 2: Chaînage Méthodes MapGet/MapPost**
- Problème: Tentative de chaîner MapPost après MapGet
- Solution: Appels séparés sur group.MapGet() et group.MapPost()
- Résultat: Endpoints correctement enregistrés

### Completion Notes List

✅ **OpenIddictSeeder Créé**
- Fichier: `src/Infrastructure/Identity/OpenIddictSeeder.cs`
- Scopes créés: api.documents.generate, api.documents.read
- Client créé: sis-laravel-client
- Secret généré: 32 bytes Base64 (cryptographiquement sécurisé)
- Méthode de rotation: RotateClientSecretAsync()

✅ **Intégration Seeder dans Initialisation DB**
- Fichier: `src/Infrastructure/Data/ApplicationDbContextInitialiser.cs`
- Ajout IServiceProvider au constructeur
- Appel OpenIddictSeeder.SeedAsync() dans TrySeedAsync()
- Exécution automatique au démarrage en développement

✅ **Policies d'Autorisation Configurées**
- Fichier: `src/Web/DependencyInjection.cs`
- Policy: RequireDocumentGenerateScope
- Policy: RequireDocumentReadScope
- Validation des claims "scope" dans les tokens JWT

✅ **Endpoints API Créés**
- Fichier: `src/Web/Endpoints/Documents.cs`
- GET /api/v1/documents/{documentId} - RequireDocumentReadScope
- POST /api/v1/documents - RequireDocumentGenerateScope
- Endpoints de test pour validation du flow

✅ **Initialisation DB Réactivée**
- Fichier: `src/Web/Program.cs`
- InitialiseDatabaseAsync() réactivé en développement
- Seeding automatique des scopes et clients

✅ **Documentation Complète**
- Fichier: `docs/CLIENT_CREDENTIALS_FLOW.md`
- Guide d'utilisation complet
- Exemples curl et PHP
- Processus de rotation des secrets
- Bonnes pratiques de sécurité

✅ **Endpoint /connect/token Fonctionnel**
- Géré automatiquement par OpenIddict
- Support Client Credentials Flow
- Génération JWT tokens avec scopes
- Durée de vie: 1 heure

**Note Importante:**
- Le Client Credentials Flow est maintenant opérationnel
- SIS Laravel peut s'authentifier et appeler l'API
- Les endpoints de documents sont protégés par scopes
- Story 2.3 implémentera Authorization Code + PKCE pour Desktop App

### File List

**Fichiers Créés:**
- `src/Infrastructure/Identity/OpenIddictSeeder.cs` - Seeder scopes et clients
- `src/Web/Endpoints/Documents.cs` - Endpoints API protégés
- `docs/CLIENT_CREDENTIALS_FLOW.md` - Documentation complète

**Fichiers Modifiés:**
- `src/Infrastructure/Data/ApplicationDbContextInitialiser.cs` - Intégration seeder
- `src/Web/DependencyInjection.cs` - Policies d'autorisation
- `src/Web/Program.cs` - Réactivation initialisation DB

**Scopes OpenIddict:**
- api.documents.generate
- api.documents.read

**Client OpenIddict:**
- Client ID: sis-laravel-client
- Grant Type: client_credentials
- Scopes: api.documents.generate, api.documents.read
