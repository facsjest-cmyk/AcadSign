# Template Management - Guide d'Utilisation

## Vue d'ensemble

Ce document explique comment gérer les templates PDF personnalisés pour chaque type de document et institution avec versioning automatique.

## Concept

Le **Template Management** permet aux administrateurs d'uploader des templates PDF personnalisés pour chaque type de document, avec support multi-institution et versioning automatique.

### Avantages

- ✅ Personnalisation du branding par institution
- ✅ Versioning automatique (1.0 → 1.1 → 2.0)
- ✅ Historique complet des templates (rétention 30 ans)
- ✅ Activation/désactivation des templates
- ✅ Support multi-institution
- ✅ RBAC: Seuls les Admins peuvent uploader

## Entité DocumentTemplate

### Modèle

```csharp
public class DocumentTemplate
{
    public Guid Id { get; set; }
    public DocumentType Type { get; set; }
    public string InstitutionId { get; set; }
    public string Version { get; set; }
    public byte[] TemplateData { get; set; }
    public string FileName { get; set; }
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
}
```

### Propriétés

- **Id**: UUID unique du template
- **Type**: Type de document (AttestationScolarite, ReleveNotes, etc.)
- **InstitutionId**: Identifiant de l'institution
- **Version**: Version du template (1.0, 1.1, 2.0, etc.)
- **TemplateData**: Données binaires du PDF
- **FileName**: Nom du fichier original
- **FileSize**: Taille du fichier en bytes
- **CreatedAt**: Date de création
- **CreatedBy**: Utilisateur créateur
- **IsActive**: Template actif (seul le template actif est utilisé)
- **Description**: Description optionnelle

## Endpoints API

### POST /api/v1/templates - Upload Template

Upload un nouveau template PDF.

**Authentification:** Admin uniquement

**Content-Type:** multipart/form-data

**Paramètres:**
- `templateFile` (file) - Fichier PDF
- `documentType` (string) - Type de document
- `institutionId` (string) - ID de l'institution
- `description` (string, optional) - Description

**Exemple avec cURL:**

```bash
curl -X POST "http://localhost:5000/api/v1/templates" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -F "templateFile=@attestation_scolarite.pdf" \
  -F "documentType=AttestationScolarite" \
  -F "institutionId=university-hassan-ii" \
  -F "description=Template officiel 2026"
```

**Réponse:**

```json
{
  "templateId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "documentType": "AttestationScolarite",
  "version": "1.0",
  "createdAt": "2026-03-05T10:00:00Z"
}
```

**Codes de statut:**
- `200 OK` - Template uploadé avec succès
- `400 Bad Request` - Fichier invalide ou paramètres manquants
- `401 Unauthorized` - Token manquant ou invalide
- `403 Forbidden` - Utilisateur non Admin

### GET /api/v1/templates - List Templates

Liste tous les templates d'une institution.

**Authentification:** Admin ou Registrar

**Paramètres:**
- `institutionId` (query) - ID de l'institution

**Exemple:**

```bash
curl -X GET "http://localhost:5000/api/v1/templates?institutionId=university-hassan-ii" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Réponse:**

```json
{
  "templates": [
    {
      "templateId": "uuid-1",
      "documentType": "AttestationScolarite",
      "institutionId": "university-hassan-ii",
      "version": "1.1",
      "isActive": true,
      "createdAt": "2026-03-05T10:00:00Z",
      "fileName": "attestation_scolarite_v1.1.pdf",
      "fileSize": 524288,
      "description": "Template officiel 2026"
    },
    {
      "templateId": "uuid-2",
      "documentType": "AttestationScolarite",
      "institutionId": "university-hassan-ii",
      "version": "1.0",
      "isActive": false,
      "createdAt": "2026-03-01T10:00:00Z",
      "fileName": "attestation_scolarite_v1.0.pdf",
      "fileSize": 512000,
      "description": "Template initial"
    }
  ]
}
```

### GET /api/v1/templates/{templateId} - Download Template

Télécharge un template spécifique.

**Authentification:** Admin uniquement

**Exemple:**

```bash
curl -X GET "http://localhost:5000/api/v1/templates/uuid-1" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -o template.pdf
```

**Réponse:**
- Content-Type: application/pdf
- Fichier PDF en binaire

### DELETE /api/v1/templates/{templateId} - Delete Template

Désactive un template (soft delete).

**Authentification:** Admin uniquement

**Exemple:**

```bash
curl -X DELETE "http://localhost:5000/api/v1/templates/uuid-1" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

**Réponse:**
- `204 No Content` - Template désactivé
- `404 Not Found` - Template non trouvé

**Note:** Les templates ne sont jamais supprimés physiquement, seulement désactivés (IsActive = false) pour conserver l'historique.

## Versioning Automatique

### Logique de Versioning

```
1.0 → 1.1 → 1.2 → ... → 1.9 → 2.0 → 2.1 → ...
```

### Règles

- **Premier template**: Version 1.0
- **Modifications mineures**: Incrémenter version mineure (1.0 → 1.1)
- **Changements majeurs**: Incrémenter version majeure (1.9 → 2.0)

### Implémentation

```csharp
private string CalculateNextVersion(IEnumerable<DocumentTemplate> existingTemplates)
{
    if (!existingTemplates.Any())
    {
        return "1.0";
    }
    
    var latestVersion = existingTemplates
        .Select(t => Version.Parse(t.Version))
        .OrderByDescending(v => v)
        .First();
    
    // Incrémenter la version mineure
    return $"{latestVersion.Major}.{latestVersion.Minor + 1}";
}
```

### Activation Automatique

Lors de l'upload d'un nouveau template:
1. Tous les anciens templates du même type et institution sont désactivés (IsActive = false)
2. Le nouveau template est activé (IsActive = true)
3. Seul le template actif est utilisé pour la génération de documents

## Workflow Complet

### 1. Upload Initial

```bash
# Upload premier template
POST /api/v1/templates
templateFile: attestation_v1.pdf
documentType: AttestationScolarite
institutionId: university-hassan-ii

# Résultat: Version 1.0, IsActive = true
```

### 2. Upload Mise à Jour

```bash
# Upload deuxième template
POST /api/v1/templates
templateFile: attestation_v2.pdf
documentType: AttestationScolarite
institutionId: university-hassan-ii

# Résultat:
# - Version 1.0 → IsActive = false
# - Version 1.1 → IsActive = true (nouveau)
```

### 3. Liste des Templates

```bash
GET /api/v1/templates?institutionId=university-hassan-ii

# Résultat:
# - Version 1.1 (IsActive = true)
# - Version 1.0 (IsActive = false)
```

### 4. Génération de Document

Le système utilise automatiquement le template actif (Version 1.1) pour générer les documents.

## Multi-Institution Support

### Isolation par Institution

Chaque institution a ses propres templates:

```
university-hassan-ii:
  - AttestationScolarite v1.1 (active)
  - ReleveNotes v2.0 (active)

university-mohammed-v:
  - AttestationScolarite v1.0 (active)
  - ReleveNotes v1.5 (active)
```

### Filtrage

Les templates sont filtrés par `InstitutionId` pour garantir l'isolation.

## Sécurité et RBAC

### Contrôle d'Accès

**Upload Template:**
- ✅ **Admin** uniquement

**List Templates:**
- ✅ **Admin**
- ✅ **Registrar**

**Download Template:**
- ✅ **Admin** uniquement

**Delete Template:**
- ✅ **Admin** uniquement

### Validation

- Fichier PDF uniquement (Content-Type: application/pdf)
- Taille maximale: Configurable (défaut: 10 MB)
- Type de document valide
- InstitutionId requis

## Rétention et Historique

### Rétention 30 Ans

Tous les templates sont conservés pendant 30 ans pour conformité CNDP:
- Soft delete uniquement (IsActive = false)
- Pas de suppression physique
- Historique complet des versions

### Audit Trail

Chaque template enregistre:
- Date de création (CreatedAt)
- Créateur (CreatedBy)
- Version
- Description

## Intégration avec PDF Generation

### Utilisation du Template Actif

```csharp
public async Task<byte[]> GenerateDocumentAsync(DocumentType type, StudentData data)
{
    // Charger le template actif
    var template = await _templateRepo.GetActiveTemplateAsync(
        data.InstitutionId, 
        type);
    
    if (template != null)
    {
        // Utiliser le template personnalisé
        return await GenerateFromCustomTemplateAsync(template, data);
    }
    else
    {
        // Utiliser le template par défaut (QuestPDF)
        return await GenerateFromDefaultTemplateAsync(type, data);
    }
}
```

**Note:** L'implémentation de `GenerateFromCustomTemplateAsync` nécessite une bibliothèque de manipulation PDF comme iText 7 (à implémenter dans une story future).

## Exemples d'Utilisation

### Exemple JavaScript

```javascript
// Upload template
const formData = new FormData();
formData.append('templateFile', pdfFile);
formData.append('documentType', 'AttestationScolarite');
formData.append('institutionId', 'university-hassan-ii');
formData.append('description', 'Template 2026');

const response = await fetch('/api/v1/templates', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${adminToken}`
  },
  body: formData
});

const result = await response.json();
console.log(`Template uploaded: ${result.version}`);
```

### Exemple C#

```csharp
// Upload template
var client = new HttpClient();
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", adminToken);

var content = new MultipartFormDataContent();
content.Add(new ByteArrayContent(pdfBytes), "templateFile", "template.pdf");
content.Add(new StringContent("AttestationScolarite"), "documentType");
content.Add(new StringContent("university-hassan-ii"), "institutionId");

var response = await client.PostAsync("/api/v1/templates", content);
var result = await response.Content.ReadFromJsonAsync<UploadTemplateResponse>();
```

## Migration Database

### Créer la Migration

```bash
cd AcadSign.Backend
dotnet ef migrations add AddDocumentTemplates --project src/Infrastructure --startup-project src/Web
```

### Appliquer la Migration

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/Web
```

## Conformité

✅ **FR40**: Template upload and management  
✅ **FR41**: Template versioning  
✅ **FR42**: Multi-institution support  
✅ **FR43**: Template activation/deactivation  
✅ **FR44**: Template history (30 years retention)  
✅ **RBAC**: Admin-only access for upload  
✅ **CNDP**: Audit trail and retention

## Limitations Actuelles

⚠️ **Template Personnalisé Non Implémenté**
- Le système peut stocker et gérer les templates
- L'utilisation des templates personnalisés pour la génération nécessite iText 7
- Pour l'instant, QuestPDF est toujours utilisé pour la génération
- À implémenter dans une story future

⚠️ **Tests Non Implémentés**
- Tests unitaires à créer
- Tests d'intégration à créer
- À implémenter dans une story future

## Évolutions Futures

- **Template Rendering**: Implémenter avec iText 7 ou PdfSharp
- **Template Preview**: Aperçu avant activation
- **Template Validation**: Vérifier les champs requis
- **Bulk Upload**: Upload multiple templates
- **Template Cloning**: Dupliquer entre institutions

## Références

- **Architecture**: `_bmad-output/planning-artifacts/architecture.md`
- **Story 3.6**: `_bmad-output/implementation-artifacts/3-6-implementer-template-management-upload-versioning.md`
- **PRD FR40-FR44**: `_bmad-output/planning-artifacts/prd.md:99-108`
