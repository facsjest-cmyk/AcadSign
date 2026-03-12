# Story 11.5: Exporter localement les attestations générées dans /Generated_Attestations classé par date

Status: review

## Story

As a **registrar staff**,
I want **exporter localement les attestations générées (PDF) dans un dossier daté**,
so that **je peux archiver/partager localement tout en gardant le stockage officiel dans MinIO/S3**.

## Acceptance Criteria

1. **Given** une liste de documents en attente ou signés
   **When** je clique "Exporter attestations" dans la Desktop App
   **Then** l’application télécharge les PDFs (via `GET /api/v1/documents/{id}/download` puis GET URL) et les enregistre dans:
   - `Generated_Attestations/yyyy-MM-dd/`

2. **Given** un document correspond à un étudiant
   **When** le fichier est sauvegardé
   **Then** le nom du fichier suit:
   - `attestation_{nom}_{apogee}.pdf` (normalisé, sans caractères invalides)

3. **Given** un téléchargement échoue
   **When** l’export continue
   **Then** les autres documents sont exportés et l’erreur est visible dans l’UI.

## Tasks / Subtasks

- [x] Task 1: Ajouter un bouton d’export dans `MainView.xaml` (AC: 1)
  - [x] Subtask 1.1: Command `ExportGeneratedAttestationsCommand`

- [x] Task 2: Implémenter l’export dans `MainViewModel.cs` (AC: 1,2,3)
  - [x] Subtask 2.1: Choisir la source: documents sélectionnés (`IsSelected`) ou tous les `FilteredDocuments`
  - [x] Subtask 2.2: Créer le dossier `Generated_Attestations/<date>` (base: répertoire de l’app)
  - [x] Subtask 2.3: Télécharger les bytes via `IApiClientService.DownloadDocumentAsync(documentId)`
  - [x] Subtask 2.4: Générer un nom de fichier sûr (sans accents, sans `/\\:*?"<>|`)

- [x] Task 3: Affichage UI des résultats (AC: 1,3)
  - [x] Subtask 3.1: StatusText: compteur exportés/échoués
  - [x] Subtask 3.2: (Option) ouvrir le dossier à la fin

## Dev Notes

- Le Desktop sait déjà télécharger un PDF pour preview: `MainViewModel.LoadPreviewAsync` utilise `IApiClientService.DownloadDocumentAsync` [Source: `AcadSign.Desktop/ViewModels/MainViewModel.cs`]
- Le download côté Desktop passe par `GET /api/v1/documents/{id}/download` [Source: `AcadSign.Desktop/Services/Api/IAcadSignApi.cs`]

## Dev Agent Record

### Agent Model Used

Cascade

### Debug Log References

- `dotnet build AcadSign.Desktop/AcadSign.Desktop.csproj`

### Completion Notes List

- Bouton toolbar remplacé en `📁 Exporter attestations` et lié à `ExportGeneratedAttestationsCommand`.
- Export implémenté avec stratégie source: documents sélectionnés (`IsSelected`) sinon tous les `FilteredDocuments`, puis filtrage sur statuts pending/signed.
- Dossier local créé sous `Generated_Attestations/yyyy-MM-dd` (base `AppContext.BaseDirectory`).
- Téléchargement réalisé via `IApiClientService.DownloadDocumentAsync(documentId)` pour chaque document.
- Nom de fichier normalisé au format `attestation_{nom}_{apogee}.pdf` (sans accents, sans caractères invalides, unicité gérée avec suffixe).
- L’export continue en cas d’échec individuel; UI affiche compteur exportés/échoués + détails d’erreur (partiels) dans `StatusText`.
- Ouverture automatique du dossier d’export en fin de traitement si au moins un fichier exporté.

### File List

- `AcadSign.Desktop/Views/MainView.xaml`
- `AcadSign.Desktop/ViewModels/MainViewModel.cs`

## Change Log

- 2026-03-10: Implémentation story 11.5 (export local daté des attestations, nommage sécurisé, tolérance aux erreurs et reporting UI)
