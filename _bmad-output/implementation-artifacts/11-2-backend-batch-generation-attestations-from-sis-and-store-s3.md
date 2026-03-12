# Story 11.2: Générer en masse les attestations depuis SIS et stocker les PDFs non signés en S3/MinIO

Status: done

## Story

As a **registrar staff (Desktop App)**,
I want **déclencher la génération d’attestations PDF non signées à partir du flux SIS**,
so that **les PDFs apparaissent dans la Desktop App comme documents en attente (UNSIGNED) et peuvent ensuite être signés via le flux existant**.

## Acceptance Criteria

1. **Given** le backend a accès au flux SIS (Story 11.1)
   **When** un utilisateur Registrar/Admin déclenche la génération
   **Then** le backend:
   - itère sur chaque étudiant du JSON
   - génère un PDF (type `AttestationScolarite` ou `AttestationInscription` selon règle définie)
   - stocke chaque PDF **non signé** dans MinIO (S3 compatible)
   - crée une entrée `Document` en DB avec `Status = "UNSIGNED"` et `S3ObjectPath`.

2. **Given** un étudiant invalide (champs requis absents)
   **When** le batch est exécuté
   **Then** le batch continue pour les autres étudiants et l’erreur est comptabilisée et retournée dans la réponse.

3. **Given** un lot de N étudiants
   **When** le traitement se termine
   **Then** la réponse contient:
   - `total`, `generated`, `failed`
   - une liste `failures[]` (apogee/nom/prenom + cause)
   - une liste `createdDocumentIds[]` (UUID `PublicId`).

4. **Given** la Desktop App utilise déjà `GET /api/v1/documents/pending` + `GET /api/v1/documents/{id}/download`
   **When** les documents sont générés
   **Then** ils deviennent accessibles immédiatement dans ces endpoints (après refresh côté Desktop).

## Tasks / Subtasks

- [x] Task 1: Définir la règle de mapping SIS → `StudentData` (AC: 1)
  - [x] Subtask 1.1: `nom/prenom/apogee/filiere/...` → `StudentData` (au minimum: FR fields)
  - [x] Subtask 1.2: Valeurs par défaut pour champs non fournis (AR fields vides si non disponibles)

- [x] Task 2: Implémenter une orchestration de génération batch (AC: 1,2,3)
  - [x] Subtask 2.1: Ajouter un service applicatif `IAttestationBatchGenerationService`
  - [x] Subtask 2.2: Pour chaque étudiant: appeler `IPdfGenerationService.GenerateDocumentAsync(...)`
  - [x] Subtask 2.3: Upload vers `IS3StorageService.UploadDocumentAsync(...)`
  - [x] Subtask 2.4: Persister `Document` en DB (PublicId, StudentId, DocumentType, Status, S3ObjectPath)

- [x] Task 3: Exposer un endpoint de déclenchement (AC: 1,3)
  - [x] Subtask 3.1: Endpoint `POST /api/v1/admin/attestations/generate-from-sis` (Admin/Registrar)
  - [x] Subtask 3.2: Retourner un JSON de résultat batch

- [x] Task 4: Tests (AC: 1,2,3)
  - [x] Subtask 4.1: Unit tests sur l’orchestration (succès/échecs)
  - [ ] Subtask 4.2: (Option) Integration test avec MinIO TestContainers si déjà utilisé dans repo

## Dev Notes

### Existant à réutiliser

- Génération PDF: `AcadSign.Backend/src/Infrastructure/Pdf/PdfGenerationService.cs` (QuestPDF) [Source]
- Stockage S3/MinIO: `AcadSign.Backend/src/Infrastructure/Storage/S3StorageService.cs` [Source]
- Entité `Document`: `AcadSign.Backend/src/Domain/Entities/Document.cs` [Source]
- Desktop flow de signature/lot: la Desktop App télécharge le PDF via URL, signe localement, puis upload via `POST /api/v1/documents/{id}/signed` [Source: `AcadSign.Backend/src/Web/Endpoints/Documents.cs#UploadSignedDocument` + `AcadSign.Desktop/Services/Batch/BatchSigningService.cs`]

### Point d’attention "template"

Le besoin mentionne un fond graphique `Attestations/`.
- Si ces templates doivent être appliqués *strictement* comme fond (positions fixes), prévoir Story 11.6 (rendu à partir template fond).
- Pour démarrer, utiliser le rendu QuestPDF existant (cohérent avec les epics existantes) puis itérer.

### References

- `AcadSign.Backend/src/Web/Endpoints/Documents.cs`
- `AcadSign.Backend/src/Infrastructure/Pdf/PdfGenerationService.cs`
- `AcadSign.Backend/src/Infrastructure/Storage/S3StorageService.cs`

## Dev Agent Record

### Agent Model Used

Cascade

### Debug Log References

### Completion Notes List

- Implémentation du service batch `IAttestationBatchGenerationService` (SIS -> PDF -> S3 -> DB) avec reporting `total/generated/failed/failures/createdDocumentIds`.
- Ajout endpoint admin `POST /api/v1/admin/attestations/generate-from-sis` (roles Administrator/Registrar).
- Remplacement du stub `GET /api/v1/documents/pending` par une requête DB (Documents UNSIGNED + nom étudiant depuis Students).
- Enrichissement des erreurs parse SIS (nom/prenom/apogee/filiere) et ajustement du test parser.

### File List

- AcadSign.Backend/src/Application/Common/Models/SisAttestationBatchGenerationResult.cs
- AcadSign.Backend/src/Application/Models/SisAttestationExportModels.cs
- AcadSign.Backend/src/Application/Services/SisAttestationExportParser.cs
- AcadSign.Backend/src/Application/Services/IAttestationBatchGenerationService.cs
- AcadSign.Backend/src/Application/Services/AttestationBatchGenerationService.cs
- AcadSign.Backend/src/Infrastructure/DependencyInjection.cs
- AcadSign.Backend/src/Web/Controllers/AttestationsAdminController.cs
- AcadSign.Backend/src/Web/Endpoints/Documents.cs
- AcadSign.Backend/tests/Application.UnitTests/API/SisAttestationExportParserTests.cs
