# Client Credentials Flow - Guide d'Utilisation

## Vue d'ensemble

Ce document explique comment utiliser le flow OAuth 2.0 Client Credentials pour authentifier le SIS Laravel auprès de l'API Backend AcadSign.

## Prérequis

- OpenIddict configuré (Story 2.1)
- PostgreSQL en cours d'exécution
- Backend API démarré

## Configuration

### 1. Obtenir les Credentials du Client

Au premier démarrage de l'application en mode développement, les credentials du client SIS Laravel sont automatiquement générés et affichés dans la console :

```
===========================================
SIS Laravel Client Created
Client ID: sis-laravel-client
Client Secret: [SECRET_BASE64]
IMPORTANT: Save this secret securely!
===========================================
```

**⚠️ IMPORTANT:** Sauvegardez ce secret de manière sécurisée. Il ne sera affiché qu'une seule fois.

### 2. Configuration SIS Laravel

Ajoutez les variables d'environnement suivantes dans le fichier `.env` du SIS Laravel :

```env
ACADSIGN_CLIENT_ID=sis-laravel-client
ACADSIGN_CLIENT_SECRET=[SECRET_FROM_CONSOLE]
ACADSIGN_TOKEN_URL=https://localhost:5001/connect/token
ACADSIGN_API_URL=https://localhost:5001/api/v1
```

## Utilisation

### 1. Obtenir un Access Token

**Requête:**

```bash
curl -X POST https://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials" \
  -d "client_id=sis-laravel-client" \
  -d "client_secret=[YOUR_SECRET]" \
  -d "scope=api.documents.generate api.documents.read"
```

**Réponse:**

```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "scope": "api.documents.generate api.documents.read"
}
```

### 2. Utiliser le Token pour Appeler l'API

**Générer un Document:**

```bash
curl -X POST https://localhost:5001/api/v1/documents \
  -H "Authorization: Bearer [ACCESS_TOKEN]" \
  -H "Content-Type: application/json" \
  -d '{
    "documentType": "transcript",
    "studentId": "12345",
    "language": "fr"
  }'
```

**Récupérer les Métadonnées d'un Document:**

```bash
curl -X GET https://localhost:5001/api/v1/documents/{documentId} \
  -H "Authorization: Bearer [ACCESS_TOKEN]"
```

## Scopes Disponibles

| Scope | Description |
|-------|-------------|
| `api.documents.generate` | Permission de générer des documents académiques |
| `api.documents.read` | Permission de lire les métadonnées des documents |

## Gestion des Erreurs

### 401 Unauthorized

Le token est expiré ou invalide :

```json
{
  "error": "invalid_token",
  "error_description": "The access token is invalid or expired"
}
```

**Solution:** Obtenez un nouveau token via `/connect/token`.

### 403 Forbidden

Le token ne possède pas les scopes requis :

```json
{
  "error": "insufficient_scope",
  "error_description": "The token does not have the required scope"
}
```

**Solution:** Demandez un token avec les scopes appropriés.

## Rotation des Secrets

Les secrets doivent être rotés tous les 90 jours pour des raisons de sécurité.

### Processus de Rotation

1. **Générer un nouveau secret:**

```bash
cd /path/to/AcadSign.Backend
dotnet run --project src/Web -- rotate-client-secret sis-laravel-client
```

2. **Mettre à jour la configuration SIS Laravel:**

Mettez à jour la variable `ACADSIGN_CLIENT_SECRET` dans le fichier `.env` du SIS Laravel avec le nouveau secret.

3. **Redémarrer le SIS Laravel:**

```bash
php artisan config:clear
php artisan cache:clear
```

## Exemple d'Implémentation PHP (SIS Laravel)

```php
<?php

namespace App\Services;

use Illuminate\Support\Facades\Http;

class AcadSignClient
{
    private string $clientId;
    private string $clientSecret;
    private string $tokenUrl;
    private string $apiUrl;
    private ?string $accessToken = null;
    private ?int $tokenExpiry = null;

    public function __construct()
    {
        $this->clientId = config('acadsign.client_id');
        $this->clientSecret = config('acadsign.client_secret');
        $this->tokenUrl = config('acadsign.token_url');
        $this->apiUrl = config('acadsign.api_url');
    }

    public function generateDocument(string $documentType, string $studentId, string $language): array
    {
        $this->ensureValidToken();

        $response = Http::withToken($this->accessToken)
            ->post("{$this->apiUrl}/documents", [
                'documentType' => $documentType,
                'studentId' => $studentId,
                'language' => $language,
            ]);

        return $response->json();
    }

    public function getDocument(string $documentId): array
    {
        $this->ensureValidToken();

        $response = Http::withToken($this->accessToken)
            ->get("{$this->apiUrl}/documents/{$documentId}");

        return $response->json();
    }

    private function ensureValidToken(): void
    {
        if ($this->accessToken && $this->tokenExpiry > time()) {
            return;
        }

        $response = Http::asForm()->post($this->tokenUrl, [
            'grant_type' => 'client_credentials',
            'client_id' => $this->clientId,
            'client_secret' => $this->clientSecret,
            'scope' => 'api.documents.generate api.documents.read',
        ]);

        $data = $response->json();
        $this->accessToken = $data['access_token'];
        $this->tokenExpiry = time() + $data['expires_in'] - 60; // 60s buffer
    }
}
```

## Sécurité

### Bonnes Pratiques

1. **Ne jamais exposer le client secret** dans le code source ou les logs
2. **Utiliser HTTPS** en production
3. **Stocker les secrets** dans des variables d'environnement ou un gestionnaire de secrets
4. **Roter les secrets** tous les 90 jours
5. **Monitorer les appels API** pour détecter les abus
6. **Limiter les scopes** au strict nécessaire

### Production

En production, les certificats JWT doivent être stockés dans Azure Key Vault au lieu d'utiliser les certificats de développement.

## Support

Pour toute question ou problème, consultez :
- Documentation OpenIddict: https://documentation.openiddict.com/
- Architecture AcadSign: `_bmad-output/planning-artifacts/architecture.md`
