# Chiffrement PII (Personally Identifiable Information) - Guide d'Utilisation

## Vue d'ensemble

Ce document explique comment utiliser le système de chiffrement au niveau application pour protéger les données PII (Personally Identifiable Information) dans AcadSign Backend API.

Le chiffrement utilise **ASP.NET Data Protection API** avec **AES-256-GCM** pour garantir la conformité avec la **Loi 53-05 (CNDP)** marocaine sur la protection des données personnelles.

## Architecture du Chiffrement

### Composants Principaux

1. **IPiiEncryptionService** - Interface de chiffrement/déchiffrement
2. **PiiEncryptionService** - Implémentation utilisant Data Protection API
3. **EncryptedPropertyAttribute** - Attribut pour marquer les propriétés à chiffrer
4. **EncryptionInterceptor** - Intercepteur EF Core pour chiffrement automatique
5. **Data Protection Keys** - Clés stockées dans PostgreSQL avec rotation automatique

### Flux de Chiffrement

```
┌─────────────────┐
│  Application    │
│  Code           │
└────────┬────────┘
         │ Save Entity
         ▼
┌─────────────────────────┐
│ EncryptionInterceptor   │
│ (EF Core)               │
└────────┬────────────────┘
         │ Encrypt [EncryptedProperty]
         ▼
┌─────────────────────────┐
│ PiiEncryptionService    │
│ (AES-256-GCM)           │
└────────┬────────────────┘
         │ Store Encrypted
         ▼
┌─────────────────────────┐
│ PostgreSQL Database     │
│ (Encrypted Data)        │
└─────────────────────────┘
```

## Configuration

### 1. Data Protection API

La configuration est dans `src/Infrastructure/DependencyInjection.cs`:

```csharp
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<ApplicationDbContext>()
    .SetApplicationName("AcadSign")
    .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
    {
        EncryptionAlgorithm = EncryptionAlgorithm.AES_256_GCM,
        ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
    })
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90)); // Rotation tous les 90 jours
```

**Caractéristiques:**
- **Algorithme:** AES-256-GCM (Galois/Counter Mode)
- **Validation:** HMACSHA256
- **Stockage des clés:** PostgreSQL (table `DataProtectionKeys`)
- **Rotation:** Automatique tous les 90 jours
- **Application Name:** "AcadSign" (pour isolation des clés)

### 2. Service d'Encryption

Le service est enregistré comme singleton:

```csharp
builder.Services.AddSingleton<IPiiEncryptionService, PiiEncryptionService>();
```

### 3. Intercepteur EF Core

L'intercepteur est enregistré pour chiffrer automatiquement les propriétés marquées:

```csharp
builder.Services.AddScoped<ISaveChangesInterceptor, EncryptionInterceptor>();
```

## Utilisation

### Marquer les Propriétés à Chiffrer

Utilisez l'attribut `[EncryptedProperty]` sur les propriétés contenant des données PII:

```csharp
using AcadSign.Backend.Domain.Attributes;

public class Student : BaseEntity
{
    // Données chiffrées (PII)
    [EncryptedProperty]
    public string CIN { get; set; } = string.Empty; // Carte d'Identité Nationale

    [EncryptedProperty]
    public string CNE { get; set; } = string.Empty; // Code National Étudiant

    [EncryptedProperty]
    public string Email { get; set; } = string.Empty;

    [EncryptedProperty]
    public string? PhoneNumber { get; set; }

    // Données non chiffrées
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}
```

### Chiffrement Automatique

Le chiffrement est **automatique** lors de `SaveChanges()`:

```csharp
// Créer un étudiant
var student = new Student
{
    CIN = "AB123456",
    CNE = "R123456789",
    Email = "ahmed.benali@university.ma",
    PhoneNumber = "+212612345678",
    FirstName = "Ahmed",
    LastName = "Benali",
    DateOfBirth = new DateTime(2000, 5, 15)
};

// Sauvegarder - Les propriétés [EncryptedProperty] sont chiffrées automatiquement
await _context.Students.AddAsync(student);
await _context.SaveChangesAsync();

// En base de données:
// CIN = "CfDJ8Kx7..." (chiffré)
// CNE = "CfDJ8Lm9..." (chiffré)
// Email = "CfDJ8Np2..." (chiffré)
// PhoneNumber = "CfDJ8Qr5..." (chiffré)
// FirstName = "Ahmed" (clair)
// LastName = "Benali" (clair)
```

### Déchiffrement Automatique

Le déchiffrement est **automatique** lors de la lecture:

```csharp
// Récupérer un étudiant
var student = await _context.Students.FindAsync(studentId);

// Les propriétés sont automatiquement déchiffrées
Console.WriteLine(student.CIN);         // "AB123456" (déchiffré)
Console.WriteLine(student.Email);       // "ahmed.benali@university.ma" (déchiffré)
Console.WriteLine(student.FirstName);   // "Ahmed" (jamais chiffré)
```

### Utilisation Manuelle du Service

Si vous avez besoin de chiffrer/déchiffrer manuellement:

```csharp
public class MyService
{
    private readonly IPiiEncryptionService _encryptionService;

    public MyService(IPiiEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    public void Example()
    {
        // Chiffrer
        string plainText = "AB123456";
        string encrypted = _encryptionService.Encrypt(plainText);
        // encrypted = "CfDJ8Kx7..."

        // Déchiffrer
        string decrypted = _encryptionService.Decrypt(encrypted);
        // decrypted = "AB123456"
    }
}
```

## Données Chiffrées dans AcadSign

### Entités avec PII

| Entité | Propriétés Chiffrées | Raison |
|--------|---------------------|--------|
| **Student** | CIN, CNE, Email, PhoneNumber | Données personnelles sensibles |
| **User** (futur) | Email, PhoneNumber | Données de contact |
| **Document** (futur) | StudentCIN, StudentCNE | Références aux données PII |

### Données NON Chiffrées

Les données suivantes ne sont **pas** chiffrées car elles ne sont pas considérées comme PII sensibles:

- Noms et prénoms (FirstName, LastName)
- Dates de naissance (DateOfBirth)
- IDs techniques (Guid, InstitutionId)
- Métadonnées (Created, LastModified, CreatedBy)
- Données publiques (DocumentType, Status)

## Rotation des Clés

### Rotation Automatique

Les clés de chiffrement sont automatiquement rotées tous les **90 jours**:

1. Une nouvelle clé est générée automatiquement
2. Les nouvelles données sont chiffrées avec la nouvelle clé
3. Les anciennes clés sont conservées pour déchiffrer les données existantes
4. Les anciennes clés expirent après leur durée de vie (90 jours supplémentaires)

### Vérifier les Clés

```sql
-- Voir les clés de chiffrement
SELECT 
    "Id",
    "FriendlyName",
    "Xml"::text
FROM "DataProtectionKeys"
ORDER BY "Id" DESC;
```

### Rotation Manuelle (si nécessaire)

Si vous devez forcer une rotation de clés:

```bash
# Créer une nouvelle clé
dotnet run --project src/Web -- rotate-data-protection-keys
```

## Sécurité

### Protection en Profondeur

1. **Chiffrement au niveau application** - Même un DBA ne peut pas lire les données PII
2. **Clés séparées** - Les clés sont stockées dans une table dédiée
3. **AES-256-GCM** - Algorithme de chiffrement authentifié (AEAD)
4. **Rotation automatique** - Limite l'exposition en cas de compromission
5. **Validation HMAC** - Détection de modification des données

### Conformité CNDP (Loi 53-05)

✅ **Article 25** - Chiffrement des données sensibles  
✅ **Article 27** - Mesures de sécurité appropriées  
✅ **Article 30** - Protection contre l'accès non autorisé  

### Bonnes Pratiques

#### ✅ À Faire

- Marquer toutes les propriétés PII avec `[EncryptedProperty]`
- Utiliser le service `IPiiEncryptionService` pour chiffrement manuel
- Vérifier régulièrement la rotation des clés
- Logger les accès aux données PII (audit)
- Limiter l'accès aux données déchiffrées (RBAC)

#### ❌ À Éviter

- Ne jamais logger les données PII en clair
- Ne jamais exposer les données PII dans les URLs
- Ne jamais stocker les clés de chiffrement dans le code source
- Ne jamais désactiver le chiffrement en production
- Ne jamais partager les clés entre environnements

## Migrations de Base de Données

### Créer la Migration pour DataProtectionKeys

```bash
cd /Users/macbookpro/e-sign/AcadSign.Backend

# Créer la migration
dotnet ef migrations add AddDataProtectionKeys \
  --project src/Infrastructure \
  --startup-project src/Web \
  --output-dir Data/Migrations

# Appliquer la migration
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/Web
```

### Migration pour Student

```bash
# Créer la migration
dotnet ef migrations add AddStudentEntity \
  --project src/Infrastructure \
  --startup-project src/Web \
  --output-dir Data/Migrations

# Appliquer la migration
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/Web
```

## Dépannage

### Problème: CryptographicException lors du déchiffrement

**Erreur:**
```
InvalidOperationException: Unable to decrypt data. The encryption key may have changed.
```

**Causes possibles:**
1. Les clés de chiffrement ont été supprimées
2. La base de données a été restaurée sans les clés
3. L'application name a changé

**Solution:**
```sql
-- Vérifier que les clés existent
SELECT COUNT(*) FROM "DataProtectionKeys";

-- Si aucune clé, redémarrer l'application pour en générer
```

### Problème: Données non chiffrées en base

**Vérification:**
```sql
-- Vérifier si les données sont chiffrées (commencent par "CfDJ8")
SELECT "CIN", "CNE", "Email" 
FROM "Students" 
LIMIT 1;
```

**Solution:**
- Vérifier que `EncryptionInterceptor` est enregistré
- Vérifier que les propriétés ont l'attribut `[EncryptedProperty]`
- Redémarrer l'application

### Problème: Performance lente

**Symptôme:** Les requêtes sont lentes avec beaucoup de données chiffrées.

**Solution:**
- Le chiffrement/déchiffrement a un coût CPU
- Limiter le nombre d'entités chargées (pagination)
- Utiliser `AsNoTracking()` pour les lectures seules
- Indexer les colonnes non chiffrées pour les recherches

```csharp
// Bon: Pagination + AsNoTracking
var students = await _context.Students
    .AsNoTracking()
    .OrderBy(s => s.LastName)
    .Skip(page * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

## Exemples Complets

### Exemple 1: Créer un Étudiant avec Données Chiffrées

```csharp
public async Task<Guid> CreateStudentAsync(CreateStudentRequest request)
{
    var student = new Student
    {
        // Données PII - seront chiffrées automatiquement
        CIN = request.CIN,
        CNE = request.CNE,
        Email = request.Email,
        PhoneNumber = request.PhoneNumber,
        
        // Données non sensibles - restent en clair
        FirstName = request.FirstName,
        LastName = request.LastName,
        DateOfBirth = request.DateOfBirth,
        InstitutionId = request.InstitutionId
    };

    await _context.Students.AddAsync(student);
    await _context.SaveChangesAsync();

    return student.Id;
}
```

### Exemple 2: Rechercher par Données Chiffrées

⚠️ **Attention:** Vous ne pouvez pas rechercher directement sur des colonnes chiffrées avec SQL.

```csharp
// ❌ NE FONCTIONNE PAS - La recherche SQL ne peut pas déchiffrer
var student = await _context.Students
    .Where(s => s.CIN == "AB123456")
    .FirstOrDefaultAsync();

// ✅ SOLUTION 1: Charger toutes les données et filtrer en mémoire (petit dataset)
var students = await _context.Students.ToListAsync();
var student = students.FirstOrDefault(s => s.CIN == "AB123456");

// ✅ SOLUTION 2: Utiliser un index non chiffré (CNE hashé)
// Ajouter une colonne CNEHash (non chiffrée) pour la recherche
var cneHash = ComputeHash(request.CNE);
var student = await _context.Students
    .Where(s => s.CNEHash == cneHash)
    .FirstOrDefaultAsync();
```

### Exemple 3: Exporter des Données (Audit)

```csharp
public async Task<byte[]> ExportStudentDataAsync(Guid studentId)
{
    var student = await _context.Students.FindAsync(studentId);
    
    if (student == null)
        throw new NotFoundException(nameof(Student), studentId);

    // Les données sont automatiquement déchiffrées
    var exportData = new
    {
        student.CIN,        // Déchiffré
        student.CNE,        // Déchiffré
        student.Email,      // Déchiffré
        student.PhoneNumber,// Déchiffré
        student.FirstName,
        student.LastName,
        student.DateOfBirth
    };

    // Logger l'accès aux données PII
    _logger.LogWarning("PII data exported for student {StudentId} by user {UserId}", 
        studentId, _currentUser.Id);

    return JsonSerializer.SerializeToUtf8Bytes(exportData);
}
```

## Références

### Documentation Technique

- **ASP.NET Data Protection API:** https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/
- **AES-256-GCM:** https://en.wikipedia.org/wiki/Galois/Counter_Mode
- **CNDP Loi 53-05:** https://www.cndp.ma/

### Architecture AcadSign

- **Architecture Document:** `_bmad-output/planning-artifacts/architecture.md`
- **Story 2.5:** `_bmad-output/implementation-artifacts/2-5-configurer-chiffrement-pii-avec-asp-net-data-protection-api.md`

### Fichiers Source

- **Interface:** `src/Application/Common/Interfaces/IPiiEncryptionService.cs`
- **Service:** `src/Infrastructure/Security/PiiEncryptionService.cs`
- **Attribut:** `src/Domain/Attributes/EncryptedPropertyAttribute.cs`
- **Intercepteur:** `src/Infrastructure/Data/Interceptors/EncryptionInterceptor.cs`
- **Configuration:** `src/Infrastructure/DependencyInjection.cs`
- **Entité Exemple:** `src/Domain/Entities/Student.cs`
