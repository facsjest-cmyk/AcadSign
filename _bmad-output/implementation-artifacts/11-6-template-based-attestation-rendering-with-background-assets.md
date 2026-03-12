# Story 11.6: Rendu d’attestation basé sur template (fond graphique) pour obtenir un PDF "semblable au modèle"

Status: review

## Story

As a **registrar staff**,
I want **que l’attestation générée respecte un template graphique (fond) fourni dans le projet**,
so that **le rendu PDF final ressemble aux attestations officielles existantes**.

## Acceptance Criteria

1. **Given** un template graphique présent dans le repo (dossier `Attestations/`)
   **When** une attestation est générée
   **Then** le PDF utilise ce template comme fond et place les champs (nom, prénom, apogee, filière, etc.) aux positions prévues.

2. **Given** plusieurs templates
   **When** la génération choisit un template
   **Then** la règle de sélection est explicite (ex: par filière / type / institution), configurable.

3. **Given** template absent/invalide
   **When** la génération est demandée
   **Then** fallback vers le rendu QuestPDF "standard" existant, avec un warning log.

## Tasks / Subtasks

- [x] Task 1: Définir le format template supporté (AC: 1)
  - [x] Subtask 1.1: Décider: image (jpg/png) en background vs PDF template
  - [x] Subtask 1.2: Définir un mapping de champs → positions (config JSON)

- [x] Task 2: Implémenter le rendu template-based (AC: 1,3)
  - [x] Subtask 2.1: Ajouter un service `IAttestationTemplateRenderer`
  - [x] Subtask 2.2: Générer PDF: background + overlay textes
  - [x] Subtask 2.3: Intégrer dans `IPdfGenerationService` uniquement pour le type attestation

- [x] Task 3: Tests (AC: 1,3)
  - [x] Subtask 3.1: Test génération avec template présent (validation taille non nulle)
  - [x] Subtask 3.2: Test fallback sans template

## Dev Notes

- Génération actuelle est entièrement QuestPDF dans `PdfGenerationService` [Source: `AcadSign.Backend/src/Infrastructure/Pdf/PdfGenerationService.cs`]
- Le projet possède aussi iText côté Desktop pour la signature. Si on choisit template PDF et overlay, iText côté Backend pourrait être envisagé, mais cela introduit une nouvelle dépendance backend; privilégier d’abord QuestPDF si possible.

## Dev Agent Record

### Agent Model Used

Cascade

### Debug Log References

- `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter AttestationTemplateRendererTests`
- `dotnet build src/Infrastructure/Infrastructure.csproj`
- `dotnet build src/Web/Web.csproj`
- `dotnet build AcadSign.Backend.slnx` (échec hors périmètre: verrou fichier coverage Domain.UnitTests)

### Completion Notes List

- Format retenu: templates **image** (`jpg/jpeg/png`) en fond + overlay de texte via QuestPDF (pas de dépendance iText backend ajoutée).
- Règles de sélection de template configurables via `Attestations/template-layouts.json` (par `documentType`, `priority`, et filtres optionnels `programContains/facultyContains/institutionContains`).
- Mapping JSON champs→positions implémenté (`field`, `x`, `y`, `fontSize`, `bold`, `prefix`) avec rendu overlay et fallback automatique.
- Service `IAttestationTemplateRenderer` ajouté et intégré à `PdfGenerationService` uniquement pour les types attestation (`AttestationScolarite`, `AttestationReussite`, `AttestationInscription`).
- En cas de template absent/invalide/erreur, warning log + fallback vers rendu QuestPDF standard existant.
- Tests ajoutés: génération template présente (PDF non vide), mapping absent (retour null), fallback `PdfGenerationService` quand renderer renvoie null.

### File List

- `AcadSign.Backend/src/Application/Common/Interfaces/IAttestationTemplateRenderer.cs`
- `AcadSign.Backend/src/Infrastructure/Pdf/AttestationTemplateRenderer.cs`
- `AcadSign.Backend/src/Infrastructure/Pdf/PdfGenerationService.cs`
- `AcadSign.Backend/src/Infrastructure/DependencyInjection.cs`
- `Attestations/template-layouts.json`
- `AcadSign.Backend/tests/Application.UnitTests/PdfGeneration/AttestationTemplateRendererTests.cs`

## Change Log

- 2026-03-10: Implémentation story 11.6 (rendu attestation template-based avec fond graphique, règles configurables et fallback standard)
