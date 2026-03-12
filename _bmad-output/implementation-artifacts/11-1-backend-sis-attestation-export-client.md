# Story 11.1: Implémenter le client Backend pour l’export d’attestations (SIS)

Status: ready-for-dev

## Story

As a **registrar staff (Desktop App)**,
I want **que le backend puisse récupérer le flux JSON d’étudiants depuis l’API SIS d’export d’attestations**,
so that **je peux générer en masse des attestations non signées et les signer ensuite dans le flux existant**.

## Acceptance Criteria

1. **Given** l’URL SIS `http://10.2.22.201/api/v1/admin/attestation/export`
   **When** le backend appelle l’endpoint en GET
   **Then** le backend récupère un JSON et le parse en une liste typée (au minimum: `nom`, `prenom`, `apogee`, `filiere`, plus champs additionnels si présents).

2. **Given** des champs manquants ou inconnus dans le JSON
   **When** le mapping est exécuté
   **Then** le backend n’échoue pas globalement:
   - les champs inconnus sont ignorés
   - les champs requis manquants créent un item en erreur (diagnostic exploitable).

3. **Given** l’API SIS est indisponible (timeout / 5xx / non-JSON)
   **When** l’appel échoue
   **Then** une erreur claire est loggée avec correlation id et un message métier est renvoyé au caller.

## Tasks / Subtasks

- [x] Task 1: Créer les modèles DTO de l’export SIS (AC: 1)
  - [x] Subtask 1.1: Ajouter `SisAttestationStudentDto` (nom, prenom, apogee, filiere, etc.)
  - [x] Subtask 1.2: Ajouter un modèle « extensible » (dictionnaire) pour champs additionnels, sans casser la désérialisation

- [x] Task 2: Implémenter un client HTTP typé côté Backend (AC: 1,2,3)
  - [x] Subtask 2.1: Créer `ISisAttestationExportClient` + implémentation (HttpClientFactory)
  - [x] Subtask 2.2: Configurer timeout, retry léger (Polly si déjà en place côté backend sinon simple)
  - [x] Subtask 2.3: Logger les erreurs (ne pas logger de PII brute)

- [x] Task 3: Tests unitaires de parsing/mapping (AC: 1,2)
  - [x] Subtask 3.1: Cas nominal (champs attendus)
  - [x] Subtask 3.2: Champs inconnus
  - [x] Subtask 3.3: Champs requis manquants

## Dev Notes

### Intégration existante à respecter

- Stockage S3/MinIO déjà disponible via `AcadSign.Backend/src/Infrastructure/Storage/S3StorageService.cs` (PutObject/Presigned URL) [Source: `AcadSign.Backend/src/Infrastructure/Storage/S3StorageService.cs`]
- Génération PDF existante via QuestPDF dans `AcadSign.Backend/src/Infrastructure/Pdf/PdfGenerationService.cs` [Source: `AcadSign.Backend/src/Infrastructure/Pdf/PdfGenerationService.cs`]
- Endpoint Desktop existant pour afficher les documents en attente: `GET /api/v1/documents/pending` (actuellement stub) [Source: `AcadSign.Backend/src/Web/Endpoints/Documents.cs#GetPendingDocuments`]

### Contraintes

- Ne pas exposer directement l’URL SIS au Desktop si on peut l’éviter: le Desktop doit rester client du Backend.
- Préparer la donnée pour la génération de document de type attestation (voir Story 11.2).

### References

- `AcadSign.Backend/src/Web/Endpoints/Documents.cs`
- `AcadSign.Backend/src/Infrastructure/Storage/S3StorageService.cs`
- `AcadSign.Backend/src/Infrastructure/Pdf/PdfGenerationService.cs`
- `AcadSign.Backend/src/Application/Common/Models/StudentData.cs`

## Dev Agent Record

### Agent Model Used

Cascade

### Debug Log References

### Completion Notes List

- Implémentation des modèles SIS avec `JsonExtensionData` pour capturer les champs additionnels.
- Ajout d’un parser dédié `SisAttestationExportParser` qui retourne un `SisAttestationExportResult` (items valides + erreurs itemisées pour champs requis manquants).
- Implémentation `SisAttestationExportClient` via `IHttpClientFactory` avec timeout par tentative, retry léger, et logs d’erreur incluant `CorrelationId` (sans PII brute).
- Configuration ajoutée dans `appsettings.json` (`Sis:AttestationExportUrl`, `Sis:TimeoutSeconds`, `Sis:RetryCount`).
- Tests:
  - `dotnet test AcadSign.Backend/tests/Application.UnitTests/Application.UnitTests.csproj -c Release` OK.
  - `dotnet test AcadSign.Backend/AcadSign.Backend.slnx -c Release` OK (tests Docker ignorés si Docker indisponible).

### File List

- AcadSign.Backend/src/Application/Models/SisAttestationExportModels.cs
- AcadSign.Backend/src/Application/Interfaces/ISisAttestationExportClient.cs
- AcadSign.Backend/src/Application/Common/Exceptions/SisAttestationExportClientException.cs
- AcadSign.Backend/src/Application/Services/SisAttestationExportParser.cs
- AcadSign.Backend/src/Application/DependencyInjection.cs
- AcadSign.Backend/src/Infrastructure/Services/SisAttestationExportClient.cs
- AcadSign.Backend/src/Infrastructure/DependencyInjection.cs
- AcadSign.Backend/src/Web/appsettings.json
- AcadSign.Backend/tests/Application.UnitTests/API/SisAttestationExportParserTests.cs
