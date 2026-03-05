---
stepsCompleted: ['step-01-validate-prerequisites', 'step-02-design-epics', 'step-03-create-stories']
inputDocuments: 
  - '/Users/macbookpro/e-sign/_bmad-output/planning-artifacts/prd.md'
  - '/Users/macbookpro/e-sign/_bmad-output/planning-artifacts/architecture.md'
workflowStatus: 'complete'
currentStep: 'completed'
totalEpics: 10
totalStories: 48
---

# AcadSign - Epic Breakdown

## Overview

This document provides the complete epic and story breakdown for AcadSign, decomposing the requirements from the PRD and Architecture into implementable stories.

## Requirements Inventory

### Functional Requirements

**FR1:** Registrar staff can generate official academic documents (attestation de scolarité, relevé de notes, attestation de réussite, attestation d'inscription) from student data

**FR2:** System can populate document templates with student data in bilingual format (Arabic/French)

**FR3:** System can generate unique, non-predictable document identifiers (UUID v4) for each document

**FR4:** System can embed QR codes in generated documents linking to verification portal

**FR5:** Registrar staff can generate documents in batch mode (up to 500 documents per batch)

**FR6:** System can track batch processing status (total, processed, failed documents)

**FR7:** Registrar staff can retrieve unsigned PDF documents for signing

**FR8:** System can store signed documents in S3-compatible storage with encryption

**FR9:** System can generate pre-signed download URLs with expiration (1 hour validity)

**FR10:** Students can download their signed documents via secure download links

**FR11:** System can send email notifications to students with document download links

**FR12:** Registrar staff can sign PDF documents using USB dongle (Barid Al-Maghrib Class 3 certificate)

**FR13:** Desktop application can detect and access USB dongle via PKCS#11 or Windows CSP

**FR14:** Desktop application can prompt for PIN code to unlock USB dongle

**FR15:** Desktop application can apply PAdES signature format to PDF documents

**FR16:** System can validate certificate status (VALID, EXPIRED, REVOKED) via OCSP/CRL

**FR17:** System can apply RFC 3161 timestamping to signatures for non-repudiation

**FR18:** Desktop application can upload signed PDFs to backend API

**FR19:** System can handle signature failures and retry logic

**FR20:** Desktop application can display batch signing progress and status

**FR21:** Public users can verify document authenticity by scanning QR code or entering document ID

**FR22:** Verification portal can validate electronic signature cryptographically

**FR23:** Verification portal can display document metadata (type, issuing institution, student name, signature date)

**FR24:** Verification portal can display certificate status (VALID, EXPIRED, REVOKED)

**FR25:** Verification portal can display certificate validity period

**FR26:** System can verify signatures without requiring authentication (public endpoint)

**FR27:** System can authenticate registrar staff via OAuth 2.0 Authorization Code with PKCE

**FR28:** System can authenticate SIS Laravel via OAuth 2.0 Client Credentials

**FR29:** System can issue JWT access tokens (1 hour validity) and refresh tokens (7 days validity)

**FR30:** System can enforce role-based access control (Admin, Registrar, Auditor, API Client)

**FR31:** Admin users can manage user accounts and assign roles

**FR32:** System can store JWT tokens securely in Windows Credential Manager (Desktop App)

**FR33:** System can rotate JWT secrets every 90 days

**FR34:** SIS Laravel can submit document generation requests via REST API with student data (JSON/XML/CSV)

**FR35:** System can validate student data against JSON schema before processing

**FR36:** System can return document generation status to SIS via API response

**FR37:** System can send webhook notifications to SIS when documents are ready (optional)

**FR38:** System can accept batch document generation requests from SIS

**FR39:** System can provide batch status endpoint for SIS to poll progress

**FR40:** Admin users can upload document templates (PDF format)

**FR41:** Admin users can associate templates with document types and institutions

**FR42:** System can version document templates

**FR43:** Admin users can list available templates

**FR44:** System can support multi-institution branding (different templates per university)

**FR45:** System can log all document lifecycle events (generation, signing, download) in immutable audit trail

**FR46:** System can store audit logs for 30 years (legal requirement)

**FR47:** Auditor users can retrieve audit trail for specific documents

**FR48:** System can encrypt sensitive student data (CIN, CNE, email, phone) at application level

**FR49:** System can enforce data minimization (collect only necessary data)

**FR50:** System can provide student data access/rectification/deletion capabilities (CNDP compliance)

**FR51:** System can retain documents for 30 years in compliance with Moroccan law

**FR52:** System can generate CNDP compliance reports

**FR53:** Admin users can view dashboard with document generation metrics (total documents, success rate, volume by type)

**FR54:** Admin users can view signing success rate metrics

**FR55:** System can alert administrators when certificate expiration is approaching (3 months before)

**FR56:** System can alert administrators on signing failures

**FR57:** System can alert administrators on storage threshold warnings

**FR58:** Admin users can view system health status (API uptime, S3 availability, dongle connectivity)

**FR59:** System can enforce rate limiting per endpoint and per client

**FR60:** System can return rate limit headers (X-RateLimit-Limit, X-RateLimit-Remaining, X-RateLimit-Reset)

**FR61:** System can provide OpenAPI 3.0 specification and interactive Swagger UI documentation

### Non-Functional Requirements

**NFR-P1:** API response time for document generation must be < 3 seconds (p95)

**NFR-P2:** API response time for all other endpoints must be < 500ms (p95)

**NFR-P3:** Desktop application signature operation must complete in < 30 seconds per document

**NFR-P4:** Batch signing of 500 documents must complete in < 15 minutes

**NFR-P5:** Public verification portal must respond in < 2 seconds (p95)

**NFR-P6:** Pre-signed S3 download URLs must be generated in < 1 second

**NFR-P7:** System must support concurrent signing operations (up to 10 simultaneous desktop app users)

**NFR-S1:** All data in transit must be encrypted using TLS 1.3 (minimum TLS 1.2)

**NFR-S2:** All documents at rest must be encrypted using SSE-KMS

**NFR-S3:** Sensitive student data (CIN, CNE, email, phone) must be encrypted at application level using AES-256-GCM

**NFR-S4:** JWT access tokens must expire after 1 hour, refresh tokens after 7 days

**NFR-S5:** JWT secrets must be rotated every 90 days

**NFR-S6:** Desktop application must store tokens securely in Windows Credential Manager

**NFR-S7:** USB dongle must require PIN code (3 attempts max before lock)

**NFR-S8:** All API endpoints must enforce authentication except public verification endpoint

**NFR-S9:** All API endpoints must enforce role-based access control (RBAC)

**NFR-S10:** Audit trail must be immutable (append-only, no deletions or modifications)

**NFR-S11:** Password policies must enforce minimum 12 characters for admin/registrar accounts

**NFR-S12:** Multi-factor authentication (MFA) must be required for admin accounts

**NFR-S13:** Certificate validation must be performed via OCSP or CRL before each signature

**NFR-S14:** System must detect and alert on certificate expiration 3 months in advance

**NFR-R1:** Backend API must maintain 99% uptime (excluding planned maintenance)

**NFR-R2:** Signature success rate must be > 99.5%

**NFR-R3:** S3 storage must maintain 99.9% availability

**NFR-R4:** System must implement dead-letter queue for failed signature operations with automatic retry

**NFR-R5:** System must implement graceful degradation (unsigned document generation if signature service unavailable)

**NFR-R6:** Database backups must be performed daily with 30-day retention

**NFR-R7:** System must support disaster recovery with Recovery Time Objective (RTO) < 4 hours

**NFR-R8:** System must support disaster recovery with Recovery Point Objective (RPO) < 1 hour

**NFR-SC1:** System must support 5,000+ documents signed per month across all faculties

**NFR-SC2:** System must support up to 50 concurrent API clients (SIS integrations)

**NFR-SC3:** System must support up to 10 concurrent desktop application users (registrar staff)

**NFR-SC4:** System must scale to support 10x document volume growth with < 10% performance degradation

**NFR-SC5:** S3 storage must scale to support 1 TB initial capacity with automatic expansion

**NFR-SC6:** Database must support horizontal scaling for read replicas

**NFR-C1:** System must comply with Moroccan Loi n° 53-05 (data protection)

**NFR-C2:** System must comply with Moroccan Loi n° 43-20 (electronic signature and digital trust)

**NFR-C3:** System must obtain CNDP F211 declaration before production deployment

**NFR-C4:** System must retain documents for 30 years (legal requirement for academic documents)

**NFR-C5:** System must retain audit logs for minimum 10 years (CNDP recommendation)

**NFR-C6:** System must support student data rights (access, rectification, deletion) per CNDP requirements

**NFR-C7:** System must enforce data minimization (collect only necessary data)

**NFR-C8:** System must provide CNDP compliance reports on demand

**NFR-C9:** Electronic signatures must use Barid Al-Maghrib Class 3 certificates (qualified signature level)

**NFR-C10:** Signatures must include RFC 3161 timestamping for legal non-repudiation

**NFR-C11:** System must maintain full legal validity of signed documents per Moroccan law

**NFR-I1:** System must support JSON, XML, and CSV data formats for SIS integration

**NFR-I2:** System must provide OpenAPI 3.0 specification for all REST endpoints

**NFR-I3:** System must provide interactive Swagger UI documentation

**NFR-I4:** System must support webhook notifications for async document status updates

**NFR-I5:** System must validate all incoming student data against JSON schema

**NFR-I6:** System must support OAuth 2.0 Client Credentials flow for machine-to-machine authentication

**NFR-I7:** System must support OAuth 2.0 Authorization Code with PKCE for user authentication

**NFR-I8:** System must enforce rate limiting (100 req/min for document generation, 1000 req/min for verification)

**NFR-M1:** System must provide structured logging with correlation IDs for request tracing

**NFR-M2:** System must provide monitoring dashboards for key metrics (signature success rate, API uptime, storage usage)

**NFR-M3:** System must provide alerting for critical events (certificate expiry, signature failures, storage thresholds)

**NFR-M4:** Desktop application must support auto-update mechanism for version management

**NFR-M5:** System must support containerized deployment (Docker)

**NFR-M6:** System must support infrastructure-as-code for reproducible deployments

**NFR-M7:** System must provide health check endpoints for monitoring

**NFR-U1:** Desktop application must support French and Arabic languages

**NFR-U2:** Desktop application must provide clear error messages for dongle connectivity issues

**NFR-U3:** Desktop application must display batch signing progress with estimated time remaining

**NFR-U4:** Admin dashboard must be accessible via modern web browsers (Chrome, Firefox, Edge, Safari)

**NFR-U5:** Public verification portal must be mobile-responsive

### Additional Requirements

**Architecture Starter Templates:**
- Backend API: Jason Taylor's Clean Architecture Solution Template (dotnet new ca-sln)
- Desktop App: Russkyc WPF MVVM Template (dotnet new russkyc-wpfmvvm)
- Infrastructure: PostgreSQL 15+ and MinIO in Docker containers via Docker Compose
- Dev Containers: Development environment with .NET 10 SDK running in Linux container

**Technology Stack Decisions:**
- OAuth 2.0 Provider: OpenIddict 7.2.0
- PII Encryption: ASP.NET Core Data Protection API with AES-256-GCM
- Background Jobs: Hangfire 1.8.23 with PostgreSQL storage
- PDF Generation: QuestPDF 2026.2.2
- PDF Signature: iText 7.9.5.0 + Portable.BouncyCastle 1.9.0
- USB Dongle Access: PKCS11Interop 5.1.2 (with Windows CSP fallback)
- HTTP Client (Desktop): Refit 10.0.1 with Polly 8.5.0
- Structured Logging: Serilog 10.0.0 + Seq (Docker container)
- Monitoring: Prometheus + Grafana (Docker containers)

**Naming Conventions & Code Standards:**
- Database: PascalCase tables (EF Core auto-pluralize), PascalCase columns
- API Endpoints: kebab-case URLs (/api/v1/documents/generate-and-sign)
- JSON Properties: camelCase
- C# Code: PascalCase classes/methods, camelCase parameters
- Correlation IDs for distributed tracing across all components

### FR Coverage Map

**Epic 1: Infrastructure & Project Foundation**
- Architecture requirements (starter templates, Dev Containers, Docker)

**Epic 2: Authentication & Security Foundation**
- FR27, FR28, FR29, FR30, FR31, FR32, FR33

**Epic 3: Document Generation & Storage**
- FR1, FR2, FR3, FR4, FR7, FR8, FR9, FR40, FR41, FR42, FR43, FR44

**Epic 4: Electronic Signature (Desktop App)**
- FR12, FR13, FR14, FR15, FR16, FR17, FR18, FR19, FR20

**Epic 5: Batch Processing & Background Jobs**
- FR5, FR6, FR19, FR38, FR39

**Epic 6: Public Verification Portal**
- FR21, FR22, FR23, FR24, FR25, FR26

**Epic 7: SIS Integration & API**
- FR34, FR35, FR36, FR37, FR38, FR39, FR61

**Epic 8: Audit Trail & Compliance**
- FR45, FR46, FR47, FR48, FR49, FR50, FR51, FR52

**Epic 9: Email Notifications & Student Experience**
- FR10, FR11

**Epic 10: Admin Dashboard & Monitoring**
- FR53, FR54, FR55, FR56, FR57, FR58, FR59, FR60

## Epic List

### Epic 1: Infrastructure & Project Foundation
Établir l'infrastructure technique de base permettant le développement de toutes les fonctionnalités futures. Les développeurs peuvent démarrer le développement avec un environnement reproductible et une architecture solide.

**FRs couverts:** Architecture requirements (starter templates, Dev Containers, Docker Compose)

### Epic 2: Authentication & Security Foundation
Permettre aux utilisateurs (registrar staff, SIS, admin) de s'authentifier de manière sécurisée et d'accéder au système selon leurs rôles. Fatima (registrar) et l'admin IT peuvent se connecter de manière sécurisée au système avec des permissions appropriées.

**FRs couverts:** FR27-FR33, NFR-S1, NFR-S4-S12

### Epic 3: Document Generation & Storage
Permettre la génération de documents académiques bilingues (AR/FR) et leur stockage sécurisé. Fatima peut générer des attestations de scolarité, relevés de notes, et attestations de réussite en format PDF bilingue.

**FRs couverts:** FR1-FR4, FR7-FR9, FR40-FR44, NFR-P1, NFR-P6, NFR-S2

### Epic 4: Electronic Signature (Desktop App)
Permettre à Fatima de signer électroniquement les documents PDF avec le dongle USB Barid Al-Maghrib. Fatima peut signer des documents officiels avec une signature électronique qualifiée légalement valide au Maroc.

**FRs couverts:** FR12-FR20, NFR-P3, NFR-S7, NFR-S13

### Epic 5: Batch Processing & Background Jobs
Permettre le traitement en masse de documents (jusqu'à 500 documents par batch). Fatima peut traiter 500 demandes d'attestations en 8 minutes au lieu de 2 jours.

**FRs couverts:** FR5-FR6, FR19, FR38-FR39, NFR-P4, NFR-R4

### Epic 6: Public Verification Portal
Permettre à n'importe qui (employeurs, agences gouvernementales) de vérifier l'authenticité des documents signés. Sarah (recruteuse RH) peut scanner le QR code et vérifier instantanément qu'un document est authentique.

**FRs couverts:** FR21-FR26, NFR-P5, NFR-U5

### Epic 7: SIS Integration & API
Permettre au SIS Laravel de l'université d'intégrer AcadSign pour automatiser la génération de documents. Omar (développeur SIS) peut intégrer AcadSign avec le système existant en quelques jours.

**FRs couverts:** FR34-FR39, FR61, NFR-I1-I8

### Epic 8: Audit Trail & Compliance
Assurer la conformité CNDP (Loi 53-05) et la traçabilité complète de tous les événements. L'université peut prouver la conformité légale et tracer chaque action sur chaque document pendant 30 ans.

**FRs couverts:** FR45-FR52, NFR-C1-C11, NFR-S10

### Epic 9: Email Notifications & Student Experience
Automatiser l'envoi de liens de téléchargement aux étudiants par email. Youssef reçoit son attestation par email en 3 minutes sans déplacement au campus.

**FRs couverts:** FR10-FR11

### Epic 10: Admin Dashboard & Monitoring
Fournir aux administrateurs une visibilité complète sur les métriques système et la santé de l'infrastructure. Karim (admin IT) peut monitorer le système en temps réel et recevoir des alertes proactives.

**FRs couverts:** FR53-FR60, NFR-M1-M7

---

## Epic 1: Infrastructure & Project Foundation

Établir l'infrastructure technique de base permettant le développement de toutes les fonctionnalités futures. Les développeurs peuvent démarrer le développement avec un environnement reproductible et une architecture solide.

### Story 1.1: Initialiser Backend API avec Clean Architecture Template

As a **développeur backend**,
I want **initialiser le projet Backend API avec le template Clean Architecture de Jason Taylor**,
So that **j'ai une structure de projet solide et production-ready pour développer toutes les fonctionnalités AcadSign**.

**Acceptance Criteria:**

**Given** le template Clean Architecture est disponible via NuGet
**When** j'exécute la commande `dotnet new install Clean.Architecture.Solution.Template` puis `dotnet new ca-sln --client-framework None --database postgresql --output AcadSign.Backend`
**Then** le projet Backend API est créé avec la structure suivante :
- Projet `Domain` (entités, value objects, events)
- Projet `Application` (use cases, CQRS avec MediatR, interfaces)
- Projet `Infrastructure` (EF Core, PostgreSQL, services externes)
- Projet `Web` (API controllers/endpoints, middleware, OpenAPI)

**And** les packages suivants sont pré-configurés :
- MediatR pour CQRS pattern
- FluentValidation pour validation
- AutoMapper pour mapping DTO ↔ Entities
- EF Core 10 avec Npgsql provider pour PostgreSQL
- Serilog pour structured logging
- OpenAPI/Scalar pour documentation API

**And** le projet compile sans erreurs avec `dotnet build`

**And** le projet démarre avec `dotnet run` et affiche Swagger UI à `/api/v1/docs`

**And** les health check endpoints sont disponibles à `/health`

---

### Story 1.2: Initialiser Desktop App avec WPF MVVM Template

As a **développeur desktop**,
I want **initialiser le projet Desktop App avec le template WPF MVVM de Russkyc**,
So that **j'ai une structure MVVM propre avec CommunityToolkit pour développer l'application de signature**.

**Acceptance Criteria:**

**Given** le template WPF MVVM est disponible via NuGet
**When** j'exécute la commande `dotnet new install Russkyc.Templates.WPF-MVVM` puis `dotnet new russkyc-wpfmvvm -n AcadSign.Desktop`
**Then** le projet Desktop App est créé avec la structure suivante :
- Dossier `Views/` pour composants XAML UI
- Dossier `ViewModels/` pour ViewModels avec CommunityToolkit.Mvvm
- Dossier `Services/` pour business logic et API clients
- Dossier `Models/` pour data models

**And** CommunityToolkit.Mvvm est pré-configuré avec support pour :
- Attributs `[ObservableProperty]` pour propriétés observables
- Attributs `[RelayCommand]` pour commandes
- Source generators pour réduire boilerplate
- `INotifyPropertyChanged` automatique

**And** le fichier `.csproj` est migré vers .NET 10 :
```xml
<TargetFramework>net10.0-windows</TargetFramework>
```

**And** Dependency Injection est configuré avec `Microsoft.Extensions.DependencyInjection`

**And** le projet compile sans erreurs avec `dotnet build`

**And** l'application démarre avec `dotnet run` et affiche une fenêtre WPF vide

**And** XAML Hot Reload fonctionne pendant le développement

---

### Story 1.3: Configurer Infrastructure Conteneurisée (PostgreSQL, MinIO, Seq)

As a **développeur backend**,
I want **configurer PostgreSQL, MinIO et Seq en conteneurs Docker via Docker Compose**,
So that **tous les développeurs ont un environnement de développement identique et reproductible**.

**Acceptance Criteria:**

**Given** Docker Desktop est installé sur la machine de développement
**When** je crée un fichier `docker-compose.yml` à la racine du projet avec les services suivants :
- PostgreSQL 15-alpine (port 5432)
- MinIO latest (ports 9000 et 9001)
- Seq 2025.2 (port 5341)

**Then** le fichier Docker Compose contient la configuration complète :

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:15-alpine
    container_name: acadsign-postgres
    environment:
      POSTGRES_DB: acadsign
      POSTGRES_USER: acadsign_user
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U acadsign_user"]
      interval: 10s
      timeout: 5s
      retries: 5

  minio:
    image: minio/minio:latest
    container_name: acadsign-minio
    command: server /data --console-address ":9001"
    environment:
      MINIO_ROOT_USER: ${MINIO_ROOT_USER}
      MINIO_ROOT_PASSWORD: ${MINIO_ROOT_PASSWORD}
    ports:
      - "9000:9000"
      - "9001:9001"
    volumes:
      - minio_data:/data
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:9000/minio/health/live"]
      interval: 30s
      timeout: 20s
      retries: 3

  seq:
    image: datalust/seq:2025.2
    container_name: acadsign-seq
    environment:
      ACCEPT_EULA: Y
    ports:
      - "5341:80"
    volumes:
      - seq_data:/data

volumes:
  postgres_data:
  minio_data:
  seq_data:
```

**And** un fichier `.env.example` est créé avec les variables d'environnement :
```
POSTGRES_PASSWORD=your_postgres_password
MINIO_ROOT_USER=minioadmin
MINIO_ROOT_PASSWORD=your_minio_password
```

**And** le fichier `.env` est ajouté au `.gitignore`

**And** la commande `docker-compose up -d` démarre tous les services sans erreurs

**And** PostgreSQL est accessible à `localhost:5432` avec les credentials configurés

**And** MinIO console est accessible à `http://localhost:9001`

**And** Seq UI est accessible à `http://localhost:5341`

**And** tous les health checks passent au vert après démarrage

**And** la commande `docker-compose down -v` arrête et nettoie tous les services

---

### Story 1.4: Configurer Dev Containers pour Développement

As a **développeur backend sur macOS**,
I want **configurer Dev Containers pour exécuter .NET 10 SDK et l'API dans un conteneur Linux**,
So that **je peux développer sur Mac avec un environnement Linux reproductible**.

**Acceptance Criteria:**

**Given** l'extension Dev Containers est installée dans VS Code
**When** je crée le fichier `.devcontainer/devcontainer.json` à la racine du projet Backend
**Then** le fichier contient la configuration suivante :

```json
{
  "name": "AcadSign Backend",
  "dockerComposeFile": "../docker-compose.yml",
  "service": "backend",
  "workspaceFolder": "/workspace",
  "customizations": {
    "vscode": {
      "extensions": [
        "ms-dotnettools.csharp",
        "ms-dotnettools.csdevkit",
        "editorconfig.editorconfig",
        "humao.rest-client"
      ]
    }
  },
  "forwardPorts": [5000, 5432, 9000, 9001, 5341],
  "postCreateCommand": "dotnet restore"
}
```

**And** le fichier `docker-compose.yml` est mis à jour pour inclure le service `backend` :

```yaml
backend:
  image: mcr.microsoft.com/devcontainers/dotnet:10.0
  container_name: acadsign-backend-dev
  volumes:
    - ../AcadSign.Backend:/workspace:cached
  command: sleep infinity
  depends_on:
    - postgres
    - minio
    - seq
  environment:
    - ASPNETCORE_ENVIRONMENT=Development
```

**And** VS Code peut ouvrir le projet dans le Dev Container avec "Reopen in Container"

**And** le .NET 10 SDK est disponible dans le conteneur (`dotnet --version` retourne 10.x.x)

**And** le projet Backend compile dans le conteneur avec `dotnet build`

**And** l'API démarre dans le conteneur avec `dotnet run` et est accessible depuis le host à `http://localhost:5000`

**And** PostgreSQL, MinIO et Seq sont accessibles depuis le conteneur backend

**And** Hot reload fonctionne pour les modifications de code C#

**And** un fichier `README.md` est créé avec les instructions de setup Dev Containers

---

## Epic 2: Authentication & Security Foundation

Permettre aux utilisateurs (registrar staff, SIS, admin) de s'authentifier de manière sécurisée et d'accéder au système selon leurs rôles. Fatima (registrar) et l'admin IT peuvent se connecter de manière sécurisée au système avec des permissions appropriées.

### Story 2.1: Configurer OpenIddict pour OAuth 2.0

As a **développeur backend**,
I want **configurer OpenIddict 7.2.0 comme provider OAuth 2.0/OpenID Connect**,
So that **le système peut authentifier les utilisateurs et les clients API avec des tokens JWT**.

**Acceptance Criteria:**

**Given** le projet Backend API est initialisé avec Clean Architecture
**When** j'installe les packages NuGet OpenIddict :
- `OpenIddict.AspNetCore` version 7.2.0
- `OpenIddict.EntityFrameworkCore` version 7.2.0

**Then** OpenIddict est configuré dans `Program.cs` avec :
- Stockage des tokens dans PostgreSQL via EF Core
- Support OAuth 2.0 Client Credentials flow
- Support OAuth 2.0 Authorization Code + PKCE flow
- JWT tokens comme format de token
- Access token validity: 1 heure
- Refresh token validity: 7 jours

**And** les tables OpenIddict sont créées dans PostgreSQL via migration EF Core :
- `OpenIddictApplications`
- `OpenIddictAuthorizations`
- `OpenIddictScopes`
- `OpenIddictTokens`

**And** un endpoint `/connect/token` est disponible pour obtenir des tokens

**And** un endpoint `/connect/authorize` est disponible pour Authorization Code flow

**And** un endpoint `/connect/introspect` est disponible pour valider les tokens

**And** la configuration TLS 1.3 (minimum TLS 1.2) est appliquée pour toutes les communications

**And** les secrets JWT sont stockés de manière sécurisée dans `appsettings.json` (dev) et Azure Key Vault (prod)

---

### Story 2.2: Implémenter Client Credentials Flow (SIS Laravel → Backend)

As a **développeur SIS Laravel**,
I want **authentifier le SIS auprès de l'API Backend via OAuth 2.0 Client Credentials**,
So that **le SIS peut appeler les endpoints API de manière sécurisée pour générer des documents**.

**Acceptance Criteria:**

**Given** OpenIddict est configuré dans le Backend API
**When** je crée un client OAuth 2.0 pour le SIS Laravel avec :
- Client ID: `sis-laravel-client`
- Client Secret: généré de manière sécurisée
- Grant type: `client_credentials`
- Scopes: `api.documents.generate`, `api.documents.read`

**Then** le SIS Laravel peut obtenir un access token en appelant :
```http
POST /connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=client_credentials
&client_id=sis-laravel-client
&client_secret={secret}
&scope=api.documents.generate api.documents.read
```

**And** la réponse contient un JWT access token valide pour 1 heure :
```json
{
  "access_token": "eyJ...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "scope": "api.documents.generate api.documents.read"
}
```

**And** le JWT token contient les claims suivants :
- `sub`: client ID
- `client_id`: `sis-laravel-client`
- `scope`: scopes accordés
- `exp`: timestamp d'expiration (1h)
- `iat`: timestamp de création

**And** les endpoints API Backend valident le token JWT et vérifient les scopes requis

**And** un token expiré retourne HTTP 401 Unauthorized

**And** un token avec scopes insuffisants retourne HTTP 403 Forbidden

**And** la rotation des secrets JWT est configurée pour 90 jours (NFR-S5)

---

### Story 2.3: Implémenter Authorization Code + PKCE Flow (Desktop App → Backend)

As a **Fatima (registrar staff)**,
I want **me connecter à la Desktop App avec mes credentials et obtenir un token sécurisé**,
So that **je peux signer des documents de manière authentifiée**.

**Acceptance Criteria:**

**Given** OpenIddict est configuré avec Authorization Code + PKCE flow
**When** Fatima lance la Desktop App et clique sur "Se connecter"
**Then** la Desktop App :
1. Génère un code_verifier aléatoire (PKCE)
2. Calcule le code_challenge = SHA256(code_verifier)
3. Ouvre un navigateur vers `/connect/authorize` avec :
   - `response_type=code`
   - `client_id=acadsign-desktop`
   - `redirect_uri=http://localhost:7890/callback`
   - `scope=openid profile api.documents.sign`
   - `code_challenge={challenge}`
   - `code_challenge_method=S256`

**And** Fatima entre ses credentials (email + password) dans le navigateur

**And** après authentification réussie, le navigateur redirige vers `http://localhost:7890/callback?code={authorization_code}`

**And** la Desktop App échange le code contre un token :
```http
POST /connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=authorization_code
&client_id=acadsign-desktop
&code={authorization_code}
&redirect_uri=http://localhost:7890/callback
&code_verifier={code_verifier}
```

**And** la réponse contient :
```json
{
  "access_token": "eyJ...",
  "refresh_token": "eyJ...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "scope": "openid profile api.documents.sign"
}
```

**And** le JWT access token contient les claims :
- `sub`: user ID de Fatima
- `email`: email de Fatima
- `role`: `Registrar`
- `institutionId`: ID de l'université
- `exp`: expiration (1h)

**And** la Desktop App peut utiliser le refresh token pour obtenir un nouveau access token après expiration (7 jours de validité)

**And** les tokens sont stockés de manière sécurisée (voir Story 2.6)

---

### Story 2.4: Implémenter RBAC (Admin, Registrar, Auditor, API Client)

As a **administrateur système**,
I want **gérer les rôles utilisateurs (Admin, Registrar, Auditor, API Client) avec des permissions spécifiques**,
So that **chaque utilisateur a accès uniquement aux fonctionnalités autorisées**.

**Acceptance Criteria:**

**Given** ASP.NET Core Identity est configuré avec OpenIddict
**When** je crée les rôles suivants dans la base de données :
- `Admin`: Accès complet (gestion templates, users, audit logs, configuration)
- `Registrar`: Génération et signature de documents
- `Auditor`: Lecture seule des audit logs
- `API Client`: Génération de documents via API (SIS)

**Then** chaque rôle a les permissions suivantes :

**Admin:**
- POST/PUT/DELETE `/api/v1/templates`
- POST/PUT/DELETE `/api/v1/users`
- GET `/api/v1/audit/*`
- GET `/api/v1/admin/dashboard`
- Tous les endpoints Registrar

**Registrar:**
- POST `/api/v1/documents/generate`
- GET `/api/v1/documents/{id}/unsigned`
- POST `/api/v1/documents/{id}/upload-signed`
- GET `/api/v1/documents/{id}`
- GET `/api/v1/documents/{id}/download`

**Auditor:**
- GET `/api/v1/audit/{documentId}`
- GET `/api/v1/audit/search`

**API Client:**
- POST `/api/v1/documents/generate`
- POST `/api/v1/documents/batch`
- GET `/api/v1/documents/batch/{batchId}/status`
- GET `/api/v1/documents/{id}`

**And** les endpoints API utilisent l'attribut `[Authorize(Roles = "Admin,Registrar")]` pour contrôler l'accès

**And** un utilisateur sans le rôle requis reçoit HTTP 403 Forbidden

**And** le JWT token contient le claim `role` avec le rôle de l'utilisateur

**And** les middleware ASP.NET Core valident automatiquement les rôles sur chaque requête

**And** un endpoint `/api/v1/users/{userId}/roles` permet aux Admins de gérer les rôles

**And** Multi-Factor Authentication (MFA) est requis pour les comptes Admin (NFR-S12)

---

### Story 2.5: Configurer Chiffrement PII avec ASP.NET Data Protection API

As a **développeur backend**,
I want **chiffrer les données sensibles (CIN, CNE, email, phone) au niveau application avec AES-256-GCM**,
So that **les données PII sont protégées même si la base de données est compromise**.

**Acceptance Criteria:**

**Given** ASP.NET Core Data Protection API est disponible
**When** je configure Data Protection dans `Program.cs` :
```csharp
services.AddDataProtection()
    .PersistKeysToDbContext<ApplicationDbContext>()
    .SetApplicationName("AcadSign")
    .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
    {
        EncryptionAlgorithm = EncryptionAlgorithm.AES_256_GCM,
        ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
    });
```

**Then** les clés de chiffrement sont stockées dans PostgreSQL dans la table `DataProtectionKeys`

**And** un service `IPiiEncryptionService` est créé avec les méthodes :
- `string Encrypt(string plainText)`
- `string Decrypt(string cipherText)`

**And** les champs suivants sont chiffrés avant insertion en base de données :
- `Student.CIN` (Carte d'Identité Nationale)
- `Student.CNE` (Code National Étudiant)
- `Student.Email`
- `Student.PhoneNumber`

**And** les entités EF Core utilisent des propriétés avec chiffrement automatique :
```csharp
public class Student
{
    public Guid Id { get; set; }
    
    [EncryptedProperty]
    public string CIN { get; set; } // Stocké chiffré en DB
    
    [EncryptedProperty]
    public string CNE { get; set; }
    
    [EncryptedProperty]
    public string Email { get; set; }
    
    [EncryptedProperty]
    public string PhoneNumber { get; set; }
    
    public string FirstName { get; set; } // Non chiffré
    public string LastName { get; set; } // Non chiffré
}
```

**And** un intercepteur EF Core chiffre/déchiffre automatiquement les propriétés marquées `[EncryptedProperty]`

**And** les clés de chiffrement sont automatiquement rotées tous les 90 jours

**And** les anciennes clés sont conservées pour déchiffrer les données existantes

**And** un test unitaire vérifie que les données chiffrées en DB ne sont pas lisibles en clair

**And** la conformité CNDP (Loi 53-05) est respectée pour la protection des données sensibles (NFR-C1, NFR-C5)

---

### Story 2.6: Implémenter Stockage Sécurisé Tokens (Desktop App - Windows Credential Manager)

As a **Fatima (registrar staff)**,
I want **que mes tokens d'authentification soient stockés de manière sécurisée sur mon workstation**,
So that **je n'ai pas besoin de me reconnecter à chaque fois et mes credentials sont protégés**.

**Acceptance Criteria:**

**Given** la Desktop App WPF est initialisée
**When** j'installe le package NuGet `CredentialManagement` pour accéder au Windows Credential Manager
**Then** un service `ITokenStorageService` est créé avec les méthodes :
- `Task SaveTokensAsync(string accessToken, string refreshToken)`
- `Task<(string accessToken, string refreshToken)> GetTokensAsync()`
- `Task DeleteTokensAsync()`

**And** les tokens sont stockés dans Windows Credential Manager avec :
- Target: `AcadSign.Desktop.Tokens`
- Username: email de l'utilisateur
- Password: JSON contenant `{ "access_token": "...", "refresh_token": "..." }`
- Persistence: `LocalMachine` (accessible uniquement sur ce PC)

**And** les tokens sont chiffrés automatiquement par Windows Credential Manager (DPAPI)

**And** après connexion réussie, la Desktop App sauvegarde les tokens :
```csharp
await _tokenStorage.SaveTokensAsync(response.AccessToken, response.RefreshToken);
```

**And** au démarrage de l'application, la Desktop App vérifie si des tokens valides existent :
```csharp
var (accessToken, refreshToken) = await _tokenStorage.GetTokensAsync();
if (!string.IsNullOrEmpty(accessToken) && !IsTokenExpired(accessToken))
{
    // Utiliser le token existant
}
else if (!string.IsNullOrEmpty(refreshToken))
{
    // Rafraîchir le token
    await RefreshAccessTokenAsync(refreshToken);
}
else
{
    // Demander connexion
    ShowLoginWindow();
}
```

**And** si l'access token est expiré mais le refresh token est valide, la Desktop App rafraîchit automatiquement le token :
```http
POST /connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=refresh_token
&client_id=acadsign-desktop
&refresh_token={refresh_token}
```

**And** lors de la déconnexion, les tokens sont supprimés du Credential Manager :
```csharp
await _tokenStorage.DeleteTokensAsync();
```

**And** les tokens ne sont jamais loggés ou affichés en clair dans l'UI

**And** un test vérifie que les tokens persistent après redémarrage de l'application

**And** la sécurité NFR-S6 est respectée (stockage sécurisé dans Windows Credential Manager)

---

## Epic 3: Document Generation & Storage

Permettre la génération de documents académiques bilingues (AR/FR) et leur stockage sécurisé. Fatima peut générer des attestations de scolarité, relevés de notes, et attestations de réussite en format PDF bilingue.

### Story 3.1: Configurer QuestPDF pour Génération PDF Bilingue

As a **développeur backend**,
I want **configurer QuestPDF 2026.2.2 pour générer des PDFs bilingues (Arabic RTL + French LTR)**,
So that **le système peut créer des documents académiques officiels dans les deux langues**.

**Acceptance Criteria:**

**Given** le projet Backend API est configuré
**When** j'installe le package NuGet `QuestPDF` version 2026.2.2
**Then** QuestPDF est configuré avec support pour :
- Fonts Unicode (Arabic + French)
- Layout RTL (Right-to-Left) pour l'arabe
- Layout LTR (Left-to-Right) pour le français
- Génération de QR codes

**And** un service `IPdfGenerationService` est créé avec la méthode :
```csharp
Task<byte[]> GenerateDocumentAsync(DocumentType type, StudentData data);
```

**And** les fonts suivantes sont incluses dans le projet :
- `Amiri-Regular.ttf` pour l'arabe
- `Roboto-Regular.ttf` pour le français
- Fonts chargées via `QuestPDF.Fonts.FontManager`

**And** un template de base est créé avec :
- En-tête bilingue (logo université + titre AR/FR)
- Section données étudiant (nom AR/FR, CIN, CNE)
- Section contenu spécifique au type de document
- QR code en bas de page
- Pied de page avec date et signature électronique

**And** un test unitaire génère un PDF de test et vérifie :
- Le fichier PDF est valide
- Les caractères arabes s'affichent correctement (RTL)
- Les caractères français s'affichent correctement (LTR)
- La taille du fichier est raisonnable (< 500 KB)

**And** la performance respecte NFR-P1 (génération < 3 secondes)

---

### Story 3.2: Implémenter Génération des 4 Types de Documents

As a **Fatima (registrar staff)**,
I want **générer les 4 types de documents académiques officiels**,
So that **je peux fournir aux étudiants tous les documents dont ils ont besoin**.

**Acceptance Criteria:**

**Given** QuestPDF est configuré avec templates bilingues
**When** je crée des templates pour les 4 types de documents :
1. **Attestation de Scolarité** (Enrollment Certificate)
2. **Relevé de Notes** (Transcript)
3. **Attestation de Réussite** (Certificate of Achievement)
4. **Attestation d'Inscription** (Registration Certificate)

**Then** chaque template contient les sections suivantes :

**Attestation de Scolarité:**
- Titre bilingue : "شهادة مدرسية / Attestation de Scolarité"
- Données étudiant : Nom AR/FR, CIN, CNE, Date de naissance
- Programme d'études : Nom du programme AR/FR, Faculté, Année académique
- Statut d'inscription : "Régulièrement inscrit(e)"
- Date d'émission
- QR code de vérification

**Relevé de Notes:**
- Titre bilingue : "كشف النقاط / Relevé de Notes"
- Données étudiant
- Tableau des notes par matière (AR/FR) :
  - Nom de la matière
  - Note sur 20
  - Crédits ECTS
- GPA (Moyenne Générale)
- Mention (Passable, Assez Bien, Bien, Très Bien)
- QR code

**Attestation de Réussite:**
- Titre bilingue : "شهادة نجاح / Attestation de Réussite"
- Données étudiant
- Programme complété
- Année d'obtention
- Mention
- QR code

**Attestation d'Inscription:**
- Titre bilingue : "شهادة تسجيل / Attestation d'Inscription"
- Données étudiant
- Programme d'inscription
- Année académique en cours
- Date d'inscription
- QR code

**And** chaque document génère un UUID v4 unique (FR3)

**And** un endpoint API est créé pour chaque type :
```http
POST /api/v1/documents/generate
Content-Type: application/json

{
  "documentType": "ATTESTATION_SCOLARITE",
  "studentData": { ... }
}
```

**And** la réponse contient le document ID et l'URL du PDF non signé :
```json
{
  "documentId": "uuid-v4",
  "status": "UNSIGNED",
  "unsignedPdfUrl": "https://api.acadsign.ma/documents/{id}/unsigned",
  "createdAt": "2026-03-04T10:00:00Z"
}
```

**And** les 4 types de documents sont testés avec des données réelles

**And** FR1 et FR2 sont complètement implémentés

---

### Story 3.3: Implémenter Génération et Embedding de QR Codes

As a **Sarah (recruteuse RH)**,
I want **scanner un QR code sur un document et être redirigée vers le portail de vérification**,
So that **je peux vérifier instantanément l'authenticité du document**.

**Acceptance Criteria:**

**Given** QuestPDF est configuré pour générer des PDFs
**When** j'installe le package NuGet `QRCoder` pour générer des QR codes
**Then** un service `IQrCodeService` est créé avec la méthode :
```csharp
byte[] GenerateQrCode(string data, int pixelSize = 300);
```

**And** lors de la génération d'un document, un QR code est créé avec :
- Données : URL de vérification `https://verify.acadsign.ma/documents/{documentId}`
- Taille : 300x300 pixels
- Format : PNG
- Niveau de correction d'erreur : Medium (M)

**And** le QR code est embedé dans le PDF en bas à droite avec :
- Position : 20mm du bord droit, 20mm du bas
- Taille : 30mm x 30mm
- Légende bilingue : "رمز التحقق / Code de Vérification"

**And** le document ID utilisé dans l'URL est un UUID v4 non-prédictible (FR3)

**And** un test vérifie que :
- Le QR code est scannable avec un smartphone
- L'URL décodée est correcte
- Le QR code est visible et lisible dans le PDF

**And** FR4 est complètement implémenté

---

### Story 3.4: Configurer MinIO S3 Storage avec Chiffrement SSE-KMS

As a **développeur backend**,
I want **stocker les documents signés dans MinIO avec chiffrement SSE-KMS**,
So that **les documents sont protégés au repos et conformes CNDP**.

**Acceptance Criteria:**

**Given** MinIO est déployé en conteneur Docker (Story 1.3)
**When** j'installe le package NuGet `Minio` SDK
**Then** un service `IS3StorageService` est créé avec les méthodes :
```csharp
Task<string> UploadDocumentAsync(byte[] pdfData, string documentId);
Task<byte[]> DownloadDocumentAsync(string documentId);
Task<string> GeneratePresignedDownloadUrlAsync(string documentId, int expiryMinutes = 60);
Task DeleteDocumentAsync(string documentId);
```

**And** MinIO est configuré avec :
- Bucket name: `acadsign-documents`
- Région: `us-east-1` (par défaut MinIO)
- Chiffrement: SSE-KMS activé
- Versioning: Activé pour rétention 30 ans

**And** la configuration MinIO dans `appsettings.json` :
```json
{
  "MinIO": {
    "Endpoint": "localhost:9000",
    "AccessKey": "${MINIO_ROOT_USER}",
    "SecretKey": "${MINIO_ROOT_PASSWORD}",
    "BucketName": "acadsign-documents",
    "UseSSL": false,
    "Region": "us-east-1"
  }
}
```

**And** lors de l'upload d'un document signé :
```csharp
var objectName = $"{documentId}.pdf";
await _minioClient.PutObjectAsync(new PutObjectArgs()
    .WithBucket("acadsign-documents")
    .WithObject(objectName)
    .WithStreamData(pdfStream)
    .WithContentType("application/pdf")
    .WithServerSideEncryption(sse));
```

**And** les documents sont organisés par année :
- Path: `{year}/{month}/{documentId}.pdf`
- Exemple: `2026/03/uuid-v4.pdf`

**And** un test vérifie :
- Upload d'un PDF réussit
- Download du PDF retourne les mêmes données
- Le document est chiffré au repos (SSE-KMS)

**And** FR8 et NFR-S2 sont implémentés

---

### Story 3.5: Implémenter Génération de Pre-Signed URLs

As a **Youssef (étudiant)**,
I want **recevoir un lien de téléchargement sécurisé avec expiration**,
So that **je peux télécharger mon document de manière sécurisée sans authentification**.

**Acceptance Criteria:**

**Given** MinIO S3 Storage est configuré
**When** un document est signé et uploadé sur S3
**Then** le système génère automatiquement une pre-signed URL avec :
- Validité : 1 heure (3600 secondes)
- Méthode HTTP : GET
- Pas d'authentification requise pour le téléchargement

**And** la méthode `GeneratePresignedDownloadUrlAsync` génère l'URL :
```csharp
var presignedUrl = await _minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
    .WithBucket("acadsign-documents")
    .WithObject($"{year}/{month}/{documentId}.pdf")
    .WithExpiry(3600)); // 1 heure
```

**And** l'endpoint API retourne la pre-signed URL :
```http
GET /api/v1/documents/{documentId}/download
Authorization: Bearer {jwt_token}

Response:
{
  "downloadUrl": "https://minio.acadsign.ma/acadsign-documents/2026/03/uuid.pdf?X-Amz-...",
  "expiresAt": "2026-03-04T11:00:00Z"
}
```

**And** Youssef peut télécharger le PDF directement via l'URL sans authentification

**And** après expiration (1h), l'URL retourne HTTP 403 Forbidden

**And** un test vérifie :
- URL générée est valide
- Téléchargement réussit avant expiration
- Téléchargement échoue après expiration

**And** FR9 et NFR-P6 sont implémentés (génération < 1 seconde)

---

### Story 3.6: Implémenter Template Management (Upload, Versioning)

As a **administrateur IT**,
I want **uploader et versionner les templates PDF pour chaque type de document**,
So that **l'université peut personnaliser les documents avec son branding**.

**Acceptance Criteria:**

**Given** le système de génération PDF est opérationnel
**When** je crée les entités EF Core suivantes :
```csharp
public class DocumentTemplate
{
    public Guid Id { get; set; }
    public DocumentType Type { get; set; }
    public string InstitutionId { get; set; }
    public string Version { get; set; }
    public byte[] TemplateData { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
```

**Then** un endpoint API permet d'uploader un template :
```http
POST /api/v1/templates
Authorization: Bearer {admin_token}
Content-Type: multipart/form-data

templateFile: (binary PDF)
documentType: "ATTESTATION_SCOLARITE"
institutionId: "university-hassan-ii"
```

**And** la réponse contient :
```json
{
  "templateId": "uuid",
  "documentType": "ATTESTATION_SCOLARITE",
  "version": "1.0",
  "createdAt": "2026-03-04T10:00:00Z"
}
```

**And** un endpoint permet de lister les templates :
```http
GET /api/v1/templates?institutionId=university-hassan-ii

Response:
{
  "templates": [
    {
      "templateId": "uuid",
      "documentType": "ATTESTATION_SCOLARITE",
      "institutionId": "university-hassan-ii",
      "version": "1.0",
      "isActive": true,
      "createdAt": "2026-03-04T10:00:00Z"
    }
  ]
}
```

**And** le versioning automatique incrémente la version (1.0 → 1.1 → 2.0)

**And** seul le template `IsActive = true` est utilisé pour la génération

**And** les anciens templates sont conservés pour historique (rétention 30 ans)

**And** seuls les utilisateurs avec rôle `Admin` peuvent uploader des templates (RBAC)

**And** FR40, FR41, FR42, FR43, FR44 sont implémentés

---

## Epic 4: Electronic Signature (Desktop App)

Permettre à Fatima de signer électroniquement les documents PDF avec le dongle USB Barid Al-Maghrib. Fatima peut signer des documents officiels avec une signature électronique qualifiée légalement valide au Maroc.

### Story 4.1: Configurer Desktop App UI avec MVVM Pattern

As a **Fatima (registrar staff)**,
I want **une interface Desktop App intuitive pour signer des documents**,
So that **je peux facilement gérer mes tâches de signature quotidiennes**.

**Acceptance Criteria:**

**Given** le projet Desktop App WPF MVVM est initialisé (Story 1.2)
**When** je crée l'architecture MVVM avec CommunityToolkit
**Then** la structure suivante est créée :

**Views:**
- `LoginView.xaml` : Écran de connexion OAuth 2.0
- `MainView.xaml` : Dashboard principal avec liste des documents à signer
- `SigningView.xaml` : Vue de signature avec progress bar
- `SettingsView.xaml` : Configuration (API endpoint, dongle settings)

**ViewModels:**
- `LoginViewModel` : Gestion authentification OAuth 2.0 + PKCE
- `MainViewModel` : Liste documents, refresh, navigation
- `SigningViewModel` : Logique signature, progress tracking
- `SettingsViewModel` : Configuration application

**Services:**
- `IAuthenticationService` : OAuth 2.0 flows
- `IApiClientService` : Communication avec Backend API (Refit)
- `ISignatureService` : Signature PAdES avec dongle
- `IDongleService` : Détection et accès USB dongle
- `ITokenStorageService` : Stockage sécurisé tokens (Story 2.6)

**And** la navigation entre vues utilise un `NavigationService` :
```csharp
[RelayCommand]
private async Task NavigateToSigningAsync()
{
    await _navigationService.NavigateToAsync<SigningViewModel>();
}
```

**And** le binding MVVM utilise `[ObservableProperty]` et `[RelayCommand]` :
```csharp
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<DocumentDto> _documents;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [RelayCommand]
    private async Task LoadDocumentsAsync()
    {
        IsLoading = true;
        Documents = await _apiClient.GetPendingDocumentsAsync();
        IsLoading = false;
    }
}
```

**And** l'UI supporte le français et l'arabe (NFR-U1) avec fichiers de ressources

**And** XAML Hot Reload fonctionne pour le développement rapide

---

### Story 4.2: Implémenter Détection et Accès USB Dongle (PKCS#11 + CSP)

As a **Fatima (registrar staff)**,
I want **que l'application détecte automatiquement mon dongle USB Barid Al-Maghrib**,
So that **je peux signer des documents sans configuration manuelle complexe**.

**Acceptance Criteria:**

**Given** la Desktop App est lancée
**When** j'installe les packages NuGet :
- `Pkcs11Interop` version 5.1.2
- `System.Security.Cryptography.Csp` (inclus dans .NET)

**Then** un service `IDongleService` est créé avec les méthodes :
```csharp
Task<bool> IsDongleConnectedAsync();
Task<DongleInfo> GetDongleInfoAsync();
Task<X509Certificate2> GetCertificateAsync(string pin);
```

**And** la détection du dongle tente d'abord PKCS#11 :
```csharp
try
{
    var pkcs11 = new Pkcs11InteropFactories().Pkcs11LibraryFactory
        .LoadPkcs11Library(factories, "baridmb.dll", AppType.MultiThreaded);
    
    var slots = pkcs11.GetSlotList(SlotsType.WithTokenPresent);
    if (slots.Count > 0)
    {
        return true; // Dongle détecté via PKCS#11
    }
}
catch (Pkcs11Exception ex)
{
    // Fallback vers Windows CSP
}
```

**And** si PKCS#11 échoue, fallback vers Windows CSP :
```csharp
var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
store.Open(OpenFlags.ReadOnly);
var certs = store.Certificates.Find(
    X509FindType.FindByIssuerName, 
    "Barid Al-Maghrib", 
    false);
if (certs.Count > 0)
{
    return true; // Certificat trouvé via CSP
}
```

**And** l'UI affiche le statut du dongle en temps réel :
- ✅ "Dongle connecté - Certificat valide jusqu'au {date}"
- ⚠️ "Dongle non détecté - Veuillez brancher votre dongle USB"
- ❌ "Certificat expiré - Veuillez renouveler votre certificat"

**And** un health check vérifie la connexion dongle toutes les 5 minutes

**And** des alertes sont affichées si le dongle est déconnecté pendant la signature

**And** FR13 est implémenté (détection dongle PKCS#11/CSP)

---

### Story 4.3: Implémenter Signature PAdES avec iText 7 + BouncyCastle

As a **Fatima (registrar staff)**,
I want **signer un document PDF avec mon dongle USB en format PAdES**,
So that **la signature est légalement valide au Maroc selon la Loi 43-20**.

**Acceptance Criteria:**

**Given** le dongle USB est détecté et connecté
**When** j'installe les packages NuGet :
- `itext7` version 9.5.0
- `itext7.bouncy-castle-adapter` version 9.5.0
- `Portable.BouncyCastle` version 1.9.0

**Then** un service `ISignatureService` est créé avec la méthode :
```csharp
Task<byte[]> SignPdfAsync(byte[] unsignedPdf, string pin);
```

**And** la signature PAdES est implémentée avec iText 7 :
```csharp
public async Task<byte[]> SignPdfAsync(byte[] unsignedPdf, string pin)
{
    // 1. Charger le certificat depuis le dongle
    var cert = await _dongleService.GetCertificateAsync(pin);
    
    // 2. Créer le PdfSigner
    using var reader = new PdfReader(new MemoryStream(unsignedPdf));
    using var outputStream = new MemoryStream();
    var signer = new PdfSigner(reader, outputStream, new StampingProperties());
    
    // 3. Configurer la signature PAdES
    var appearance = signer.GetSignatureAppearance();
    appearance.SetReason("Document académique officiel");
    appearance.SetLocation("Casablanca, Maroc");
    appearance.SetLayer2Text("Signé électroniquement par Université Hassan II");
    
    // 4. Créer l'external signature avec le dongle
    var externalSignature = new PrivateKeySignature(cert.PrivateKey, DigestAlgorithms.SHA256);
    
    // 5. Signer le PDF (PAdES-B-LT)
    signer.SignDetached(externalSignature, chain, null, null, null, 0, 
        PdfSigner.CryptoStandard.CADES);
    
    return outputStream.ToArray();
}
```

**And** la signature inclut :
- Format : PAdES-B-LT (PDF Advanced Electronic Signature - Long Term)
- Algorithme de hash : SHA-256
- Certificat chain complet (certificat + intermédiaires + root CA)
- Signature visible avec texte "Signé électroniquement"
- Position : En bas à gauche du document

**And** un test vérifie que :
- Le PDF signé est valide
- La signature est détectable par Adobe Acrobat Reader
- Le certificat chain est complet
- Le hash SHA-256 est correct

**And** FR15 est implémenté (signature PAdES)

---

### Story 4.4: Implémenter Validation Certificat OCSP/CRL et RFC 3161 Timestamping

As a **système AcadSign**,
I want **valider le certificat via OCSP/CRL et ajouter un timestamp RFC 3161**,
So that **la signature est non-répudiable et légalement valide à long terme**.

**Acceptance Criteria:**

**Given** un document est en cours de signature
**When** le certificat est récupéré du dongle
**Then** le système valide le statut du certificat via OCSP :
```csharp
public async Task<CertificateStatus> ValidateCertificateAsync(X509Certificate2 cert)
{
    // 1. Construire la requête OCSP
    var ocspReq = new OcspReqGenerator();
    var certId = new CertificateID(
        CertificateID.HASH_SHA1,
        issuerCert,
        cert.SerialNumber);
    ocspReq.AddRequest(certId);
    
    // 2. Envoyer à l'OCSP responder Barid Al-Maghrib
    var ocspUrl = "http://ocsp.baridmb.ma";
    var response = await SendOcspRequestAsync(ocspUrl, ocspReq.Generate());
    
    // 3. Vérifier le statut
    if (response.Status == OcspResponseStatus.Successful)
    {
        var basicResp = (BasicOcspResp)response.GetResponseObject();
        var status = basicResp.Responses[0].GetCertStatus();
        
        if (status == CertificateStatus.Good)
            return CertificateStatus.Valid;
        else if (status is RevokedStatus)
            return CertificateStatus.Revoked;
    }
    
    return CertificateStatus.Unknown;
}
```

**And** si OCSP échoue, fallback vers CRL :
```csharp
var crlUrl = "http://crl.baridmb.ma/barid.crl";
var crl = await DownloadCrlAsync(crlUrl);
var isRevoked = crl.IsRevoked(cert);
```

**And** un timestamp RFC 3161 est ajouté à la signature :
```csharp
var tsaClient = new TSAClientBouncyCastle("http://tsa.baridmb.ma");
signer.SignDetached(externalSignature, chain, null, null, tsaClient, 0, 
    PdfSigner.CryptoStandard.CADES);
```

**And** le timestamp prouve la date/heure exacte de la signature (non-répudiation)

**And** si le certificat est REVOKED, la signature est bloquée avec message d'erreur :
"❌ Certificat révoqué - Veuillez contacter Barid Al-Maghrib"

**And** si le certificat est EXPIRED, alerte affichée :
"⚠️ Certificat expiré le {date} - Renouvellement requis"

**And** FR16 et FR17 sont implémentés (OCSP/CRL + RFC 3161)

**And** NFR-S13 est respecté (validation certificat avant chaque signature)

---

### Story 4.5: Implémenter Communication Desktop App ↔ Backend API (Refit)

As a **Desktop App**,
I want **communiquer avec le Backend API de manière type-safe avec retry logic**,
So that **je peux récupérer les documents à signer et uploader les documents signés de manière fiable**.

**Acceptance Criteria:**

**Given** le Backend API expose les endpoints REST
**When** j'installe les packages NuGet :
- `Refit` version 10.0.1
- `Refit.HttpClientFactory` version 10.0.1
- `Polly` version 8.5.0

**Then** une interface Refit est créée pour l'API Backend :
```csharp
public interface IAcadSignApi
{
    [Get("/api/v1/documents/pending")]
    Task<List<DocumentDto>> GetPendingDocumentsAsync();
    
    [Get("/api/v1/documents/{documentId}/unsigned")]
    Task<Stream> GetUnsignedDocumentAsync(Guid documentId);
    
    [Post("/api/v1/documents/{documentId}/upload-signed")]
    [Multipart]
    Task<DocumentResponse> UploadSignedDocumentAsync(
        Guid documentId,
        [AliasAs("signedPdf")] StreamPart signedPdf,
        [AliasAs("certificateSerial")] string certificateSerial,
        [AliasAs("signatureTimestamp")] DateTime signatureTimestamp);
}
```

**And** Refit est configuré avec Polly pour resilience :
```csharp
services.AddRefitClient<IAcadSignApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.acadsign.ma"))
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => 
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}
```

**And** les requêtes incluent automatiquement le JWT token :
```csharp
services.AddHttpClient("AcadSignApi")
    .AddHttpMessageHandler<AuthHeaderHandler>();

public class AuthHeaderHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenStorage.GetAccessTokenAsync();
        request.Headers.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}
```

**And** un test vérifie :
- Récupération d'un document non signé réussit
- Upload d'un document signé réussit
- Retry automatique après échec temporaire (3 tentatives)
- Circuit breaker s'ouvre après 5 échecs consécutifs

**And** FR7 et FR18 sont implémentés

---

### Story 4.6: Implémenter Batch Signing avec Progress Tracking

As a **Fatima (registrar staff)**,
I want **signer 50 documents en batch avec une progress bar**,
So that **je peux traiter efficacement les demandes en masse**.

**Acceptance Criteria:**

**Given** Fatima a sélectionné 50 documents à signer
**When** elle clique sur "Signer le Batch"
**Then** la Desktop App :

1. **Demande le PIN une seule fois** au début du batch
2. **Télécharge tous les PDFs non signés** en parallèle (max 5 concurrent)
3. **Signe chaque PDF séquentiellement** avec le dongle
4. **Upload chaque PDF signé** immédiatement après signature
5. **Affiche la progress bar** en temps réel

**And** le ViewModel gère le batch signing :
```csharp
[ObservableProperty]
private int _totalDocuments;

[ObservableProperty]
private int _processedDocuments;

[ObservableProperty]
private int _failedDocuments;

[ObservableProperty]
private string _currentDocumentName;

[RelayCommand]
private async Task SignBatchAsync(List<DocumentDto> documents)
{
    TotalDocuments = documents.Count;
    ProcessedDocuments = 0;
    FailedDocuments = 0;
    
    // Demander PIN une fois
    var pin = await PromptForPinAsync();
    
    foreach (var doc in documents)
    {
        CurrentDocumentName = doc.StudentName;
        
        try
        {
            // 1. Télécharger PDF non signé
            var unsignedPdf = await _apiClient.GetUnsignedDocumentAsync(doc.Id);
            
            // 2. Signer avec dongle
            var signedPdf = await _signatureService.SignPdfAsync(unsignedPdf, pin);
            
            // 3. Upload PDF signé
            await _apiClient.UploadSignedDocumentAsync(doc.Id, signedPdf, ...);
            
            ProcessedDocuments++;
        }
        catch (Exception ex)
        {
            FailedDocuments++;
            _logger.LogError(ex, "Échec signature document {DocId}", doc.Id);
        }
    }
    
    ShowCompletionSummary();
}
```

**And** l'UI affiche :
- Progress bar : "Signature en cours... 35/50 documents"
- Document actuel : "Signature de : Ahmed Ben Ali - Attestation de Scolarité"
- Temps estimé restant : "~5 minutes restantes"
- Succès/Échecs : "✅ 35 réussis | ❌ 2 échecs"

**And** si le dongle est déconnecté pendant le batch :
- Pause automatique
- Alerte : "⚠️ Dongle déconnecté - Veuillez rebrancher le dongle"
- Reprise automatique après reconnexion

**And** un rapport final est affiché :
```
✅ Batch terminé !
- Total : 50 documents
- Réussis : 48 documents
- Échecs : 2 documents
- Durée : 8 minutes 32 secondes
```

**And** les documents échoués sont listés pour retry manuel

**And** FR19 et FR20 sont implémentés

**And** NFR-P3 est respecté (< 30 secondes par document)

**And** NFR-U3 est respecté (progress bar avec temps estimé)

---

## Epic 5: Batch Processing & Background Jobs

Permettre le traitement en masse de documents (jusqu'à 500 documents par batch). Fatima peut traiter 500 demandes d'attestations en 8 minutes au lieu de 2 jours.

### Story 5.1: Configurer Hangfire pour Background Jobs

As a **développeur backend**,
I want **configurer Hangfire 1.8.23 pour gérer les jobs asynchrones et le retry logic**,
So that **le système peut traiter des batches de 500 documents de manière fiable**.

**Acceptance Criteria:**

**Given** le projet Backend API est configuré
**When** j'installe les packages NuGet :
- `Hangfire.AspNetCore` version 1.8.23
- `Hangfire.PostgreSql` version 1.20.9

**Then** Hangfire est configuré dans `Program.cs` :
```csharp
services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(connectionString));

services.AddHangfireServer(options =>
{
    options.WorkerCount = 5;
    options.Queues = new[] { "default", "critical", "batch" };
});

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});
```

**And** les tables Hangfire sont créées dans PostgreSQL :
- `hangfire.job`
- `hangfire.state`
- `hangfire.jobqueue`
- `hangfire.server`
- `hangfire.set`
- `hangfire.hash`
- `hangfire.list`
- `hangfire.counter`

**And** le dashboard Hangfire est accessible à `/hangfire` (authentification Admin requise)

**And** la configuration de retry est définie :
- Max retry attempts : 5
- Exponential backoff : 1min, 5min, 15min, 1h, 6h

**And** un test vérifie qu'un job peut être enqueued et exécuté

---

### Story 5.2: Implémenter Batch Document Generation Endpoint

As a **SIS Laravel**,
I want **soumettre un batch de 500 documents à générer en une seule requête**,
So that **je peux automatiser la génération massive de documents**.

**Acceptance Criteria:**

**Given** Hangfire est configuré
**When** je crée l'endpoint `POST /api/v1/documents/batch`
**Then** l'endpoint accepte un payload JSON :
```json
{
  "batchId": "optional-uuid",
  "documents": [
    {
      "studentId": "12345",
      "firstName": "Ahmed",
      "lastName": "Ben Ali",
      "documentType": "ATTESTATION_SCOLARITE",
      ...
    },
    // ... 499 autres documents
  ]
}
```

**And** la réponse HTTP 202 Accepted est retournée immédiatement :
```json
{
  "batchId": "uuid-v4",
  "totalDocuments": 500,
  "status": "PROCESSING",
  "createdAt": "2026-03-04T10:00:00Z",
  "statusUrl": "/api/v1/documents/batch/{batchId}/status"
}
```

**And** un job Hangfire est créé pour traiter le batch :
```csharp
BackgroundJob.Enqueue<BatchProcessingService>(
    x => x.ProcessBatchAsync(batchId, documents));
```

**And** le job traite les documents en parallèle (5 workers Hangfire)

**And** chaque document est généré individuellement et stocké sur S3

**And** le statut du batch est mis à jour en temps réel dans la base de données

**And** FR5, FR6, FR38 sont implémentés

---

### Story 5.3: Implémenter Batch Status Polling Endpoint

As a **SIS Laravel**,
I want **interroger le statut d'un batch en cours de traitement**,
So that **je peux afficher la progression à l'utilisateur**.

**Acceptance Criteria:**

**Given** un batch est en cours de traitement
**When** j'appelle `GET /api/v1/documents/batch/{batchId}/status`
**Then** la réponse contient :
```json
{
  "batchId": "uuid-v4",
  "status": "PROCESSING",
  "totalDocuments": 500,
  "processedDocuments": 350,
  "failedDocuments": 2,
  "startedAt": "2026-03-04T10:00:00Z",
  "estimatedCompletionAt": "2026-03-04T10:12:00Z",
  "documents": [
    {
      "documentId": "uuid",
      "studentId": "12345",
      "status": "SIGNED",
      "downloadUrl": "https://...",
      "error": null
    },
    {
      "documentId": "uuid",
      "studentId": "67890",
      "status": "FAILED",
      "error": "Invalid student data: CIN format incorrect"
    }
  ]
}
```

**And** le statut peut être :
- `PROCESSING` : En cours
- `COMPLETED` : Tous les documents traités avec succès
- `PARTIAL` : Certains documents ont échoué
- `FAILED` : Échec complet du batch

**And** l'endpoint est rate-limited à 200 req/min par client

**And** un webhook peut être configuré pour notification async (optionnel)

**And** FR39 est implémenté

---

### Story 5.4: Implémenter Dead-Letter Queue et Retry Logic

As a **système AcadSign**,
I want **capturer 100% des échecs de signature dans une dead-letter queue avec retry automatique**,
So that **aucun document n'est perdu et les échecs temporaires sont réessayés**.

**Acceptance Criteria:**

**Given** un document échoue lors de la signature
**When** l'exception est capturée par Hangfire
**Then** le job est automatiquement réessayé selon la politique :
- Tentative 1 : Immédiatement
- Tentative 2 : Après 1 minute
- Tentative 3 : Après 5 minutes
- Tentative 4 : Après 15 minutes
- Tentative 5 : Après 1 heure
- Tentative 6 : Après 6 heures

**And** après 6 échecs, le job est déplacé vers la dead-letter queue :
```csharp
[AutomaticRetry(Attempts = 6, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
public async Task SignDocumentAsync(Guid documentId)
{
    try
    {
        // Logique de signature
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Signature failed for document {DocId}", documentId);
        
        if (IsRetryableException(ex))
        {
            throw; // Hangfire va retry
        }
        else
        {
            await MoveToDeadLetterQueueAsync(documentId, ex);
        }
    }
}
```

**And** la dead-letter queue est stockée dans une table PostgreSQL :
```sql
CREATE TABLE dead_letter_queue (
    id UUID PRIMARY KEY,
    document_id UUID NOT NULL,
    error_message TEXT,
    stack_trace TEXT,
    retry_count INT,
    created_at TIMESTAMP,
    last_retry_at TIMESTAMP
);
```

**And** un dashboard admin affiche les jobs en dead-letter queue

**And** un admin peut manuellement retry un job depuis le dashboard

**And** NFR-R4 est implémenté (dead-letter queue avec retry automatique)

---

### Story 5.5: Optimiser Performance Batch Processing

As a **Fatima (registrar staff)**,
I want **que 500 documents soient signés en moins de 15 minutes**,
So that **je peux traiter rapidement les demandes de fin de semestre**.

**Acceptance Criteria:**

**Given** un batch de 500 documents est soumis
**When** le traitement démarre
**Then** les optimisations suivantes sont appliquées :

**Parallélisation :**
- 5 workers Hangfire traitent les documents en parallèle
- Chaque worker peut signer 1 document à la fois
- Throughput théorique : 5 docs/30s = 10 docs/min = 600 docs/h

**Connection Pooling :**
```csharp
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MaxBatchSize(100);
        npgsqlOptions.CommandTimeout(30);
    }));

services.AddSingleton<IMinioClient>(sp =>
{
    var client = new MinioClient()
        .WithEndpoint("minio.acadsign.ma")
        .WithCredentials(accessKey, secretKey)
        .WithHttpClient(new HttpClient { Timeout = TimeSpan.FromMinutes(5) })
        .Build();
    return client;
});
```

**Caching :**
- Templates PDF mis en cache (MemoryCache)
- Certificats validés OCSP mis en cache (5 minutes)
- Métadonnées documents en cache (Redis optionnel)

**Async I/O :**
- Tous les appels S3 sont async
- Tous les appels PostgreSQL sont async
- Pas de blocking calls

**And** un test de performance vérifie :
- 500 documents signés en < 15 minutes
- CPU usage < 80%
- Memory usage < 4 GB
- PostgreSQL connections < 50

**And** NFR-P4 est respecté (500 docs en < 15 min)

---

## Epic 6: Public Verification Portal

Permettre à n'importe qui (employeurs, agences gouvernementales) de vérifier l'authenticité des documents signés. Sarah (recruteuse RH) peut scanner le QR code et vérifier instantanément qu'un document est authentique.

### Story 6.1: Créer Public Verification Web Page

As a **Sarah (recruteuse RH)**,
I want **accéder à un portail web public pour vérifier un document**,
So that **je peux confirmer l'authenticité d'un document académique sans authentification**.

**Acceptance Criteria:**

**Given** un document signé avec QR code
**When** Sarah scanne le QR code avec son smartphone
**Then** elle est redirigée vers `https://verify.acadsign.ma/documents/{documentId}`

**And** la page web affiche :
- Logo AcadSign
- Titre : "Vérification de Document Académique"
- Formulaire de saisie manuelle : "Entrez l'ID du document"
- Bouton : "Vérifier"

**And** la page est responsive (mobile-first design)

**And** la page supporte français et arabe (switch langue)

**And** aucune authentification n'est requise (endpoint public)

**And** NFR-U5 est respecté (mobile-responsive)

---

### Story 6.2: Implémenter Endpoint de Vérification Publique

As a **système AcadSign**,
I want **exposer un endpoint public pour vérifier la signature d'un document**,
So that **n'importe qui peut valider l'authenticité sans credentials**.

**Acceptance Criteria:**

**Given** un document ID valide
**When** Sarah appelle `GET /api/v1/documents/verify/{documentId}`
**Then** l'endpoint :
1. Récupère le document signé depuis S3
2. Valide la signature PAdES cryptographiquement
3. Vérifie le statut du certificat (OCSP/CRL)
4. Retourne les métadonnées du document

**And** la réponse contient :
```json
{
  "documentId": "uuid-v4",
  "isValid": true,
  "documentType": "Attestation de Scolarité",
  "issuedBy": "Université Hassan II Casablanca",
  "studentName": "Ahmed Ben Ali",
  "signedAt": "2026-03-04T10:30:00Z",
  "certificateSerial": "1234567890ABCDEF",
  "certificateStatus": "VALID",
  "certificateValidUntil": "2027-03-04T00:00:00Z",
  "certificateIssuer": "Barid Al-Maghrib PKI",
  "signatureAlgorithm": "SHA256withRSA",
  "timestampAuthority": "Barid Al-Maghrib TSA"
}
```

**And** si le document n'existe pas : HTTP 404 Not Found

**And** si la signature est invalide :
```json
{
  "isValid": false,
  "error": "Signature cryptographique invalide",
  "reason": "Certificate chain validation failed"
}
```

**And** si le certificat est révoqué :
```json
{
  "isValid": false,
  "certificateStatus": "REVOKED",
  "revokedAt": "2026-02-01T00:00:00Z"
}
```

**And** l'endpoint est rate-limited à 1000 req/min (global, toutes IPs)

**And** FR21, FR22, FR23, FR24, FR25, FR26 sont implémentés

**And** NFR-P5 est respecté (< 2 secondes response time)

---

### Story 6.3: Afficher Résultat de Vérification avec UI/UX Claire

As a **Sarah (recruteuse RH)**,
I want **voir clairement si un document est authentique ou non**,
So that **je peux prendre une décision de recrutement en toute confiance**.

**Acceptance Criteria:**

**Given** Sarah a soumis un document ID pour vérification
**When** la vérification est terminée
**Then** l'UI affiche :

**Si document VALIDE :**
```
✅ Document Authentique

Type : Attestation de Scolarité
Émis par : Université Hassan II Casablanca
Étudiant : Ahmed Ben Ali
Date de signature : 04 mars 2026 à 10h30

Certificat de signature :
✅ Valide jusqu'au 04 mars 2027
✅ Émis par : Barid Al-Maghrib PKI
✅ Numéro de série : 1234567890ABCDEF

Signature électronique :
✅ Algorithme : SHA256withRSA
✅ Horodatage : 04 mars 2026 à 10h30 (RFC 3161)
✅ Autorité d'horodatage : Barid Al-Maghrib TSA

Ce document est légalement valide au Maroc.
```

**Si document INVALIDE :**
```
❌ Document Non Authentique

⚠️ La signature électronique de ce document est invalide.

Raison : Validation de la chaîne de certificats échouée

Ce document ne doit PAS être considéré comme authentique.
```

**Si certificat RÉVOQUÉ :**
```
❌ Certificat Révoqué

⚠️ Le certificat utilisé pour signer ce document a été révoqué.

Date de révocation : 01 février 2026

Ce document n'est plus valide.
```

**And** les couleurs sont claires :
- Vert pour VALIDE
- Rouge pour INVALIDE/RÉVOQUÉ
- Orange pour EXPIRÉ

**And** un bouton "Télécharger le Rapport de Vérification (PDF)" est disponible

**And** le rapport PDF contient toutes les informations de vérification

---

## Epic 7: SIS Integration & API

Permettre au SIS Laravel de l'université d'intégrer AcadSign pour automatiser la génération de documents. Omar (développeur SIS) peut intégrer AcadSign avec le système existant en quelques jours.

### Story 7.1: Générer OpenAPI 3.0 Specification Complète

As a **Omar (développeur SIS)**,
I want **une spécification OpenAPI 3.0 complète et à jour**,
So that **je peux générer automatiquement un client API pour Laravel**.

**Acceptance Criteria:**

**Given** tous les endpoints API sont implémentés
**When** je configure Swashbuckle/Scalar dans le Backend API
**Then** la spécification OpenAPI 3.0 est générée automatiquement à `/api/v1/swagger.json`

**And** le Swagger UI interactif est disponible à `/api/v1/docs`

**And** la spécification inclut :
- Tous les 11 endpoints REST
- Schémas JSON complets (request/response)
- Codes d'erreur HTTP avec exemples
- Authentification OAuth 2.0 flows
- Rate limiting headers
- Exemples de requêtes/réponses

**And** les schémas sont validés avec JSON Schema :
```yaml
components:
  schemas:
    StudentData:
      type: object
      required:
        - studentId
        - firstName
        - lastName
        - cin
        - documentType
      properties:
        studentId:
          type: string
        firstName:
          type: string
        cin:
          type: string
          pattern: '^[A-Z]{1,2}[0-9]{6}$'
        documentType:
          type: string
          enum:
            - ATTESTATION_SCOLARITE
            - RELEVE_NOTES
            - ATTESTATION_REUSSITE
            - ATTESTATION_INSCRIPTION
```

**And** FR61, NFR-I2, NFR-I3 sont implémentés

---

### Story 7.2: Implémenter JSON Schema Validation

As a **Backend API**,
I want **valider strictement tous les payloads JSON entrants**,
So that **les données invalides sont rejetées avant traitement**.

**Acceptance Criteria:**

**Given** le SIS Laravel envoie un payload JSON
**When** le payload arrive à l'endpoint API
**Then** FluentValidation valide le payload :
```csharp
public class GenerateDocumentRequestValidator : AbstractValidator<GenerateDocumentRequest>
{
    public GenerateDocumentRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty()
            .MaximumLength(50);
        
        RuleFor(x => x.CIN)
            .NotEmpty()
            .Matches(@"^[A-Z]{1,2}[0-9]{6}$")
            .WithMessage("CIN format invalide. Format attendu: A123456 ou AB123456");
        
        RuleFor(x => x.CNE)
            .NotEmpty()
            .Matches(@"^[A-Z0-9]{10}$")
            .WithMessage("CNE format invalide. Format attendu: 10 caractères alphanumériques");
        
        RuleFor(x => x.DocumentType)
            .IsInEnum()
            .WithMessage("Type de document invalide");
        
        RuleFor(x => x.AcademicYear)
            .Matches(@"^[0-9]{4}-[0-9]{4}$")
            .WithMessage("Année académique invalide. Format attendu: 2025-2026");
    }
}
```

**And** si la validation échoue, HTTP 400 Bad Request est retourné :
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Les données fournies sont invalides",
    "details": [
      {
        "field": "cin",
        "message": "CIN format invalide. Format attendu: A123456 ou AB123456"
      },
      {
        "field": "academicYear",
        "message": "Année académique invalide. Format attendu: 2025-2026"
      }
    ],
    "timestamp": "2026-03-04T10:00:00Z",
    "requestId": "uuid-correlation-id"
  }
}
```

**And** tous les champs requis sont validés

**And** les formats (CIN, CNE, dates) sont validés avec regex

**And** FR35 et NFR-I5 sont implémentés

---

### Story 7.3: Implémenter Webhook Notifications (Optionnel)

As a **SIS Laravel**,
I want **recevoir une notification webhook quand un document est prêt**,
So that **je n'ai pas besoin de poller le statut en boucle**.

**Acceptance Criteria:**

**Given** le SIS Laravel a configuré une URL webhook
**When** un document est signé et prêt
**Then** le Backend API envoie une requête POST au webhook :
```http
POST https://sis.university.ma/webhooks/document-ready
Content-Type: application/json
X-AcadSign-Signature: HMAC-SHA256-signature

{
  "event": "document.signed",
  "documentId": "uuid-v4",
  "studentId": "12345",
  "documentType": "ATTESTATION_SCOLARITE",
  "status": "SIGNED",
  "downloadUrl": "https://api.acadsign.ma/documents/{id}/download",
  "signedAt": "2026-03-04T10:30:00Z"
}
```

**And** le webhook est signé avec HMAC-SHA256 pour vérifier l'authenticité

**And** si le webhook échoue, retry automatique (3 tentatives avec exponential backoff)

**And** un endpoint permet de configurer le webhook :
```http
POST /api/v1/webhooks
Authorization: Bearer {jwt_token}

{
  "url": "https://sis.university.ma/webhooks/document-ready",
  "events": ["document.signed", "batch.completed"],
  "secret": "webhook-secret-key"
}
```

**And** FR37 et NFR-I4 sont implémentés

---

### Story 7.4: Implémenter Rate Limiting par Endpoint

As a **Backend API**,
I want **limiter le nombre de requêtes par client pour éviter les abus**,
So that **le système reste disponible pour tous les utilisateurs**.

**Acceptance Criteria:**

**Given** un client API fait des requêtes
**When** le rate limit est atteint
**Then** HTTP 429 Too Many Requests est retourné :
```http
HTTP/1.1 429 Too Many Requests
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 1709553600
Retry-After: 60

{
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Limite de requêtes dépassée. Réessayez dans 60 secondes."
  }
}
```

**And** les limites par endpoint sont :
- `POST /api/v1/documents/generate` : 100 req/min par client JWT
- `POST /api/v1/documents/batch` : 10 req/min par client JWT
- `GET /api/v1/documents/verify` : 1000 req/min (global)
- Tous les autres endpoints : 200 req/min par client JWT

**And** le rate limiting utilise Redis (ou MemoryCache en dev) :
```csharp
services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("document-generation", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });
});

app.MapPost("/api/v1/documents/generate", ...)
    .RequireRateLimiting("document-generation");
```

**And** les headers de rate limit sont retournés sur chaque réponse

**And** FR59, FR60, NFR-I8 sont implémentés

---

## Epic 8: Audit Trail & Compliance

Assurer la conformité CNDP (Loi 53-05) et la traçabilité complète de tous les événements. L'université peut prouver la conformité légale et tracer chaque action sur chaque document pendant 30 ans.

### Story 8.1: Implémenter Audit Trail Immuable

As a **système AcadSign**,
I want **logger tous les événements de cycle de vie des documents dans un audit trail immuable**,
So that **chaque action est traçable pendant 30 ans pour conformité légale**.

**Acceptance Criteria:**

**Given** une action est effectuée sur un document
**When** l'événement se produit
**Then** une entrée d'audit est créée dans la table `audit_logs` :
```sql
CREATE TABLE audit_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    document_id UUID NOT NULL,
    event_type VARCHAR(50) NOT NULL,
    user_id UUID,
    ip_address INET,
    user_agent TEXT,
    certificate_serial VARCHAR(100),
    metadata JSONB,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    correlation_id UUID NOT NULL
);

CREATE INDEX idx_audit_logs_document_id ON audit_logs(document_id);
CREATE INDEX idx_audit_logs_event_type ON audit_logs(event_type);
CREATE INDEX idx_audit_logs_created_at ON audit_logs(created_at);
```

**And** les événements suivants sont loggés :
- `DOCUMENT_GENERATED` : Document PDF créé
- `DOCUMENT_SIGNED` : Signature PAdES appliquée
- `DOCUMENT_UPLOADED` : Upload sur S3 réussi
- `DOCUMENT_DOWNLOADED` : Téléchargement par étudiant
- `DOCUMENT_VERIFIED` : Vérification publique effectuée
- `CERTIFICATE_VALIDATED` : Validation OCSP/CRL
- `TEMPLATE_UPLOADED` : Nouveau template ajouté
- `USER_LOGIN` : Connexion utilisateur
- `USER_LOGOUT` : Déconnexion utilisateur

**And** chaque log contient :
```json
{
  "documentId": "uuid",
  "eventType": "DOCUMENT_SIGNED",
  "userId": "uuid-fatima",
  "ipAddress": "192.168.1.100",
  "userAgent": "AcadSign.Desktop/1.0",
  "certificateSerial": "1234567890ABCDEF",
  "metadata": {
    "signatureAlgorithm": "SHA256withRSA",
    "timestampAuthority": "Barid Al-Maghrib TSA",
    "documentType": "ATTESTATION_SCOLARITE"
  },
  "createdAt": "2026-03-04T10:30:00Z",
  "correlationId": "uuid-request-trace"
}
```

**And** la table `audit_logs` est append-only (pas de UPDATE ni DELETE)

**And** un trigger PostgreSQL bloque toute tentative de modification :
```sql
CREATE OR REPLACE FUNCTION prevent_audit_modification()
RETURNS TRIGGER AS $$
BEGIN
    RAISE EXCEPTION 'Modification des logs d''audit interdite';
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER audit_logs_immutable
BEFORE UPDATE OR DELETE ON audit_logs
FOR EACH ROW EXECUTE FUNCTION prevent_audit_modification();
```

**And** FR45, FR46, NFR-S10 sont implémentés

---

### Story 8.2: Implémenter Endpoint Audit Trail pour Auditors

As a **auditeur CNDP**,
I want **accéder à l'audit trail complet d'un document**,
So that **je peux vérifier la conformité légale et tracer toutes les actions**.

**Acceptance Criteria:**

**Given** un utilisateur avec rôle `Auditor`
**When** il appelle `GET /api/v1/audit/{documentId}`
**Then** la réponse contient tous les événements du document :
```json
{
  "documentId": "uuid-v4",
  "events": [
    {
      "eventType": "DOCUMENT_GENERATED",
      "timestamp": "2026-03-04T10:00:00Z",
      "userId": "uuid-sis-client",
      "ipAddress": "10.0.1.50",
      "metadata": {
        "documentType": "ATTESTATION_SCOLARITE",
        "studentId": "12345"
      }
    },
    {
      "eventType": "DOCUMENT_SIGNED",
      "timestamp": "2026-03-04T10:30:00Z",
      "userId": "uuid-fatima",
      "certificateSerial": "1234567890ABCDEF",
      "metadata": {
        "signatureAlgorithm": "SHA256withRSA"
      }
    },
    {
      "eventType": "DOCUMENT_DOWNLOADED",
      "timestamp": "2026-03-04T10:35:00Z",
      "userId": "uuid-student",
      "ipAddress": "41.250.10.20"
    },
    {
      "eventType": "DOCUMENT_VERIFIED",
      "timestamp": "2026-03-05T14:00:00Z",
      "ipAddress": "102.50.30.10",
      "metadata": {
        "verificationResult": "VALID"
      }
    }
  ],
  "totalEvents": 4
}
```

**And** un endpoint de recherche permet de filtrer les logs :
```http
GET /api/v1/audit/search?eventType=DOCUMENT_SIGNED&startDate=2026-03-01&endDate=2026-03-31&limit=100
```

**And** seuls les utilisateurs avec rôle `Auditor` ou `Admin` peuvent accéder aux logs

**And** les logs sont retournés par ordre chronologique

**And** FR47 est implémenté

---

### Story 8.3: Implémenter Student Rights API (Access, Rectification, Deletion)

As a **étudiant**,
I want **accéder à mes données personnelles, les rectifier ou les supprimer**,
So that **mes droits CNDP (Loi 53-05) sont respectés**.

**Acceptance Criteria:**

**Given** un étudiant authentifié
**When** il appelle les endpoints suivants
**Then** les opérations sont effectuées :

**Accès aux données (Right to Access) :**
```http
GET /api/v1/students/{studentId}/data
Authorization: Bearer {student_token}

Response:
{
  "studentId": "12345",
  "firstName": "Ahmed",
  "lastName": "Ben Ali",
  "cin": "A123456",
  "cne": "ABC1234567",
  "email": "ahmed@example.com",
  "phoneNumber": "+212612345678",
  "documents": [
    {
      "documentId": "uuid",
      "documentType": "ATTESTATION_SCOLARITE",
      "createdAt": "2026-03-04T10:00:00Z",
      "status": "SIGNED"
    }
  ],
  "dataCollectedAt": "2025-09-01T00:00:00Z",
  "dataRetentionUntil": "2056-09-01T00:00:00Z"
}
```

**Rectification des données (Right to Rectification) :**
```http
PUT /api/v1/students/{studentId}/data
Authorization: Bearer {student_token}

{
  "email": "new-email@example.com",
  "phoneNumber": "+212698765432"
}

Response: 200 OK
```

**Suppression des données (Right to Erasure) :**
```http
DELETE /api/v1/students/{studentId}/data
Authorization: Bearer {student_token}

Response: 204 No Content
```

**And** la suppression respecte les contraintes légales :
- Documents académiques : **NON supprimables** (rétention 30 ans obligatoire)
- Données PII (email, phone) : **Anonymisables** après obtention du diplôme
- Logs d'audit : **NON supprimables** (rétention 10 ans minimum)

**And** une demande de suppression crée un ticket pour validation manuelle par l'admin

**And** FR50 et NFR-C6 sont implémentés

---

### Story 8.4: Générer CNDP Compliance Reports

As a **administrateur IT**,
I want **générer des rapports de conformité CNDP sur demande**,
So that **l'université peut prouver sa conformité lors d'un audit**.

**Acceptance Criteria:**

**Given** un utilisateur Admin
**When** il appelle `GET /api/v1/compliance/report?startDate=2026-01-01&endDate=2026-03-31`
**Then** un rapport PDF est généré contenant :

**Section 1 : Données Collectées**
- Types de données : CIN, CNE, Email, Phone, Nom, Prénom
- Finalité : Délivrance de documents académiques officiels
- Base légale : Loi 53-05, Loi 43-20

**Section 2 : Mesures de Sécurité**
- Chiffrement en transit : TLS 1.3
- Chiffrement au repos : SSE-KMS (S3) + AES-256-GCM (PII)
- Authentification : OAuth 2.0 + JWT
- Contrôle d'accès : RBAC (4 rôles)

**Section 3 : Rétention des Données**
- Documents académiques : 30 ans
- Logs d'audit : 10 ans minimum
- Données PII temporaires : Suppression après traitement

**Section 4 : Droits des Étudiants**
- Accès : API disponible
- Rectification : API disponible
- Suppression : Procédure manuelle (contraintes légales)

**Section 5 : Statistiques**
- Nombre de documents générés : 5,234
- Nombre de requêtes d'accès aux données : 12
- Nombre de rectifications : 3
- Nombre de demandes de suppression : 1

**And** le rapport est signé électroniquement par l'admin

**And** FR52 et NFR-C8 sont implémentés

---

## Epic 9: Email Notifications & Student Experience

Automatiser l'envoi de liens de téléchargement aux étudiants par email. Youssef reçoit son attestation par email en 3 minutes sans déplacement au campus.

### Story 9.1: Configurer Service d'Email Notifications

As a **système AcadSign**,
I want **envoyer automatiquement des emails aux étudiants avec les liens de téléchargement**,
So that **les étudiants reçoivent leurs documents sans intervention manuelle**.

**Acceptance Criteria:**

**Given** un document est signé et uploadé sur S3
**When** le système génère la pre-signed URL
**Then** un email est automatiquement envoyé à l'étudiant :

**Template Email (Français) :**
```
Objet : Votre document académique est prêt - Université Hassan II

Bonjour Ahmed Ben Ali,

Votre document académique est maintenant disponible :

Type de document : Attestation de Scolarité
Date d'émission : 04 mars 2026

Télécharger votre document :
[Lien sécurisé valide 24 heures]

Ce lien expirera le 05 mars 2026 à 10h30.

Pour vérifier l'authenticité de votre document, scannez le QR code présent sur le document ou visitez :
https://verify.acadsign.ma

Cordialement,
Service de Scolarité
Université Hassan II Casablanca
```

**Template Email (Arabe) :**
```
الموضوع: وثيقتك الأكاديمية جاهزة - جامعة الحسن الثاني

مرحبا أحمد بن علي،

وثيقتك الأكاديمية متاحة الآن:

نوع الوثيقة: شهادة مدرسية
تاريخ الإصدار: 04 مارس 2026

تحميل وثيقتك:
[رابط آمن صالح لمدة 24 ساعة]

سينتهي هذا الرابط في 05 مارس 2026 الساعة 10:30.

للتحقق من صحة وثيقتك، امسح رمز QR الموجود على الوثيقة أو قم بزيارة:
https://verify.acadsign.ma

مع أطيب التحيات،
مصلحة الشؤون الدراسية
جامعة الحسن الثاني الدار البيضاء
```

**And** le service email utilise SMTP ou SendGrid :
```csharp
public interface IEmailService
{
    Task SendDocumentReadyEmailAsync(string toEmail, DocumentMetadata doc, string downloadUrl);
}
```

**And** l'événement `EMAIL_SENT` est loggé dans l'audit trail

**And** FR11 est implémenté

---

### Story 9.2: Implémenter Retry Logic pour Emails Échoués

As a **système AcadSign**,
I want **réessayer automatiquement l'envoi d'emails échoués**,
So that **tous les étudiants reçoivent leurs documents même en cas de problème temporaire**.

**Acceptance Criteria:**

**Given** un email échoue lors de l'envoi
**When** l'exception est capturée
**Then** Hangfire retry automatiquement :
- Tentative 1 : Après 1 minute
- Tentative 2 : Après 5 minutes
- Tentative 3 : Après 15 minutes

**And** après 3 échecs, l'email est déplacé vers la dead-letter queue

**And** un admin peut manuellement retry depuis le dashboard Hangfire

**And** l'étudiant peut re-demander l'email via un endpoint :
```http
POST /api/v1/documents/{documentId}/resend-email
Authorization: Bearer {student_token}
```

**And** FR10 est implémenté

---

## Epic 10: Admin Dashboard & Monitoring

Fournir aux administrateurs une visibilité complète sur les métriques système et la santé de l'infrastructure. Karim (admin IT) peut monitorer le système en temps réel et recevoir des alertes proactives.

### Story 10.1: Configurer Serilog + Seq pour Structured Logging

As a **développeur backend**,
I want **configurer Serilog avec Seq pour centraliser les logs**,
So that **tous les logs sont searchables et traçables avec correlation IDs**.

**Acceptance Criteria:**

**Given** le Backend API et Desktop App sont opérationnels
**When** je configure Serilog dans `Program.cs` :
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", "AcadSign.Backend")
    .WriteTo.Console()
    .WriteTo.File("logs/acadsign-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();
```

**Then** tous les logs sont envoyés à Seq (conteneur Docker)

**And** chaque requête HTTP a un correlation ID unique :
```csharp
app.Use(async (context, next) =>
{
    var correlationId = Guid.NewGuid().ToString();
    context.Items["CorrelationId"] = correlationId;
    LogContext.PushProperty("CorrelationId", correlationId);
    await next();
});
```

**And** les logs structurés incluent :
```csharp
_logger.LogInformation("Document generated: {DocumentId} for student {StudentId}", documentId, studentId);
```

**And** Seq UI est accessible à `http://localhost:5341`

**And** NFR-M1 est implémenté (structured logging + correlation IDs)

---

### Story 10.2: Configurer Prometheus + Grafana pour Monitoring

As a **Karim (admin IT)**,
I want **visualiser les métriques système en temps réel dans Grafana**,
So that **je peux détecter les problèmes avant qu'ils n'impactent les utilisateurs**.

**Acceptance Criteria:**

**Given** Prometheus et Grafana sont déployés en conteneurs Docker
**When** je configure les métriques ASP.NET Core :
```csharp
services.AddOpenTelemetry()
    .WithMetrics(builder =>
    {
        builder.AddPrometheusExporter();
        builder.AddMeter("AcadSign.Backend");
        builder.AddAspNetCoreInstrumentation();
        builder.AddHttpClientInstrumentation();
    });

app.MapPrometheusScrapingEndpoint();
```

**Then** Prometheus scrape les métriques à `/metrics`

**And** un dashboard Grafana affiche :
- **Documents générés/jour** (time series)
- **Signature success rate** (gauge)
- **API response times** (p50, p95, p99)
- **Storage usage** (PostgreSQL + MinIO)
- **Certificate validity status** (days remaining)
- **Active users** (concurrent Desktop Apps)
- **Error rate** (HTTP 5xx)

**And** les métriques custom sont collectées :
```csharp
var documentsGenerated = Metrics.CreateCounter("acadsign_documents_generated_total", "Total documents generated");
var signatureSuccessRate = Metrics.CreateGauge("acadsign_signature_success_rate", "Signature success rate");
```

**And** FR53, FR54, NFR-M2 sont implémentés

---

### Story 10.3: Configurer Alerting (Certificate Expiry, Failures, Storage)

As a **Karim (admin IT)**,
I want **recevoir des alertes proactives pour les problèmes critiques**,
So that **je peux intervenir avant que le système ne tombe en panne**.

**Acceptance Criteria:**

**Given** Prometheus Alertmanager est configuré
**When** une condition d'alerte est détectée
**Then** une notification est envoyée (email + Slack)

**Alertes configurées :**

**1. Certificate Expiry (3 mois avant) :**
```yaml
- alert: CertificateExpiringSoon
  expr: acadsign_certificate_days_remaining < 90
  for: 1h
  labels:
    severity: warning
  annotations:
    summary: "Certificat Barid Al-Maghrib expire bientôt"
    description: "Le certificat expire dans {{ $value }} jours"
```

**2. Signing Failures :**
```yaml
- alert: HighSignatureFailureRate
  expr: rate(acadsign_signature_failures_total[5m]) > 0.05
  for: 5m
  labels:
    severity: critical
  annotations:
    summary: "Taux d'échec de signature élevé"
    description: "{{ $value }}% des signatures échouent"
```

**3. Storage Threshold :**
```yaml
- alert: StorageAlmostFull
  expr: acadsign_storage_usage_bytes / acadsign_storage_capacity_bytes > 0.8
  for: 10m
  labels:
    severity: warning
  annotations:
    summary: "Stockage S3 presque plein"
    description: "{{ $value }}% du stockage utilisé"
```

**4. API Downtime :**
```yaml
- alert: APIDown
  expr: up{job="acadsign-backend"} == 0
  for: 1m
  labels:
    severity: critical
  annotations:
    summary: "API Backend indisponible"
```

**And** FR55, FR56, FR57, NFR-M3 sont implémentés

---

### Story 10.4: Créer Admin Dashboard avec Métriques Temps Réel

As a **Karim (admin IT)**,
I want **un dashboard admin web pour visualiser les métriques sans ouvrir Grafana**,
So that **je peux rapidement vérifier l'état du système**.

**Acceptance Criteria:**

**Given** un utilisateur avec rôle `Admin`
**When** il accède à `/admin/dashboard`
**Then** le dashboard affiche :

**Métriques en temps réel :**
- Documents générés aujourd'hui : 1,234
- Documents signés aujourd'hui : 1,180
- Signature success rate : 99.2%
- Temps moyen de signature : 28 secondes
- Storage utilisé : 450 GB / 1 TB (45%)

**Statut des Services :**
- ✅ Backend API : Opérationnel (99.8% uptime)
- ✅ PostgreSQL : Opérationnel
- ✅ MinIO S3 : Opérationnel
- ⚠️ Seq Logging : Dégradé (latence élevée)

**Certificats :**
- Certificat Barid Al-Maghrib : Valide jusqu'au 04/03/2027 (365 jours restants)

**Jobs Hangfire :**
- Jobs en cours : 5
- Jobs réussis (24h) : 1,234
- Jobs échoués (24h) : 2
- Dead-letter queue : 0

**And** le dashboard se rafraîchit automatiquement toutes les 30 secondes

**And** FR58 est implémenté
