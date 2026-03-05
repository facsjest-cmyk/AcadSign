# Pre-Signed URLs - Guide d'Utilisation

## Vue d'ensemble

Ce document explique comment les pre-signed URLs sont générées pour permettre aux étudiants de télécharger leurs documents de manière sécurisée sans authentification directe.

## Concept

Les **pre-signed URLs** sont des URLs temporaires qui permettent l'accès à des ressources privées sans nécessiter d'authentification au moment du téléchargement.

### Avantages

- ✅ Pas besoin de stocker les credentials côté client
- ✅ Expiration automatique après 1 heure
- ✅ Pas d'authentification requise pour le téléchargement
- ✅ URL unique et non-réutilisable après expiration
- ✅ Sécurité renforcée (URL signée cryptographiquement)

### Considérations

- ⚠️ L'URL peut être partagée pendant sa validité (1h)
- ⚠️ Après expiration, une nouvelle URL doit être générée
- ⚠️ L'étudiant doit s'authentifier pour obtenir l'URL

## Endpoint API

### GET /api/v1/documents/{documentId}/download

Génère une pre-signed URL pour télécharger un document.

**Authentification:** Requise (Bearer Token)

**Paramètres:**
- `documentId` (path) - UUID du document

**Réponse:**
```json
{
  "downloadUrl": "http://localhost:9000/acadsign-documents/2026/03/a1b2c3d4-e5f6-7890-abcd-ef1234567890.pdf?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=minioadmin%2F20260305%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20260305T093500Z&X-Amz-Expires=3600&X-Amz-SignedHeaders=host&X-Amz-Signature=...",
  "expiresAt": "2026-03-05T10:35:00Z"
}
```

**Codes de statut:**
- `200 OK` - URL générée avec succès
- `401 Unauthorized` - Token manquant ou invalide
- `403 Forbidden` - Accès refusé au document
- `404 Not Found` - Document non trouvé

## Flow Complet

### 1. Étudiant demande le lien de téléchargement

```http
GET /api/v1/documents/a1b2c3d4-e5f6-7890-abcd-ef1234567890/download
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 2. Backend vérifie les permissions

Le système vérifie:
- ✅ Le document existe dans la base de données
- ✅ L'utilisateur est authentifié
- ✅ L'utilisateur a accès au document (Admin, Registrar, ou propriétaire)

### 3. Backend génère la pre-signed URL

```csharp
var downloadUrl = await s3Storage.GeneratePresignedDownloadUrlAsync(
    documentId.ToString(),
    expiryMinutes: 60);
```

### 4. Étudiant reçoit la réponse

```json
{
  "downloadUrl": "http://localhost:9000/acadsign-documents/2026/03/uuid.pdf?X-Amz-...",
  "expiresAt": "2026-03-05T10:35:00Z"
}
```

### 5. Étudiant télécharge le PDF

```http
GET http://localhost:9000/acadsign-documents/2026/03/uuid.pdf?X-Amz-...
# Pas d'authentification requise
# Retourne le PDF en binaire (application/pdf)
```

### 6. Après expiration (1h)

```http
GET http://localhost:9000/acadsign-documents/2026/03/uuid.pdf?X-Amz-...
# Retourne: 403 Forbidden
# Message: Request has expired
```

## Exemples d'Utilisation

### Exemple avec cURL

```bash
# 1. Obtenir la pre-signed URL
curl -X GET "http://localhost:5000/api/v1/documents/a1b2c3d4-e5f6-7890-abcd-ef1234567890/download" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  | jq -r '.downloadUrl'

# 2. Télécharger le PDF
curl -X GET "http://localhost:9000/acadsign-documents/2026/03/uuid.pdf?X-Amz-..." \
  -o document.pdf
```

### Exemple avec JavaScript

```javascript
// 1. Obtenir la pre-signed URL
const response = await fetch(
  `https://api.acadsign.ma/api/v1/documents/${documentId}/download`,
  {
    headers: {
      'Authorization': `Bearer ${jwtToken}`
    }
  }
);

const { downloadUrl, expiresAt } = await response.json();

// 2. Télécharger le PDF
const pdfResponse = await fetch(downloadUrl);
const pdfBlob = await pdfResponse.blob();

// 3. Créer un lien de téléchargement
const url = window.URL.createObjectURL(pdfBlob);
const a = document.createElement('a');
a.href = url;
a.download = 'document.pdf';
a.click();
```

### Exemple avec C# (Desktop App)

```csharp
// 1. Obtenir la pre-signed URL
var client = new HttpClient();
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", jwtToken);

var response = await client.GetAsync(
    $"https://api.acadsign.ma/api/v1/documents/{documentId}/download");

var result = await response.Content.ReadFromJsonAsync<DownloadUrlResponse>();

// 2. Télécharger le PDF
var pdfBytes = await client.GetByteArrayAsync(result.DownloadUrl);

// 3. Sauvegarder localement
await File.WriteAllBytesAsync("document.pdf", pdfBytes);
```

## Sécurité

### Contrôle d'Accès

**Qui peut générer une pre-signed URL?**
- ✅ **Admin** - Accès à tous les documents
- ✅ **Registrar** - Accès à tous les documents
- ✅ **Étudiant** - Accès uniquement à ses propres documents

**Vérifications effectuées:**
1. Token JWT valide
2. Document existe dans la base de données
3. Utilisateur a les permissions nécessaires
4. Document est dans un état téléchargeable (SIGNED ou UNSIGNED pour MVP)

### Signature Cryptographique

La pre-signed URL contient:
- `X-Amz-Algorithm` - Algorithme de signature (AWS4-HMAC-SHA256)
- `X-Amz-Credential` - Credentials utilisés
- `X-Amz-Date` - Date de génération
- `X-Amz-Expires` - Durée de validité (3600 secondes = 1 heure)
- `X-Amz-Signature` - Signature cryptographique

**Toute modification de l'URL invalide la signature.**

### Expiration

- **Durée:** 1 heure (3600 secondes)
- **Calcul:** `DateTime.UtcNow.AddHours(1)`
- **Après expiration:** HTTP 403 Forbidden
- **Renouvellement:** Générer une nouvelle URL

## Configuration

### Développement

```json
{
  "MinIO": {
    "Endpoint": "localhost:9000",
    "UseSSL": false
  }
}
```

**URL générée:**
```
http://localhost:9000/acadsign-documents/2026/03/uuid.pdf?X-Amz-...
```

### Production

```json
{
  "MinIO": {
    "Endpoint": "minio.acadsign.ma:9000",
    "UseSSL": true
  }
}
```

**URL générée:**
```
https://minio.acadsign.ma/acadsign-documents/2026/03/uuid.pdf?X-Amz-...
```

## Performance

### Génération de l'URL

- **Temps:** < 100 ms
- **Conformité:** NFR-P6 (< 1 seconde)
- **Opération:** Calcul cryptographique côté serveur

### Téléchargement

- **Temps:** Dépend de la taille du fichier et de la bande passante
- **Optimisation:** Accès direct à MinIO (pas de proxy)
- **Streaming:** Supporté par MinIO

## Intégration Future

### Email Notification (Epic 9)

```csharp
// Après génération du document signé
var downloadUrl = await s3Storage.GeneratePresignedDownloadUrlAsync(
    documentId.ToString());

// Envoyer email à l'étudiant
await emailService.SendDocumentReadyEmailAsync(new EmailData
{
    To = student.Email,
    Subject = "Votre document est prêt",
    Body = $@"
        Bonjour {student.FirstName},
        
        Votre document est disponible au téléchargement:
        {downloadUrl}
        
        Ce lien expire dans 1 heure.
        
        Cordialement,
        L'équipe AcadSign
    ",
    ExpiresAt = DateTime.UtcNow.AddHours(1)
});
```

### Desktop App Integration

```csharp
// Dans le ViewModel
public async Task DownloadDocumentAsync(Guid documentId)
{
    try
    {
        // 1. Obtenir la pre-signed URL
        var response = await apiClient.GetDownloadUrlAsync(documentId);
        
        // 2. Télécharger le PDF
        var pdfBytes = await httpClient.GetByteArrayAsync(response.DownloadUrl);
        
        // 3. Sauvegarder avec SaveFileDialog
        var dialog = new SaveFileDialog
        {
            Filter = "PDF Files (*.pdf)|*.pdf",
            FileName = $"document_{documentId}.pdf"
        };
        
        if (dialog.ShowDialog() == true)
        {
            await File.WriteAllBytesAsync(dialog.FileName, pdfBytes);
            MessageBox.Show("Document téléchargé avec succès!");
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Erreur: {ex.Message}");
    }
}
```

## Troubleshooting

### Erreur: 403 Forbidden

**Causes possibles:**
- URL expirée (> 1 heure)
- Signature invalide (URL modifiée)
- MinIO non accessible

**Solution:**
- Générer une nouvelle URL
- Vérifier que MinIO est démarré
- Vérifier la configuration MinIO

### Erreur: 404 Not Found

**Causes possibles:**
- Document n'existe pas dans MinIO
- Chemin S3 incorrect

**Solution:**
- Vérifier que le document a été uploadé
- Vérifier le chemin dans la base de données

### Erreur: Téléchargement lent

**Causes possibles:**
- Fichier très volumineux
- Bande passante limitée
- MinIO surchargé

**Solution:**
- Optimiser la taille des PDFs
- Augmenter les ressources MinIO
- Utiliser un CDN en production

## Conformité

✅ **FR9**: Pre-signed download URLs with expiration  
✅ **NFR-P6**: Pre-signed URL generation < 1 second  
✅ **NFR-S3**: Secure access control  
✅ **CNDP**: Traçabilité des accès (logs)

## Références

- **Architecture**: `_bmad-output/planning-artifacts/architecture.md`
- **Story 3.5**: `_bmad-output/implementation-artifacts/3-5-implementer-generation-de-pre-signed-urls.md`
- **MinIO Pre-signed URLs**: https://min.io/docs/minio/linux/developers/dotnet/API.html#presignedgetobjectasync
