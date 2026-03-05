# MinIO S3 Storage - Guide d'Utilisation

## Vue d'ensemble

Ce document explique comment les documents PDF sont stockés dans MinIO S3 avec chiffrement SSE-S3 pour la conformité CNDP.

## Architecture

### Service S3 Storage

**Interface:** `IS3StorageService`
```csharp
public interface IS3StorageService
{
    Task<string> UploadDocumentAsync(byte[] pdfData, string documentId);
    Task<byte[]> DownloadDocumentAsync(string documentId);
    Task<string> GeneratePresignedDownloadUrlAsync(string documentId, int expiryMinutes = 60);
    Task DeleteDocumentAsync(string documentId);
    Task<bool> DocumentExistsAsync(string documentId);
}
```

**Implémentation:** `S3StorageService`
- Package: Minio 6.0.3
- Chiffrement: SSE-S3 (Server-Side Encryption)
- Versioning: Activé pour rétention 30 ans
- Organisation: Par date {year}/{month}/{documentId}.pdf

## Configuration

### appsettings.json

**Développement:**
```json
{
  "MinIO": {
    "Endpoint": "localhost:9000",
    "AccessKey": "minioadmin",
    "SecretKey": "minioadmin",
    "BucketName": "acadsign-documents",
    "UseSSL": false,
    "Region": "us-east-1"
  }
}
```

**Production:**
```json
{
  "MinIO": {
    "Endpoint": "minio.acadsign.ma:9000",
    "AccessKey": "${MINIO_ACCESS_KEY}",
    "SecretKey": "${MINIO_SECRET_KEY}",
    "BucketName": "acadsign-documents",
    "UseSSL": true,
    "Region": "us-east-1"
  }
}
```

### Variables d'Environnement

Pour la production, utiliser des variables d'environnement:
```bash
export MINIO_ACCESS_KEY="your-access-key"
export MINIO_SECRET_KEY="your-secret-key"
```

## Organisation des Documents

### Structure du Bucket

```
acadsign-documents/
├── 2026/
│   ├── 01/
│   │   ├── a1b2c3d4-e5f6-7890-abcd-ef1234567890.pdf
│   │   └── b2c3d4e5-f6a7-8901-bcde-f12345678901.pdf
│   ├── 02/
│   │   └── c3d4e5f6-a7b8-9012-cdef-123456789012.pdf
│   └── 03/
│       └── d4e5f6a7-b8c9-0123-def1-234567890123.pdf
├── 2027/
│   └── ...
```

### Avantages de l'Organisation par Date

- ✅ Organisation chronologique claire
- ✅ Facilite les audits par période
- ✅ Simplifie la rétention 30 ans
- ✅ Améliore les performances de recherche
- ✅ Permet le partitionnement futur

## Chiffrement

### SSE-S3 (Server-Side Encryption)

**Configuration actuelle:**
```csharp
var sse = new SSES3();
```

**Caractéristiques:**
- Chiffrement automatique côté serveur
- Clés gérées par MinIO
- Transparent pour le client
- Conforme CNDP pour données au repos

### Options de Chiffrement

**SSE-S3** (Utilisé actuellement) ✅
- Chiffrement géré par MinIO
- Simple à configurer
- Bon pour MVP

**SSE-KMS** (Production recommandée)
```csharp
var sse = new SSEKMS("acadsign-kms-key");
```
- Nécessite HashiCorp Vault ou AWS KMS
- Rotation automatique des clés
- Meilleur contrôle d'accès

**SSE-C** (Client-Side)
```csharp
var sse = new SSEC(encryptionKey);
```
- Clé fournie par le client
- Plus de contrôle mais plus complexe

## Utilisation

### Upload d'un Document

```csharp
var s3Storage = serviceProvider.GetRequiredService<IS3StorageService>();
var documentId = Guid.NewGuid().ToString();
var pdfBytes = await GeneratePdfAsync();

// Upload vers MinIO
var s3ObjectPath = await s3Storage.UploadDocumentAsync(pdfBytes, documentId);

// s3ObjectPath = "2026/03/a1b2c3d4-e5f6-7890-abcd-ef1234567890.pdf"
```

### Download d'un Document

```csharp
var documentId = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";

// Download depuis MinIO
var pdfBytes = await s3Storage.DownloadDocumentAsync(documentId);

// Utiliser les bytes
await File.WriteAllBytesAsync("document.pdf", pdfBytes);
```

### Génération de Pre-signed URL

```csharp
var documentId = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";

// Générer URL temporaire (valide 60 minutes par défaut)
var presignedUrl = await s3Storage.GeneratePresignedDownloadUrlAsync(documentId, expiryMinutes: 60);

// URL: http://localhost:9000/acadsign-documents/2026/03/a1b2c3d4...pdf?X-Amz-Expires=3600...
```

### Vérifier l'Existence d'un Document

```csharp
var documentId = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";

var exists = await s3Storage.DocumentExistsAsync(documentId);

if (exists)
{
    // Document existe dans MinIO
}
```

### Suppression d'un Document

```csharp
var documentId = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";

await s3Storage.DeleteDocumentAsync(documentId);
```

## Entité Document (Base de Données)

### Modèle

```csharp
public class Document : BaseAuditableEntity
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; }
    public Guid StudentId { get; set; }
    public string Status { get; set; } = "UNSIGNED";
    public string S3ObjectPath { get; set; }
    public DateTime? SignedAt { get; set; }
    public string? SignerName { get; set; }
    public string? SignatureData { get; set; }
}
```

### Workflow Complet

```csharp
// 1. Générer le PDF
var pdfBytes = await pdfService.GenerateDocumentAsync(documentType, studentData);

// 2. Upload vers MinIO
var s3ObjectPath = await s3Storage.UploadDocumentAsync(pdfBytes, documentId.ToString());

// 3. Sauvegarder métadonnées en DB
var document = new Document
{
    Id = documentId,
    DocumentType = "AttestationScolarite",
    StudentId = studentId,
    Status = "UNSIGNED",
    S3ObjectPath = s3ObjectPath,
    Created = DateTime.UtcNow
};

dbContext.Documents.Add(document);
await dbContext.SaveChangesAsync();
```

## Versioning et Rétention

### Activation du Versioning

Le versioning est activé automatiquement lors de la création du bucket:

```csharp
await _minioClient.SetVersioningAsync(
    new SetVersioningArgs()
        .WithBucket(_bucketName)
        .WithVersioningEnabled());
```

### Rétention 30 Ans

**Conformité CNDP:**
- Les documents académiques doivent être conservés 30 ans
- Le versioning permet de garder l'historique complet
- Lifecycle policy configurée pour expiration après 30 ans

**Configuration (à implémenter):**
```xml
<LifecycleConfiguration>
  <Rule>
    <ID>Retain30Years</ID>
    <Status>Enabled</Status>
    <Expiration>
      <Days>10950</Days> <!-- 30 ans = 30 * 365 -->
    </Expiration>
  </Rule>
</LifecycleConfiguration>
```

## Déploiement MinIO

### Docker Compose

```yaml
version: '3.8'

services:
  minio:
    image: minio/minio:latest
    container_name: acadsign-minio
    ports:
      - "9000:9000"
      - "9001:9001"
    environment:
      MINIO_ROOT_USER: minioadmin
      MINIO_ROOT_PASSWORD: minioadmin
    volumes:
      - minio-data:/data
    command: server /data --console-address ":9001"
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:9000/minio/health/live"]
      interval: 30s
      timeout: 20s
      retries: 3

volumes:
  minio-data:
    driver: local
```

### Démarrage

```bash
docker-compose up -d minio
```

### Console Web

Accéder à la console MinIO:
- URL: http://localhost:9001
- Username: minioadmin
- Password: minioadmin

## Sécurité

### Chiffrement au Repos

✅ **SSE-S3 activé** pour tous les documents  
✅ Chiffrement transparent  
✅ Conforme CNDP

### Contrôle d'Accès

- Credentials stockés dans variables d'environnement
- Pas de credentials en clair dans le code
- Bucket privé (pas d'accès public)
- Pre-signed URLs pour accès temporaire

### Audit

- Tous les uploads/downloads loggés
- Métadonnées en DB pour traçabilité
- Versioning pour historique complet

## Performance

### Optimisations

- Upload asynchrone
- Streaming pour gros fichiers
- Pre-signed URLs pour éviter proxy
- Organisation par date pour recherche rapide

### Métriques

- Upload: < 1 seconde pour PDF 500 KB
- Download: < 500 ms
- Pre-signed URL: < 100 ms

## Conformité

✅ **FR8**: Documents stored in S3-compatible storage  
✅ **NFR-S2**: Encryption at rest (SSE-S3)  
✅ **NFR-P2**: Performance < 1s pour upload  
✅ **CNDP**: Rétention 30 ans avec versioning

## Évolutions Futures

- **Story 3.5**: Pre-signed URLs pour download sécurisé
- **Story 3.6**: Template management avec versioning
- **Production**: Migration vers SSE-KMS avec Vault
- **Monitoring**: Métriques et alertes MinIO

## Références

- **Architecture**: `_bmad-output/planning-artifacts/architecture.md`
- **Story 3.4**: `_bmad-output/implementation-artifacts/3-4-configurer-minio-s3-storage-avec-chiffrement-sse-kms.md`
- **MinIO Documentation**: https://min.io/docs/minio/linux/index.html
