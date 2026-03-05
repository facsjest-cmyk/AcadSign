# Role-Based Access Control (RBAC) - Guide d'Utilisation

## Vue d'ensemble

Ce document explique comment utiliser le système RBAC (Role-Based Access Control) dans AcadSign Backend API pour gérer les permissions des utilisateurs.

## Rôles Disponibles

AcadSign implémente 4 rôles principaux:

| Rôle | Description | Permissions |
|------|-------------|-------------|
| **Administrator** | Accès complet au système | Toutes les opérations (gestion templates, users, audit logs, configuration) + permissions Registrar |
| **Registrar** | Personnel du registrariat | Génération et signature de documents |
| **Auditor** | Auditeur | Lecture seule des audit logs |
| **API Client** | Client API (SIS Laravel) | Génération de documents via API |

## Configuration des Rôles

### Constantes de Rôles

Les rôles sont définis dans `Domain/Constants/Roles.cs`:

```csharp
public abstract class Roles
{
    public const string Administrator = nameof(Administrator);
    public const string Registrar = nameof(Registrar);
    public const string Auditor = nameof(Auditor);
    public const string ApiClient = "API Client";
}
```

### Seeding des Rôles

Les rôles sont automatiquement créés au démarrage de l'application via `ApplicationDbContextInitialiser`:

```csharp
var roles = new[]
{
    new IdentityRole(Roles.Administrator),
    new IdentityRole(Roles.Registrar),
    new IdentityRole(Roles.Auditor),
    new IdentityRole(Roles.ApiClient)
};

foreach (var role in roles)
{
    if (_roleManager.Roles.All(r => r.Name != role.Name))
    {
        await _roleManager.CreateAsync(role);
    }
}
```

## Policies d'Autorisation

Les policies d'autorisation sont configurées dans `Web/DependencyInjection.cs`:

```csharp
builder.Services.AddAuthorization(options =>
{
    // Policy pour Admin
    options.AddPolicy("RequireAdminRole", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole(Roles.Administrator);
    });
    
    // Policy pour Registrar (Admin a aussi accès)
    options.AddPolicy("RequireRegistrarRole", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole(Roles.Registrar, Roles.Administrator);
    });
    
    // Policy pour Auditor (Admin a aussi accès)
    options.AddPolicy("RequireAuditorRole", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole(Roles.Auditor, Roles.Administrator);
    });
    
    // Policy pour API Client
    options.AddPolicy("RequireApiClientRole", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole(Roles.ApiClient);
    });
});
```

## Utilisation dans les Endpoints

### Méthode 1: Attribut [Authorize]

```csharp
[Authorize(Roles = "Administrator")]
public async Task<IResult> DeleteTemplate(Guid id)
{
    // Seuls les Administrators peuvent supprimer des templates
    return Results.Ok();
}

[Authorize(Roles = "Registrar,Administrator")]
public async Task<IResult> GenerateDocument([FromBody] GenerateDocumentRequest request)
{
    // Registrar et Administrator peuvent générer des documents
    return Results.Ok();
}
```

### Méthode 2: Policy

```csharp
group.MapPost(GenerateDocument, "generate")
    .RequireAuthorization("RequireRegistrarRole");

group.MapGet(GetAuditLog, "audit/{documentId}")
    .RequireAuthorization("RequireAuditorRole");

group.MapDelete(DeleteTemplate, "templates/{id}")
    .RequireAuthorization("RequireAdminRole");
```

## Gestion des Rôles Utilisateurs

### Endpoints de Gestion des Rôles

**Seuls les Administrators peuvent gérer les rôles.**

#### 1. Assigner un Rôle

```http
POST /api/v1/users/{userId}/roles
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "roleName": "Registrar"
}
```

**Réponse:**
```json
{
  "message": "Role 'Registrar' assigned to user 'fatima@university.edu'"
}
```

#### 2. Retirer un Rôle

```http
DELETE /api/v1/users/{userId}/roles/Registrar
Authorization: Bearer {admin_token}
```

**Réponse:**
```json
{
  "message": "Role 'Registrar' removed from user 'fatima@university.edu'"
}
```

#### 3. Obtenir les Rôles d'un Utilisateur

```http
GET /api/v1/users/{userId}/roles
Authorization: Bearer {admin_token}
```

**Réponse:**
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "userName": "fatima@university.edu",
  "roles": ["Registrar"]
}
```

## Claims JWT

Les rôles sont inclus dans le JWT token sous forme de claims `role`:

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

**Note:** Si un utilisateur a plusieurs rôles, le claim `role` sera un tableau:

```json
{
  "role": ["Registrar", "Auditor"]
}
```

## Matrice de Permissions

### Administrator

| Endpoint | Méthode | Permission |
|----------|---------|------------|
| `/api/v1/templates` | POST/PUT/DELETE | ✅ Autorisé |
| `/api/v1/users/{userId}/roles` | POST/DELETE/GET | ✅ Autorisé |
| `/api/v1/audit/*` | GET | ✅ Autorisé |
| `/api/v1/documents/generate` | POST | ✅ Autorisé |
| `/api/v1/documents/{id}/upload-signed` | POST | ✅ Autorisé |

### Registrar

| Endpoint | Méthode | Permission |
|----------|---------|------------|
| `/api/v1/documents/generate` | POST | ✅ Autorisé |
| `/api/v1/documents/{id}/unsigned` | GET | ✅ Autorisé |
| `/api/v1/documents/{id}/upload-signed` | POST | ✅ Autorisé |
| `/api/v1/documents/{id}` | GET | ✅ Autorisé |
| `/api/v1/documents/{id}/download` | GET | ✅ Autorisé |
| `/api/v1/templates` | POST/PUT/DELETE | ❌ Interdit (403) |
| `/api/v1/audit/*` | GET | ❌ Interdit (403) |

### Auditor

| Endpoint | Méthode | Permission |
|----------|---------|------------|
| `/api/v1/audit/{documentId}` | GET | ✅ Autorisé |
| `/api/v1/audit/search` | GET | ✅ Autorisé |
| `/api/v1/documents/generate` | POST | ❌ Interdit (403) |
| `/api/v1/templates` | POST/PUT/DELETE | ❌ Interdit (403) |

### API Client

| Endpoint | Méthode | Permission |
|----------|---------|------------|
| `/api/v1/documents/generate` | POST | ✅ Autorisé |
| `/api/v1/documents/batch` | POST | ✅ Autorisé |
| `/api/v1/documents/batch/{batchId}/status` | GET | ✅ Autorisé |
| `/api/v1/documents/{id}` | GET | ✅ Autorisé |
| `/api/v1/documents/{id}/upload-signed` | POST | ❌ Interdit (403) |
| `/api/v1/audit/*` | GET | ❌ Interdit (403) |

## Gestion des Erreurs

### 401 Unauthorized

L'utilisateur n'est pas authentifié:

```json
{
  "error": "invalid_token",
  "error_description": "The access token is invalid or expired"
}
```

**Solution:** Obtenir un nouveau token via `/connect/token`.

### 403 Forbidden

L'utilisateur n'a pas le rôle requis:

```json
{
  "error": "forbidden",
  "error_description": "You do not have permission to access this resource",
  "required_role": "Administrator"
}
```

**Solution:** Demander à un Administrator d'assigner le rôle approprié.

## Exemples d'Utilisation

### Exemple 1: Registrar Génère un Document

```bash
# 1. Obtenir un token (Authorization Code + PKCE)
# Voir AUTHORIZATION_CODE_PKCE_FLOW.md

# 2. Générer un document
curl -X POST https://localhost:5001/api/v1/documents/generate \
  -H "Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "studentId": "123456",
    "documentType": "Transcript",
    "language": "en"
  }'
```

### Exemple 2: Administrator Assigne un Rôle

```bash
# 1. Obtenir un token Administrator
# ...

# 2. Assigner le rôle Registrar à un utilisateur
curl -X POST https://localhost:5001/api/v1/users/550e8400-e29b-41d4-a716-446655440000/roles \
  -H "Authorization: Bearer {admin_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "roleName": "Registrar"
  }'
```

### Exemple 3: Auditor Consulte les Logs

```bash
# 1. Obtenir un token Auditor
# ...

# 2. Consulter les audit logs
curl -X GET https://localhost:5001/api/v1/audit/search?startDate=2024-01-01 \
  -H "Authorization: Bearer {auditor_token}"
```

### Exemple 4: API Client (SIS Laravel) Génère des Documents

```bash
# 1. Obtenir un token via Client Credentials
curl -X POST https://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials" \
  -d "client_id=sis-laravel-client" \
  -d "client_secret={secret}" \
  -d "scope=api.documents.generate"

# 2. Générer un document
curl -X POST https://localhost:5001/api/v1/documents/generate \
  -H "Authorization: Bearer {api_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "studentId": "123456",
    "documentType": "Diploma"
  }'
```

## Bonnes Pratiques

### 1. Principe du Moindre Privilège

Assignez uniquement les rôles nécessaires aux utilisateurs:
- ✅ Registrar pour le personnel du registrariat
- ✅ Auditor pour les auditeurs
- ❌ Éviter d'assigner Administrator sauf nécessité absolue

### 2. Séparation des Responsabilités

- **Administrator:** Gestion système uniquement
- **Registrar:** Opérations quotidiennes de génération/signature
- **Auditor:** Surveillance et conformité
- **API Client:** Intégrations système

### 3. Audit des Changements de Rôles

Tous les changements de rôles doivent être loggés pour traçabilité:
- Qui a assigné le rôle?
- À quel utilisateur?
- Quand?
- Pourquoi?

### 4. Révision Périodique

Réviser les rôles assignés tous les 90 jours:
- Retirer les rôles inutilisés
- Vérifier que les utilisateurs ont toujours besoin de leurs permissions
- Désactiver les comptes inactifs

### 5. Multi-Factor Authentication (MFA)

**MFA est obligatoire pour les comptes Administrator** (NFR-S12).

Configuration MFA dans `Program.cs`:

```csharp
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;
    options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
```

## Dépannage

### Problème: 403 Forbidden malgré le bon rôle

**Vérifications:**
1. Le token contient-il le claim `role`?
   ```bash
   # Décoder le JWT sur jwt.io
   ```
2. Le rôle est-il correctement assigné dans la base de données?
   ```sql
   SELECT * FROM AspNetUserRoles WHERE UserId = '{userId}';
   ```
3. La policy est-elle correctement configurée?

### Problème: Rôle non inclus dans le JWT

**Solution:** Vérifier que l'endpoint Authorization ajoute bien les rôles:

```csharp
var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
foreach (var role in roles)
{
    identity.AddClaim(new Claim(Claims.Role, role));
}
```

### Problème: Impossible d'assigner un rôle

**Erreurs possibles:**
- Le rôle n'existe pas dans la base de données
- L'utilisateur a déjà ce rôle
- L'utilisateur effectuant l'opération n'est pas Administrator

## Support

Pour toute question ou problème, consultez:
- Documentation ASP.NET Identity: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity
- Architecture AcadSign: `_bmad-output/planning-artifacts/architecture.md`
- Story 2.4: `_bmad-output/implementation-artifacts/2-4-implementer-rbac-admin-registrar-auditor-api-client.md`
