# Story 11.4: Ajouter le bouton Desktop pour déclencher la génération des attestations et rafraîchir la liste

Status: review

## Story

As a **registrar staff**,
I want **un bouton dans la MainWindow pour lancer la génération d’attestations depuis SIS**,
so that **je peux générer puis voir immédiatement les documents en attente à signer dans la liste**.

## Acceptance Criteria

1. **Given** la MainWindow est ouverte
   **When** je clique sur le bouton "Générer attestations" (dans `MainView.xaml`)
   **Then** la Desktop App appelle l’API backend (Story 11.2) et affiche un statut "en cours".

2. **Given** la génération se termine
   **When** l’API répond
   **Then**:
   - la Desktop App affiche `total/generated/failed`
   - puis déclenche un refresh `LoadDocumentsUiCommand`.

3. **Given** des erreurs de génération
   **When** la réponse contient des `failures[]`
   **Then** elles sont affichées dans l’UI (au minimum via `StatusText`, ou une zone dédiée) sans bloquer l’app.

## Tasks / Subtasks

- [x] Task 1: UI - Ajouter le bouton dans `AcadSign.Desktop/Views/MainView.xaml` (AC: 1)
  - [x] Subtask 1.1: Ajouter un `Button` dans la toolbar (barre supérieure) avec `Command={Binding GenerateAttestationsFromSisCommand}`

- [x] Task 2: ViewModel - Ajouter la commande dans `AcadSign.Desktop/ViewModels/MainViewModel.cs` (AC: 1,2,3)
  - [x] Subtask 2.1: Créer `[RelayCommand] GenerateAttestationsFromSisAsync()`
  - [x] Subtask 2.2: Gérer `IsLoading` + `StatusText`
  - [x] Subtask 2.3: Appeler le nouveau endpoint via `IApiClientService`

- [x] Task 3: API Client Desktop - Étendre `IAcadSignApi` + `IApiClientService` (AC: 1)
  - [x] Subtask 3.1: Ajouter méthode Refit `POST /api/v1/admin/attestations/generate-from-sis`
  - [x] Subtask 3.2: Ajouter DTO `AttestationBatchGenerationResponse`

## Dev Notes

- Le fichier UI cible: `AcadSign.Desktop/Views/MainView.xaml` (toolbar déjà présente avec `LoadDocumentsUiCommand`, `StartBatchSigningCommand`, etc.) [Source: `AcadSign.Desktop/Views/MainView.xaml`]
- Le ViewModel central: `AcadSign.Desktop/ViewModels/MainViewModel.cs` [Source]
- Le client API Refit: `AcadSign.Desktop/Services/Api/IAcadSignApi.cs` + `RefitApiClientService.cs` [Source]

## Dev Agent Record

### Agent Model Used

Cascade

### Debug Log References

- `dotnet build AcadSign.Desktop/AcadSign.Desktop.csproj`

### Completion Notes List

- Bouton `🧾 Générer attestations` ajouté dans la toolbar de `MainView.xaml` et lié à `GenerateAttestationsFromSisCommand`.
- Nouvelle commande `GenerateAttestationsFromSisAsync` ajoutée au `MainViewModel` avec statut "en cours", appel API, affichage `total/generated/failed`, affichage des erreurs `failures[]` via `StatusText`, puis refresh de la liste via `LoadDocumentsUiCommand`.
- Extension du client API Desktop: nouveau endpoint Refit `POST /api/v1/admin/attestations/generate-from-sis`, DTO de réponse batch, propagation via `IApiClientService`, implémentations `RefitApiClientService` et `ApiClientService`.
- Build Desktop valide; aucun test Desktop dédié présent dans le repository actuellement.

### File List

- `AcadSign.Desktop/Views/MainView.xaml`
- `AcadSign.Desktop/ViewModels/MainViewModel.cs`
- `AcadSign.Desktop/Services/Api/IAcadSignApi.cs`
- `AcadSign.Desktop/Services/Api/IApiClientService.cs`
- `AcadSign.Desktop/Services/Api/RefitApiClientService.cs`
- `AcadSign.Desktop/Services/Api/ApiClientService.cs`

## Change Log

- 2026-03-10: Implémentation story 11.4 (bouton génération attestations, commande ViewModel, extension API client Desktop, refresh automatique de la liste)
