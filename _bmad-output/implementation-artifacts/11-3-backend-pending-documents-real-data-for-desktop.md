# Story 11.3: Alimenter la liste Desktop avec les vrais documents en attente (UNSIGNED) depuis la DB

Status: review

## Story

As a **registrar staff (Desktop App)**,
I want **voir la liste réelle des documents non signés en attente dans la MainWindow**,
so that **je peux sélectionner et signer (simple ou batch) les attestations générées**.

## Acceptance Criteria

1. **Given** des `Document` existent en DB
   **When** le Desktop appelle `GET /api/v1/documents/pending`
   **Then** l’API retourne une liste de `DocumentDto` alimentée par la DB (et non un stub).

2. **Given** un document a `Status = "UNSIGNED"` ou `"PENDING"`
   **When** la liste est demandée
   **Then** le document apparaît dans la réponse.

3. **Given** un document passe à `Status = "SIGNED"` après upload
   **When** le Desktop rafraîchit la liste
   **Then** le document n’apparaît plus dans la liste "pending" (ou est filtré selon stratégie déjà existante côté UI).

4. La réponse inclut au minimum:
   - `Id` (PublicId guid)
   - `StudentName`
   - `DocumentType`
   - `CreatedAt`
   - `Status`
   - (Option) `StudentId`, `Cin`, `Program`, `Level`, `Reference` si disponibles

## Tasks / Subtasks

- [x] Task 1: Remplacer le stub `GetPendingDocuments` (AC: 1,2)
  - [x] Subtask 1.1: Query DB `Documents` filtrés sur status pending
  - [x] Subtask 1.2: Mapper vers `AcadSign.Desktop.Models.DocumentDto` contract (shape JSON)

- [x] Task 2: Garantir compatibilité avec preview Desktop (AC: 3)
  - [x] Subtask 2.1: Vérifier que `GET /api/v1/documents/{id}/download` reste utilisable (dev raw vs presigned en prod)

- [x] Task 3: Tests (AC: 1,2,3)
  - [x] Subtask 3.1: Tests unitaires de mapping
  - [x] Subtask 3.2: Test fonctionnel: create doc UNSIGNED en DB → endpoint returns it

## Dev Notes

- Endpoint actuel: `AcadSign.Backend/src/Web/Endpoints/Documents.cs#GetPendingDocuments` retourne un stub [Source]
- Desktop utilise `IAcadSignApi.GetPendingDocumentsAsync()` [Source: `AcadSign.Desktop/Services/Api/IAcadSignApi.cs`]
- Le download Desktop passe par `GET /api/v1/documents/{id}/download` puis HTTP GET de l’URL retournée [Source: `AcadSign.Desktop/Services/Api/RefitApiClientService.cs`]

## Dev Agent Record

### Agent Model Used

Cascade

### Debug Log References

- `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter "FullyQualifiedName~PendingDocumentDtoMapperTests|FullyQualifiedName~DocumentDownloadEndpointCompatibilityTests"`
- `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter GetPendingDocumentsTests` (tests ignorés si Docker indisponible via Testcontainers)
- `dotnet test AcadSign.Backend.slnx --no-build` (échec hors périmètre story sur `P0_042_JobStatusPolling_ReturnsProgress`)

### Completion Notes List

- Endpoint `GET /api/v1/documents/pending` branché sur DB réelle avec filtre `UNSIGNED` + `PENDING` et tri desc sur date de création.
- Mapping centralisé vers `PendingDocumentDto` aligné avec le contrat Desktop (`Id`, `StudentName`, `DocumentType`, `CreatedAt`, `Status` + champs optionnels).
- Compatibilité preview conservée pour `GET /api/v1/documents/{id}/download` en environnement Development (retour URL `/raw`).
- Tests ajoutés: mapping unitaire, compatibilité download endpoint, et scénario fonctionnel pending list (incluant exclusion après passage à `SIGNED`).

### File List

- `AcadSign.Backend/src/Application/Common/Models/PendingDocumentDto.cs`
- `AcadSign.Backend/src/Web/Endpoints/Documents.cs`
- `AcadSign.Backend/tests/Application.UnitTests/API/PendingDocumentDtoMapperTests.cs`
- `AcadSign.Backend/tests/Application.UnitTests/API/DocumentDownloadEndpointCompatibilityTests.cs`
- `AcadSign.Backend/tests/Application.UnitTests/Application.UnitTests.csproj`
- `AcadSign.Backend/tests/Application.FunctionalTests/Testing.cs`
- `AcadSign.Backend/tests/Application.FunctionalTests/Documents/Queries/GetPendingDocumentsTests.cs`

## Change Log

- 2026-03-10: Implémentation story 11.3 (pending documents DB-backed + mapping DTO Desktop + tests unitaires/fonctionnels)
