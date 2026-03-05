# Génération de PDF Bilingue avec QuestPDF - Guide d'Utilisation

## Vue d'ensemble

Ce document explique comment utiliser le système de génération de PDF bilingue (Arabe/Français) dans AcadSign Backend API en utilisant **QuestPDF**.

Le système permet de générer 4 types de documents académiques officiels:
- Attestation de Scolarité
- Relevé de Notes
- Attestation de Réussite
- Attestation d'Inscription

## Configuration

### Package NuGet

```xml
<PackageReference Include="QuestPDF" Version="2024.12.3" />
```

### Licence QuestPDF

```csharp
QuestPDF.Settings.License = LicenseType.Community;
```

**Pour AcadSign (université)**: Community License acceptable

### Enregistrement dans DI

```csharp
builder.Services.AddScoped<IPdfGenerationService, PdfGenerationService>();
```

## Utilisation

### Générer un Document PDF

```csharp
public class DocumentController : ControllerBase
{
    private readonly IPdfGenerationService _pdfService;
    
    [HttpGet("attestation-scolarite/{studentId}")]
    public async Task<IActionResult> GenerateAttestationScolarite(Guid studentId)
    {
        var studentData = await GetStudentData(studentId);
        
        var pdfBytes = await _pdfService.GenerateDocumentAsync(
            DocumentType.AttestationScolarite, 
            studentData
        );
        
        return File(pdfBytes, "application/pdf", "attestation-scolarite.pdf");
    }
}
```

### Types de Documents

```csharp
public enum DocumentType
{
    AttestationScolarite,    // شهادة مدرسية
    ReleveNotes,             // كشف النقاط
    AttestationReussite,     // شهادة نجاح
    AttestationInscription   // شهادة تسجيل
}
```

## Structure du PDF

### En-tête
- Logo de l'université
- Titre bilingue (arabe + français)

### Contenu
- Informations étudiant (nom AR/FR, CIN, CNE)
- Date de naissance
- Programme et faculté
- Contenu spécifique au document

### Pied de page
- Date d'émission
- QR Code (placeholder - Story 3.3)

## Performance

✅ **NFR-P1**: Génération < 3 secondes  
✅ **Taille fichier**: < 500 KB

## Évolutions Futures

- **Story 3.2**: Templates détaillés pour chaque type de document
- **Story 3.3**: QR codes avec données sécurisées
- **Story 3.4**: Stockage MinIO S3
- **Story 3.5**: Pre-signed URLs

## Références

- **QuestPDF**: https://www.questpdf.com/
- **Architecture**: `_bmad-output/planning-artifacts/architecture.md`
- **Story 3.1**: `_bmad-output/implementation-artifacts/3-1-configurer-questpdf-pour-generation-pdf-bilingue.md`
