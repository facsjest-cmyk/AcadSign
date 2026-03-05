# QR Code Verification - Guide d'Utilisation

## Vue d'ensemble

Ce document explique comment les QR codes sont générés et intégrés dans les documents PDF pour permettre la vérification publique de l'authenticité.

## Architecture

### Service QR Code

**Interface:** `IQrCodeService`
```csharp
public interface IQrCodeService
{
    byte[] GenerateQrCode(string data, int pixelSize = 300);
}
```

**Implémentation:** `QrCodeService`
- Package: QRCoder 1.6.0
- Niveau de correction d'erreur: Medium (M) - ~15% de correction
- Format de sortie: PNG
- Taille par défaut: 300x300 pixels

### Intégration dans PDF

Les QR codes sont automatiquement générés et intégrés dans chaque document PDF lors de la génération.

**Position:**
- Bas à droite du document
- Taille: 80x80 pixels (environ 30mm x 30mm)
- Légende bilingue: "رمز التحقق / Code de Vérification"

## URL de Vérification

### Format

```
{BaseUrl}/documents/{documentId}
```

### Exemples

**Production:**
```
https://verify.acadsign.ma/documents/a1b2c3d4-e5f6-7890-abcd-ef1234567890
```

**Développement:**
```
http://localhost:5000/documents/a1b2c3d4-e5f6-7890-abcd-ef1234567890
```

### Configuration

**Fichier:** `appsettings.json`

```json
{
  "VerificationPortal": {
    "BaseUrl": "http://localhost:5000"
  }
}
```

**Production:** `appsettings.Production.json`

```json
{
  "VerificationPortal": {
    "BaseUrl": "https://verify.acadsign.ma"
  }
}
```

## Sécurité

### UUID v4 Non-Prédictible

Chaque document reçoit un UUID v4 unique et non-prédictible (FR3):
- Format: `xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx`
- Exemple: `a1b2c3d4-e5f6-7890-abcd-ef1234567890`
- Impossible à deviner ou énumérer

### Pas d'Informations Sensibles

L'URL ne contient **aucune information sensible**:
- ✅ Seulement l'UUID du document
- ❌ Pas de nom d'étudiant
- ❌ Pas de CIN/CNE
- ❌ Pas de données académiques

### Endpoint Public

L'endpoint de vérification est **public** (pas d'authentification requise):
- Accessible à tous (recruteurs, universités, etc.)
- Rate limiting appliqué pour prévenir les abus
- Logging des accès pour audit

## Niveau de Correction d'Erreur

### QRCoder ECCLevel

**Medium (M) - Choisi** ✅
- Correction: ~15% de données
- Bon équilibre entre taille et robustesse
- Permet de scanner même si légèrement endommagé
- Taille raisonnable pour embedding dans PDF

**Autres niveaux disponibles:**
- **L (Low)**: ~7% de correction
- **Q (Quartile)**: ~25% de correction
- **H (High)**: ~30% de correction

## Utilisation

### Génération Automatique

Les QR codes sont générés automatiquement lors de la création d'un document:

```csharp
POST /api/v1/documents/generate
{
  "documentType": "AttestationScolarite",
  "studentId": "uuid",
  "studentData": { ... }
}
```

**Réponse:**
```json
{
  "documentId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "status": "UNSIGNED",
  "unsignedPdfUrl": "/api/v1/documents/{id}/unsigned",
  "createdAt": "2026-03-05T09:00:00Z"
}
```

### Vérification par Scan

1. **Scanner le QR code** avec un smartphone
2. **Redirection automatique** vers le portail de vérification
3. **Affichage des informations** du document:
   - Type de document
   - Nom de l'étudiant
   - Statut (signé/non signé)
   - Date d'émission
   - Signature numérique (si signée)

## Exemple de Code

### Génération Manuelle

```csharp
var qrCodeService = serviceProvider.GetRequiredService<IQrCodeService>();
var documentId = Guid.NewGuid();
var verificationUrl = $"https://verify.acadsign.ma/documents/{documentId}";

var qrCodeBytes = qrCodeService.GenerateQrCode(verificationUrl);

// Sauvegarder en fichier
await File.WriteAllBytesAsync("qr-code.png", qrCodeBytes);
```

### Intégration dans PDF

```csharp
// Automatiquement géré par PdfGenerationService
var pdfBytes = await pdfService.GenerateDocumentAsync(
    DocumentType.AttestationScolarite,
    studentData);

// Le QR code est déjà intégré dans le PDF
```

## Tests

### Test de Génération

```csharp
[Test]
public void GenerateQrCode_ValidUrl_ReturnsQrCodeBytes()
{
    var service = new QrCodeService();
    var url = "https://verify.acadsign.ma/documents/12345";
    
    var qrCodeBytes = service.GenerateQrCode(url);
    
    qrCodeBytes.Should().NotBeNull();
    qrCodeBytes.Length.Should().BeGreaterThan(0);
    
    // Vérifier que c'est un PNG
    var pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
    qrCodeBytes.Take(4).Should().Equal(pngHeader);
}
```

### Test de Scan

1. Générer un document PDF
2. Scanner le QR code avec un smartphone
3. Vérifier que l'URL décodée est correcte
4. Vérifier que le portail de vérification s'affiche

## Conformité

✅ **FR3**: UUID v4 unique pour chaque document  
✅ **FR4**: QR codes embedded in documents  
✅ **NFR-S1**: Pas d'informations sensibles dans l'URL  
✅ **NFR-U1**: Facile à scanner avec smartphone

## Évolutions Futures

- **Story 3.4**: Stockage MinIO S3 avec métadonnées
- **Story 3.5**: Pre-signed URLs pour accès sécurisé
- **Portail de vérification**: Interface web publique
- **Analytics**: Statistiques de vérification

## Références

- **Architecture**: `_bmad-output/planning-artifacts/architecture.md`
- **Story 3.3**: `_bmad-output/implementation-artifacts/3-3-implementer-generation-et-embedding-de-qr-codes.md`
- **QRCoder Documentation**: https://github.com/codebude/QRCoder
