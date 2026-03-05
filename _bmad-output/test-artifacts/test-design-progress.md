---
stepsCompleted: ['step-01-detect-mode', 'step-02-load-context']
lastStep: 'step-02-load-context'
lastSaved: '2026-03-05'
mode: 'system-level'
project: 'AcadSign'
detected_stack: 'fullstack'
test_stack_type: '.NET 10 Backend + WPF Desktop'
inputDocuments:
  - '/Users/macbookpro/e-sign/_bmad-output/planning-artifacts/prd.md'
  - '/Users/macbookpro/e-sign/_bmad-output/planning-artifacts/architecture.md'
  - '/Users/macbookpro/e-sign/_bmad-output/planning-artifacts/epics.md'
  - '/Users/macbookpro/e-sign/_bmad-output/planning-artifacts/ux-design-specification.md'
  - '/Users/macbookpro/e-sign/_bmad-output/implementation-artifacts/*.md (43 stories)'
---

# Test Design Progress - AcadSign

## Step 1: Mode Detection - COMPLETED

**Mode Sélectionné:** System-Level Mode

**Raison:** Projet complet avec PRD, Architecture, et 43 stories implémentées (10 Epics). Le System-Level Mode permet de créer une stratégie de test globale couvrant l'ensemble du système.

**Documents Disponibles:**
- ✅ PRD: `/Users/macbookpro/e-sign/_bmad-output/planning-artifacts/prd.md`
- ✅ Architecture: `/Users/macbookpro/e-sign/_bmad-output/planning-artifacts/architecture.md`
- ✅ Epics: `/Users/macbookpro/e-sign/_bmad-output/planning-artifacts/epics.md`
- ✅ UX Design: `/Users/macbookpro/e-sign/_bmad-output/planning-artifacts/ux-design-specification.md`
- ✅ 43 Stories implémentées dans `implementation-artifacts/`
- ✅ Sprint Status: `implementation-artifacts/sprint-status.yaml`

**Outputs à Générer:**
1. `test-design-architecture.md` - Pour équipe Architecture/Dev
2. `test-design-qa.md` - Pour équipe QA

---

## Step 2: Load Context - COMPLETED

### Configuration Chargée

**Stack Détecté:** Fullstack (.NET 10 Backend + WPF Desktop)
- Backend: 10 fichiers `.csproj` détectés (Clean Architecture)
  - Application, Domain, Infrastructure, Web
  - Tests: UnitTests, IntegrationTests, FunctionalTests
- Desktop: WPF MVVM (AcadSign.Desktop.csproj)
- Pas de Playwright/Cypress détecté → Tests backend prioritaires

**Configuration TEA:**
- `test_stack_type`: auto → détecté comme `.NET fullstack`
- `tea_browser_automation`: auto
- `tea_use_playwright_utils`: true (mais non applicable - pas de tests UI browser)
- `tea_use_pactjs_utils`: true (applicable pour intégration SIS Laravel)

### Documents Chargés

**Artifacts Projet (System-Level):**
1. ✅ **PRD** (1671 lignes) - 61 FRs + 65 NFRs
   - Domaines: Document Generation, E-Signature, Verification, Auth, SIS Integration, Templates, Audit, Admin
   - Compliance: Loi 53-05 (CNDP), Loi 43-20 (e-signature)
   - Success Criteria: 99% uptime, < 30s signature, 95% student satisfaction

2. ✅ **Architecture** (2902 lignes) - Décisions techniques
   - Hybrid Architecture: Backend API + Desktop Client (USB dongle constraint)
   - Stack: .NET 10, PostgreSQL 15+, MinIO S3, WPF MVVM
   - Cryptographie: PAdES, OCSP/CRL, RFC 3161, AES-256-GCM
   - Intégrations: Barid Al-Maghrib e-Sign, SIS Laravel (OAuth 2.0)

3. ✅ **Epics** - 10 Epics, 43 Stories
   - Epic 1-4: Infrastructure, Auth, PDF Generation, E-Signature
   - Epic 5-7: Background Jobs, Verification, API Externe
   - Epic 8-10: Compliance CNDP, Notifications, Monitoring

4. ✅ **UX Design Specification** - Principes UX validés
   - Transparence Totale, Résilience Gracieuse, Efficacité Maximale
   - Confiance Visuelle, Simplicité Malgré la Complexité

### Tech Stack Analysé

**Backend (.NET 10):**
- Clean Architecture (Domain, Application, Infrastructure, Web)
- Entity Framework Core + PostgreSQL
- OpenIddict (OAuth 2.0), Hangfire (Background Jobs)
- Serilog + Seq (Logging), Prometheus + Grafana (Monitoring)

**Desktop (WPF):**
- MVVM Pattern (CommunityToolkit.Mvvm)
- USB Dongle Access (PKCS#11/Windows CSP)
- iText7 + BouncyCastle (PAdES Signature)
- Refit (API Client)

**Intégrations Critiques:**
- Barid Al-Maghrib e-Sign (PKI nationale)
- SIS Laravel (OAuth Client Credentials + Webhooks)
- MinIO S3 (SSE-KMS encryption)
- SMTP (Email notifications)

### NFRs Critiques pour Tests

**Performance:**
- API < 500ms (p95)
- Document generation < 3s
- Signature < 30s
- Batch 500 docs < 15min
- Public verification < 2s

**Sécurité:**
- TLS 1.3 (min 1.2)
- SSE-KMS (S3), AES-256-GCM (PII)
- OCSP/CRL validation
- JWT rotation 90j
- MFA admin

**Fiabilité:**
- 99% uptime
- 99.5% signature success rate
- Dead-letter queue
- RTO < 4h, RPO < 1h

**Compliance:**
- Loi 53-05 (CNDP Maroc)
- Loi 43-20 (e-signature)
- 30 ans rétention docs
- 10 ans rétention logs
- Student rights (accès, rectification, suppression)

### Prochaine Étape

Analyser les risques et la testabilité (Step 3)

---

## Step 3: Testability & Risk Assessment - COMPLETED

### 🚨 Testability Concerns (Actionable)

**TC-1: USB Dongle Hardware Dependency (CRITICAL)**
- **Concern:** Tests de signature PAdES nécessitent un dongle USB physique Barid Al-Maghrib
- **Impact:** Impossible de tester la signature en CI/CD sans hardware
- **Testability Gap:** Pas de mock/stub pour PKCS#11/Windows CSP
- **Mitigation:** 
  - Créer une abstraction `ISignatureProvider` avec implémentation mock pour tests
  - Tests unitaires utilisent mock, tests d'intégration utilisent vrai dongle
  - Documenter setup dongle pour environnement de test local
- **Owner:** Dev Team
- **Priority:** P0 (bloquant pour CI/CD)

**TC-2: Barid Al-Maghrib e-Sign API Externe (HIGH)**
- **Concern:** Dépendance à API externe (OCSP/CRL validation, timestamping RFC 3161)
- **Impact:** Tests flaky si API down, coûts potentiels pour appels API en tests
- **Testability Gap:** Pas de sandbox/mock pour Barid e-Sign
- **Mitigation:**
  - Créer mock server pour OCSP/CRL responses
  - Utiliser certificats de test auto-signés pour validation chain
  - Tests d'intégration réels marqués comme `[Category("External")]` et exécutés séparément
- **Owner:** QA Team
- **Priority:** P0 (bloquant pour tests automatisés)

**TC-3: SIS Laravel Integration (MEDIUM)**
- **Concern:** Dépendance au SIS Laravel pour données étudiants
- **Impact:** Tests nécessitent SIS mock ou données de test
- **Testability Gap:** Pas de contract testing entre Backend et SIS
- **Mitigation:**
  - Implémenter Pact.js consumer tests (Backend = consumer, SIS = provider)
  - Créer fixtures JSON avec données étudiants de test
  - Webhook testing avec mock server
- **Owner:** Dev Team
- **Priority:** P1 (important mais contournable avec fixtures)

**TC-4: MinIO S3 State Management (MEDIUM)**
- **Concern:** Tests doivent gérer l'état S3 (upload/download/delete)
- **Impact:** Tests peuvent laisser des artifacts orphelins
- **Testability Gap:** Pas de cleanup automatique après tests
- **Mitigation:**
  - Utiliser MinIO Testcontainers pour isolation complète
  - Cleanup hooks dans `[TearDown]` pour supprimer buckets de test
  - Préfixer tous les objets de test avec `test-{guid}`
- **Owner:** Dev Team
- **Priority:** P2 (amélioration qualité tests)

**TC-5: PostgreSQL Database State (MEDIUM)**
- **Concern:** Tests d'intégration modifient la DB
- **Impact:** Tests non isolés peuvent interférer
- **Testability Gap:** Pas de stratégie de rollback/cleanup
- **Mitigation:**
  - Utiliser transactions avec rollback dans tests d'intégration
  - PostgreSQL Testcontainers pour isolation complète
  - Seed data déterministe avec `Respawn` library
- **Owner:** Dev Team
- **Priority:** P2 (déjà partiellement résolu avec Testcontainers)

**TC-6: Email SMTP Testing (LOW)**
- **Concern:** Envoi emails réels en tests pollue boîtes mail
- **Impact:** Spam, coûts SMTP
- **Testability Gap:** Pas de mock SMTP
- **Mitigation:**
  - Utiliser MailHog ou smtp4dev pour capture emails en local
  - Tests vérifient emails capturés sans envoi réel
  - Mock `IEmailService` pour tests unitaires
- **Owner:** QA Team
- **Priority:** P3 (nice-to-have)

**TC-7: Background Jobs Hangfire (MEDIUM)**
- **Concern:** Tests de jobs async difficiles à synchroniser
- **Impact:** Tests flaky avec timing issues
- **Testability Gap:** Pas de mode synchrone pour tests
- **Mitigation:**
  - Configurer Hangfire en mode in-memory pour tests
  - Utiliser `BackgroundJob.Enqueue` avec attente synchrone
  - Tests vérifient état final après job completion
- **Owner:** Dev Team
- **Priority:** P1 (important pour fiabilité tests)

**TC-8: Bilingual PDF Generation (MEDIUM)**
- **Concern:** Validation layout RTL (arabe) + LTR (français) complexe
- **Impact:** Bugs visuels difficiles à détecter automatiquement
- **Testability Gap:** Pas de visual regression testing
- **Mitigation:**
  - Tests unitaires vérifient contenu texte (pas layout)
  - Visual regression tests avec Percy.io ou Applitools (optionnel)
  - Tests manuels pour validation layout final
- **Owner:** QA Team
- **Priority:** P2 (acceptable avec tests manuels)

### ✅ Testability Assessment Summary (Strengths)

**Strong Testability Foundations:**

1. **Clean Architecture** ✅
   - Séparation Domain/Application/Infrastructure facilite mocking
   - Dependency Injection native (.NET) permet substitution facile
   - Tests unitaires Domain/Application sans dépendances externes

2. **Existing Test Projects** ✅
   - `Application.UnitTests`, `Domain.UnitTests` déjà configurés
   - `Infrastructure.IntegrationTests`, `Application.FunctionalTests` présents
   - Structure de tests alignée avec architecture

3. **Observability Built-in** ✅
   - Serilog + Seq pour structured logging
   - Correlation IDs pour traçabilité
   - Prometheus + Grafana pour métriques
   - Tests peuvent vérifier logs et métriques

4. **API-First Design** ✅
   - OpenAPI 3.0 spec facilite contract testing
   - JSON Schema validation permet tests de conformité
   - Endpoints REST testables avec HttpClient

5. **Immutable Audit Trail** ✅
   - Audit logs immuables facilitent vérification tests
   - Tests peuvent valider compliance CNDP via audit trail

### Architecturally Significant Requirements (ASRs)

**ASR-1: Signature PAdES Compliance (ACTIONABLE)**
- **Requirement:** Signatures doivent être conformes PAdES-B-LT (ISO 32000-2)
- **Test Implication:** Validation cryptographique complète requise
- **Action:** Créer test suite validant:
  - Structure PDF conforme ISO 32000-2
  - Signature dictionary présent avec /SubFilter /ETSI.CAdES.detached
  - LTV (Long Term Validation) info embedded
  - Timestamp RFC 3161 présent
- **Priority:** P0 (compliance légale)

**ASR-2: CNDP Compliance Loi 53-05 (ACTIONABLE)**
- **Requirement:** Chiffrement PII, data minimization, student rights
- **Test Implication:** Tests de sécurité et compliance requis
- **Action:** Créer test suite validant:
  - PII chiffré en DB (CIN, CNE, email, phone)
  - Student rights API (accès, rectification, suppression)
  - Audit trail complet pour toutes opérations PII
  - Rétention 30 ans docs + 10 ans logs
- **Priority:** P0 (compliance légale)

**ASR-3: 99% Uptime & 99.5% Signature Success (ACTIONABLE)**
- **Requirement:** NFR-R1, NFR-R2
- **Test Implication:** Tests de fiabilité et resilience requis
- **Action:** Créer test suite validant:
  - Retry logic avec exponential backoff
  - Dead-letter queue pour échecs persistants
  - Graceful degradation si Barid e-Sign API down
  - Circuit breaker pattern pour services externes
- **Priority:** P0 (SLA contractuel)

**ASR-4: Performance < 30s Signature (ACTIONABLE)**
- **Requirement:** NFR-P3
- **Test Implication:** Performance tests requis
- **Action:** Créer test suite validant:
  - Signature single document < 30s (p95)
  - Batch 500 docs < 15min
  - Load testing avec 10 desktop apps concurrents
- **Priority:** P1 (expérience utilisateur)

**ASR-5: OAuth 2.0 Security (FYI)**
- **Requirement:** FR27-FR33
- **Test Implication:** Tests de sécurité OAuth requis
- **Action:** Validation déjà couverte par OpenIddict tests intégrés
- **Priority:** P2 (déjà partiellement testé)

### Risk Assessment Matrix

| Risk ID | Category | Description | Probability | Impact | Score | Mitigation | Owner | Timeline |
|---------|----------|-------------|-------------|--------|-------|------------|-------|----------|
| **R-1** | **TECH** | **USB Dongle failure pendant batch signing** | 3 (High) | 3 (High) | **9** | Retry logic + pause/resume batch + user notification | Dev | Sprint 1 |
| **R-2** | **SEC** | **Certificat Barid expiré non détecté** | 2 (Medium) | 3 (High) | **6** | Alerting proactif 30j avant expiry + validation pre-flight | Dev | Sprint 1 |
| **R-3** | **PERF** | **Batch 500 docs dépasse 15min SLA** | 2 (Medium) | 2 (Medium) | **4** | Load testing + optimization parallel processing | QA | Sprint 2 |
| **R-4** | **DATA** | **PII leak via logs non chiffrés** | 1 (Low) | 3 (High) | **3** | Audit logs + masking PII dans Serilog | Dev | Sprint 1 |
| **R-5** | **BUS** | **Documents signés rejetés par employeurs** | 1 (Low) | 3 (High) | **3** | Validation PAdES compliance + tests avec vrais employeurs | QA | Sprint 2 |
| **R-6** | **OPS** | **MinIO S3 storage full bloque génération** | 2 (Medium) | 2 (Medium) | **4** | Monitoring storage + alerting 80% capacity | Ops | Sprint 1 |
| **R-7** | **TECH** | **Barid e-Sign API down bloque tout** | 2 (Medium) | 3 (High) | **6** | Circuit breaker + graceful degradation + retry queue | Dev | Sprint 1 |
| **R-8** | **SEC** | **JWT tokens volés permettent accès non autorisé** | 1 (Low) | 3 (High) | **3** | JWT rotation 90j + MFA admin + audit trail | Dev | Sprint 1 |
| **R-9** | **PERF** | **PostgreSQL query lent sur audit trail (30 ans data)** | 2 (Medium) | 2 (Medium) | **4** | Indexing + partitioning + archiving old data | Dev | Sprint 2 |
| **R-10** | **DATA** | **Student rights API permet suppression non autorisée** | 1 (Low) | 3 (High) | **3** | RBAC strict + audit trail + soft delete only | Dev | Sprint 1 |

### High-Risk Items (Score ≥ 6)

**🚨 R-1: USB Dongle Failure (Score 9 - CRITICAL)**
- **Mitigation Priority:** P0
- **Test Coverage Required:**
  - Unit tests: Retry logic avec mock dongle failures
  - Integration tests: Pause/resume batch après dongle reconnect
  - E2E tests: User notification workflow
- **Acceptance Criteria:** 
  - Batch signing reprend automatiquement après dongle reconnect
  - Aucune perte de documents en cas de failure
  - User voit message clair "Reconnectez le dongle USB"

**🚨 R-2: Certificat Expiré (Score 6 - HIGH)**
- **Mitigation Priority:** P0
- **Test Coverage Required:**
  - Unit tests: Validation certificat expiry date
  - Integration tests: Alerting 30j avant expiry
  - E2E tests: Blocage signature si certificat expiré
- **Acceptance Criteria:**
  - Admin reçoit email 30j avant expiry
  - Desktop app refuse signature si certificat expiré
  - Monitoring dashboard affiche statut certificat

**🚨 R-7: Barid e-Sign API Down (Score 6 - HIGH)**
- **Mitigation Priority:** P0
- **Test Coverage Required:**
  - Unit tests: Circuit breaker logic
  - Integration tests: Retry avec exponential backoff
  - Chaos tests: Simuler API down pendant batch
- **Acceptance Criteria:**
  - Circuit breaker ouvre après 3 failures consécutifs
  - Documents en échec vont dans dead-letter queue
  - User voit message "Service temporairement indisponible"

### Summary

**Testability Score:** 7/10 (Good with actionable improvements)

**Strengths:**
- Clean Architecture facilite testing
- Observability built-in (logs, metrics)
- Test projects déjà configurés

**Weaknesses:**
- Hardware dependency (USB dongle)
- External API dependencies (Barid e-Sign)
- Async background jobs complexes

**Top 3 Priorities:**
1. Mock USB dongle pour CI/CD (TC-1)
2. Mock Barid e-Sign API (TC-2)
3. Implement contract testing SIS (TC-3)

**High Risks:** 3 items (R-1, R-2, R-7) nécessitent mitigation immédiate

### Prochaine Étape

Définir le plan de couverture de test (Step 4)
