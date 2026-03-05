---
stepsCompleted: ['step-01-detect-mode', 'step-02-load-context', 'step-03-risk-and-testability', 'step-04-coverage-plan']
lastStep: 'step-04-coverage-plan'
lastSaved: '2026-03-05'
workflowType: 'testarch-test-design'
mode: 'system-level'
project: 'AcadSign'
inputDocuments:
  - '/Users/macbookpro/e-sign/_bmad-output/planning-artifacts/prd.md'
  - '/Users/macbookpro/e-sign/_bmad-output/planning-artifacts/architecture.md'
  - '/Users/macbookpro/e-sign/_bmad-output/planning-artifacts/epics.md'
  - '/Users/macbookpro/e-sign/_bmad-output/planning-artifacts/ux-design-specification.md'
  - '/Users/macbookpro/e-sign/_bmad-output/implementation-artifacts/*.md (43 stories)'
---

# Test Design for QA: AcadSign - Electronic Signature Platform

**Purpose:** Test execution recipe for QA team. Defines what to test, how to test it, and what QA needs from other teams.

**Date:** 2026-03-05
**Author:** Test Architect (TEA)
**Status:** Ready for QA Implementation
**Project:** AcadSign

**Related:** See Architecture doc (test-design-architecture.md) for testability concerns and architectural blockers.

---

## Executive Summary

**Scope:** Complete test coverage for AcadSign platform - Backend API (.NET 10) + Desktop Client (WPF) + Integrations (Barid e-Sign, SIS Laravel, MinIO S3, SMTP)

**Risk Summary:**

- Total Risks: 10 (3 high-priority score ≥6, 4 medium, 3 low)
- Critical Categories: TECH (3 risks), SEC (2 risks), PERF (2 risks)

**Coverage Summary:**

- P0 tests: ~60 (critical paths, security, compliance)
- P1 tests: ~70 (important features, integration)
- P2 tests: ~40 (edge cases, regression)
- P3 tests: ~10 (exploratory, benchmarks)
- **Total**: ~180 tests (~4-6 weeks with 2 QAs)

---

## Not in Scope

**Components or systems explicitly excluded from this test plan:**

| Item                                    | Reasoning                                                | Mitigation                                                                 |
| --------------------------------------- | -------------------------------------------------------- | -------------------------------------------------------------------------- |
| **SIS Laravel Internal Logic**          | Upstream system owned by SIS team                        | Contract testing validates API contract, SIS team tests their own logic   |
| **Barid Al-Maghrib e-Sign PKI**         | External PKI infrastructure, certified by national authority | Integration tests validate our usage, Barid certifies their own PKI      |
| **Windows OS / WPF Framework**          | Third-party platform, tested by Microsoft                | Focus on our WPF app logic, not framework itself                          |
| **PostgreSQL / MinIO / Hangfire Core**  | Third-party libraries with their own test suites         | Integration tests validate our configuration and usage                    |
| **Visual Layout of Bilingual PDFs**     | Complex RTL/LTR layout validation                        | Manual testing for Phase 1, visual regression tests post-GA (optional)    |

**Note:** Items listed here have been reviewed and accepted as out-of-scope by QA, Dev, and PM.

---

## Dependencies & Test Blockers

**CRITICAL:** QA cannot proceed without these items from other teams.

### Backend/Architecture Dependencies (Pre-Implementation)

**Source:** See Architecture doc "Quick Guide" for detailed mitigation plans

1. **ISignatureProvider Abstraction** - Dev Team - Sprint 1
   - QA needs: Mock implementation of signature provider for CI/CD tests
   - Why it blocks testing: Cannot test signature logic without USB dongle abstraction

2. **Barid e-Sign Mock Server** - QA Team - Sprint 1
   - QA needs: Mock OCSP/CRL/RFC3161 server for automated tests
   - Why it blocks testing: Cannot run signature validation tests without external API mock

3. **SIS Laravel Contract (Pact.js)** - Dev Team - Sprint 1
   - QA needs: Pact.js consumer tests with published contracts
   - Why it blocks testing: Integration tests fragile without contract validation

4. **Hangfire Synchronous Mode** - Dev Team - Sprint 1
   - QA needs: In-memory Hangfire configuration for synchronous job execution in tests
   - Why it blocks testing: Async job tests flaky with timing issues

### QA Infrastructure Setup (Pre-Implementation)

1. **Test Data Factories** - QA Team - Sprint 1
   - Student factory with faker-based randomization
   - Document request factory with all 4 types (Attestation, Relevé, Réussite, Inscription)
   - Certificate factory with expiry date control
   - Auto-cleanup fixtures for parallel safety

2. **Test Environments** - QA Team - Sprint 1
   - Local: Testcontainers (PostgreSQL, MinIO, MailHog)
   - CI/CD: GitHub Actions with .NET 10 SDK + Docker
   - Staging: Full environment with real Barid test certificates

**Example factory pattern (.NET):**

```csharp
public class StudentFactory
{
    private readonly Faker<Student> _faker;

    public StudentFactory()
    {
        _faker = new Faker<Student>()
            .RuleFor(s => s.Id, f => $"E-2024-{f.Random.Number(1000, 9999)}")
            .RuleFor(s => s.FullName, f => f.Name.FullName())
            .RuleFor(s => s.Cin, f => f.Random.AlphaNumeric(8).ToUpper())
            .RuleFor(s => s.Email, f => f.Internet.Email())
            .RuleFor(s => s.Program, f => f.PickRandom("Génie Informatique", "Mathématiques", "Physique"))
            .RuleFor(s => s.Level, f => f.PickRandom("1ère année", "2ème année", "3ème année"));
    }

    public Student Generate() => _faker.Generate();
    public List<Student> Generate(int count) => _faker.Generate(count);
}
```

---

## Risk Assessment

**Note:** Full risk details in Architecture doc. This section summarizes risks relevant to QA test planning.

### High-Priority Risks (Score ≥6)

| Risk ID    | Category  | Description                                  | Score | QA Test Coverage                                                                 |
| ---------- | --------- | -------------------------------------------- | ----- | -------------------------------------------------------------------------------- |
| **R-1**    | **TECH**  | **USB Dongle failure pendant batch signing** | **9** | P0-015 to P0-020: Retry logic, pause/resume, user notification                  |
| **R-2**    | **SEC**   | **Certificat Barid expiré non détecté**      | **6** | P0-025 to P0-028: Certificate validation, alerting, expiry blocking             |
| **R-7**    | **TECH**  | **Barid e-Sign API down bloque tout**        | **6** | P0-030 to P0-035: Circuit breaker, retry, dead-letter queue, graceful degradation |

### Medium/Low-Priority Risks

| Risk ID | Category | Description                                         | Score | QA Test Coverage                                                  |
| ------- | -------- | --------------------------------------------------- | ----- | ----------------------------------------------------------------- |
| R-3     | PERF     | Batch 500 docs dépasse 15min SLA                    | 4     | P1-050 to P1-055: Load testing, parallel processing optimization  |
| R-4     | DATA     | PII leak via logs non chiffrés                      | 3     | P1-060 to P1-062: Log masking, audit trail encryption            |
| R-5     | BUS      | Documents signés rejetés par employeurs             | 3     | P0-040 to P0-042: PAdES compliance validation                    |
| R-6     | OPS      | MinIO S3 storage full bloque génération             | 4     | P2-010 to P2-012: Storage monitoring, alerting                   |
| R-8     | SEC      | JWT tokens volés permettent accès non autorisé      | 3     | P1-070 to P1-075: JWT rotation, MFA, RBAC                        |
| R-9     | PERF     | PostgreSQL query lent sur audit trail (30 ans data) | 4     | P2-020 to P2-022: Query performance, indexing                    |
| R-10    | DATA     | Student rights API permet suppression non autorisée | 3     | P1-080 to P1-082: RBAC, audit trail, soft delete                 |

---

## Entry Criteria

**QA testing cannot begin until ALL of the following are met:**

- [ ] All requirements and acceptance criteria agreed upon by QA, Dev, PM
- [ ] Test environments provisioned and accessible (local, CI/CD, staging)
- [ ] Test data factories ready (Student, DocumentRequest, Certificate)
- [ ] Pre-implementation blockers resolved (ISignatureProvider, mock servers, Pact.js, Hangfire sync mode)
- [ ] Backend API deployed to staging environment
- [ ] Desktop app deployed to test workstations with USB dongles
- [ ] Barid Al-Maghrib test certificates issued and configured
- [ ] SIS Laravel sandbox environment accessible
- [ ] MinIO S3 test bucket created with SSE-KMS encryption
- [ ] SMTP test server (MailHog/smtp4dev) running

## Exit Criteria

**Testing phase is complete when ALL of the following are met:**

- [ ] All P0 tests passing (60 tests, 100% pass rate)
- [ ] All P1 tests passing or failures triaged and accepted (70 tests, ≥95% pass rate)
- [ ] No open P0/P1 bugs
- [ ] Test coverage ≥80% for Backend API (unit + integration)
- [ ] Performance baselines met (API < 500ms p95, Signature < 30s, Batch 500 docs < 15min)
- [ ] Security tests passing (OAuth, RBAC, PII encryption, audit trail)
- [ ] Compliance tests passing (PAdES validation, CNDP Loi 53-05, student rights)
- [ ] QA Lead and Dev Lead sign-off on test results

---

## Test Coverage Plan

**IMPORTANT:** P0/P1/P2/P3 = **priority and risk level** (what to focus on if time-constrained), NOT execution timing.

### P0 (Critical) - 60 Tests

**Criteria:** Blocks core functionality + High risk (≥6) + No workaround + Affects majority of users

#### Epic 1: Infrastructure (5 tests)

| Test ID    | Requirement                                  | Test Level  | Risk Link | Notes                                    |
| ---------- | -------------------------------------------- | ----------- | --------- | ---------------------------------------- |
| **P0-001** | Backend API starts successfully              | Integration | -         | Health check endpoint returns 200       |
| **P0-002** | PostgreSQL connection established            | Integration | -         | Database migrations applied successfully |
| **P0-003** | MinIO S3 connection established              | Integration | -         | Bucket creation and SSE-KMS encryption   |
| **P0-004** | Desktop app starts successfully              | E2E         | -         | WPF window loads without errors          |
| **P0-005** | Dev Containers build successfully            | Integration | -         | .NET SDK and dependencies installed      |

#### Epic 2: Authentication & Security (10 tests)

| Test ID    | Requirement                                  | Test Level  | Risk Link | Notes                                           |
| ---------- | -------------------------------------------- | ----------- | --------- | ----------------------------------------------- |
| **P0-006** | OAuth 2.0 Authorization Code + PKCE flow     | Integration | R-8       | Desktop app obtains access token                |
| **P0-007** | OAuth 2.0 Client Credentials flow            | Integration | R-8       | SIS Laravel obtains access token                |
| **P0-008** | JWT token validation                         | Unit        | R-8       | Invalid/expired tokens rejected                 |
| **P0-009** | RBAC: Admin can access all endpoints         | Integration | R-8       | Role-based access control                       |
| **P0-010** | RBAC: Registrar cannot access admin endpoints| Integration | R-8       | 403 Forbidden for unauthorized roles            |
| **P0-011** | PII encryption in database                   | Integration | R-4       | CIN, CNE, email, phone encrypted with AES-256   |
| **P0-012** | JWT rotation after 90 days                   | Unit        | R-8       | Old tokens invalidated                          |
| **P0-013** | MFA required for admin login                 | E2E         | R-8       | Admin cannot login without MFA                  |
| **P0-014** | Audit trail logs all PII access              | Integration | R-4       | Every PII read/write logged with correlation ID |

#### Epic 3: PDF Generation & Storage (8 tests)

| Test ID    | Requirement                                  | Test Level  | Risk Link | Notes                                           |
| ---------- | -------------------------------------------- | ----------- | --------- | ----------------------------------------------- |
| **P0-015** | Generate bilingual PDF (French + Arabic)     | Integration | -         | QuestPDF renders RTL Arabic correctly           |
| **P0-016** | Generate 4 document types                    | Integration | -         | Attestation, Relevé, Réussite, Inscription      |
| **P0-017** | Embed QR code in PDF                         | Integration | -         | QR code scannable and contains verification URL |
| **P0-018** | Upload PDF to MinIO S3 with SSE-KMS          | Integration | -         | Encryption at rest verified                     |
| **P0-019** | Generate pre-signed URL (7 days expiry)      | Integration | -         | URL accessible for 7 days, then 403             |
| **P0-020** | Template management: upload new template     | Integration | -         | Versioning and multi-institution branding       |
| **P0-021** | Batch generate 500 PDFs < 15min              | Performance | R-3       | Parallel processing optimization                |
| **P0-022** | S3 storage full blocks generation gracefully | Integration | R-6       | Error message clear, no data loss               |

#### Epic 4: Electronic Signature (15 tests)

| Test ID    | Requirement                                  | Test Level  | Risk Link | Notes                                           |
| ---------- | -------------------------------------------- | ----------- | --------- | ----------------------------------------------- |
| **P0-023** | Detect USB dongle (PKCS#11/Windows CSP)      | Integration | R-1       | Desktop app detects dongle presence             |
| **P0-024** | Sign PDF with PAdES-B-LT                     | Integration | R-5       | Signature conforms to ISO 32000-2               |
| **P0-025** | Validate certificate expiry before signing   | Unit        | R-2       | Refuse signature if cert expires < 7 days       |
| **P0-026** | Alert admin 30 days before cert expiry       | Integration | R-2       | Email sent to admin                             |
| **P0-027** | Block signature if certificate expired       | E2E         | R-2       | Desktop app shows error message                 |
| **P0-028** | OCSP/CRL validation during signature         | Integration | R-7       | Certificate revocation checked                  |
| **P0-029** | RFC 3161 timestamping embedded in signature  | Integration | R-5       | Timestamp present in signed PDF                 |
| **P0-030** | Retry signature on dongle disconnect         | Integration | R-1       | Exponential backoff (1s, 5s, 15s)               |
| **P0-031** | Pause batch signing on dongle disconnect     | E2E         | R-1       | Batch paused, not failed                        |
| **P0-032** | Resume batch signing after dongle reconnect  | E2E         | R-1       | No data loss, continues from last signed doc    |
| **P0-033** | User notification on dongle disconnect       | E2E         | R-1       | Clear message: "Reconnectez le dongle USB"      |
| **P0-034** | Circuit breaker opens after 3 API failures   | Unit        | R-7       | Barid e-Sign API failures trigger circuit       |
| **P0-035** | Graceful degradation on Barid API down       | Integration | R-7       | Use cached OCSP/CRL (max 24h old)              |
| **P0-036** | Dead-letter queue for failed signatures      | Integration | R-7       | Retry every 15min for 24h                       |
| **P0-037** | Batch sign 500 docs < 15min                  | Performance | R-3       | Parallel processing with progress tracking      |

#### Epic 5: Background Jobs (5 tests)

| Test ID    | Requirement                                  | Test Level  | Risk Link | Notes                                           |
| ---------- | -------------------------------------------- | ----------- | --------- | ----------------------------------------------- |
| **P0-038** | Hangfire job enqueued successfully           | Integration | -         | Background job created in DB                    |
| **P0-039** | Batch document generation job completes      | Integration | -         | 500 docs generated in background                |
| **P0-040** | Retry logic with exponential backoff         | Unit        | R-7       | 1min, 5min, 15min, 1h intervals                 |
| **P0-041** | Dead-letter queue captures persistent failures| Integration | R-7       | Failed jobs moved to DLQ after max retries      |
| **P0-042** | Job status polling endpoint returns progress | Integration | -         | Real-time progress tracking                     |

#### Epic 6: Public Verification (5 tests)

| Test ID    | Requirement                                  | Test Level  | Risk Link | Notes                                           |
| ---------- | -------------------------------------------- | ----------- | --------- | ----------------------------------------------- |
| **P0-043** | Public verification page loads               | E2E         | -         | Web page accessible without auth                |
| **P0-044** | Scan QR code and verify signature            | E2E         | R-5       | Signature valid, certificate valid              |
| **P0-045** | Verify signature cryptographically           | Integration | R-5       | PAdES signature validation                      |
| **P0-046** | Display document metadata (student, type)    | E2E         | -         | Clear UI/UX for verification result             |
| **P0-047** | Verification < 2s (p95)                      | Performance | -         | Public endpoint performance SLA                 |

#### Epic 7: External API (4 tests)

| Test ID    | Requirement                                  | Test Level  | Risk Link | Notes                                           |
| ---------- | -------------------------------------------- | ----------- | --------- | ----------------------------------------------- |
| **P0-048** | OpenAPI 3.0 spec generated                   | Integration | -         | Swagger UI accessible                           |
| **P0-049** | JSON Schema validation on requests           | Integration | -         | Invalid requests rejected with 400              |
| **P0-050** | Webhook notification sent on signature       | Integration | -         | SIS Laravel receives webhook                    |
| **P0-051** | Rate limiting enforced (100 req/min)         | Integration | -         | 429 Too Many Requests after limit               |

#### Epic 8: CNDP Compliance (5 tests)

| Test ID    | Requirement                                  | Test Level  | Risk Link | Notes                                           |
| ---------- | -------------------------------------------- | ----------- | --------- | ----------------------------------------------- |
| **P0-052** | Audit trail logs all document operations     | Integration | R-4       | Immutable logs for 10 years                     |
| **P0-053** | Student rights API: access to own data       | Integration | R-10      | Student can view their documents                |
| **P0-054** | Student rights API: rectification request    | Integration | R-10      | Student can request data correction             |
| **P0-055** | Student rights API: deletion request         | Integration | R-10      | Soft delete only, audit trail preserved         |
| **P0-056** | CNDP compliance report generated             | Integration | -         | Report includes all required fields (Loi 53-05) |

#### Epic 9: Notifications (2 tests)

| Test ID    | Requirement                                  | Test Level  | Risk Link | Notes                                           |
| ---------- | -------------------------------------------- | ----------- | --------- | ----------------------------------------------- |
| **P0-057** | Email sent to student after signature        | Integration | -         | Email contains download link                    |
| **P0-058** | Retry email sending on SMTP failure          | Integration | -         | Exponential backoff, max 3 retries              |

#### Epic 10: Monitoring (1 test)

| Test ID    | Requirement                                  | Test Level  | Risk Link | Notes                                           |
| ---------- | -------------------------------------------- | ----------- | --------- | ----------------------------------------------- |
| **P0-059** | Prometheus metrics exposed                   | Integration | -         | /metrics endpoint returns valid data            |
| **P0-060** | Alert triggered on certificate expiry        | Integration | R-2       | Alert sent 30 days before expiry                |

**Total P0:** 60 tests

---

### P1 (High) - 70 Tests

**Criteria:** Important features + Medium risk (3-4) + Common workflows + Workaround exists but difficult

#### Backend API Integration (25 tests)

| Test ID    | Requirement                                  | Test Level  | Risk Link | Notes                                           |
| ---------- | -------------------------------------------- | ----------- | --------- | ----------------------------------------------- |
| **P1-001** | SIS Laravel contract testing (Pact.js)       | Contract    | -         | Consumer tests for Backend ↔ SIS                |
| **P1-002** | Fetch student data from SIS API              | Integration | -         | OAuth Client Credentials flow                   |
| **P1-003** | Batch import students from CSV               | Integration | -         | 1000 students imported successfully             |
| **P1-004** | Batch import students from JSON              | Integration | -         | Validation errors handled gracefully            |
| **P1-005** | Batch import students from XML               | Integration | -         | XML parsing and validation                      |
| **P1-006** | Desktop app ↔ Backend API (Refit)            | Integration | -         | All CRUD operations via Refit client            |
| **P1-007** | API response time < 500ms (p95)              | Performance | -         | Load testing with 50 concurrent clients         |
| **P1-008** | API handles 50 concurrent clients            | Performance | -         | No degradation under load                       |
| **P1-009** | Database transaction rollback on error       | Integration | -         | No partial data commits                         |
| **P1-010** | PostgreSQL connection pool management        | Integration | -         | No connection leaks                             |
| **P1-011** | Entity Framework migrations applied          | Integration | -         | Schema updates successful                       |
| **P1-012** | Soft delete preserves audit trail            | Integration | R-10      | Deleted records still in DB, marked as deleted  |
| **P1-013** | Correlation ID propagated across services    | Integration | -         | Logs traceable end-to-end                       |
| **P1-014** | Structured logging with Serilog              | Integration | -         | JSON logs sent to Seq                           |
| **P1-015** | Health check endpoint returns detailed status| Integration | -         | DB, S3, Barid API status                        |
| **P1-016** | Graceful shutdown on SIGTERM                 | Integration | -         | In-flight requests completed                    |
| **P1-017** | Configuration loaded from appsettings.json   | Integration | -         | Environment-specific config                     |
| **P1-018** | Secrets loaded from environment variables    | Integration | -         | No hardcoded secrets                            |
| **P1-019** | CORS configured for Desktop app origin       | Integration | -         | Preflight requests handled                      |
| **P1-020** | API versioning (v1, v2)                      | Integration | -         | Backward compatibility maintained               |
| **P1-021** | Pagination for large result sets             | Integration | -         | 1000+ documents paginated correctly             |
| **P1-022** | Filtering by document type                   | Integration | -         | Query string parameters work                    |
| **P1-023** | Sorting by date, status, student name        | Integration | -         | Multiple sort fields supported                  |
| **P1-024** | Search by student name, CIN, CNE             | Integration | -         | Full-text search with PostgreSQL                |
| **P1-025** | Bulk operations (delete, update status)      | Integration | -         | Transaction safety for bulk ops                 |

#### Desktop App WPF (20 tests)

| Test ID    | Requirement                                  | Test Level  | Risk Link | Notes                                           |
| ---------- | -------------------------------------------- | ----------- | --------- | ----------------------------------------------- |
| **P1-026** | MVVM pattern implemented correctly           | Unit        | -         | ViewModels testable without UI                  |
| **P1-027** | Data binding updates UI reactively           | E2E         | -         | ObservableCollection changes reflected          |
| **P1-028** | Commands enabled/disabled based on state     | Unit        | -         | RelayCommand CanExecute logic                   |
| **P1-029** | Progress bar updates during batch signing    | E2E         | -         | Real-time progress tracking                     |
| **P1-030** | Logs displayed in real-time (FlowDocument)   | E2E         | -         | Colored logs (green=success, red=error)         |
| **P1-031** | PDF viewer displays before/after signature   | E2E         | -         | PdfiumViewer integration                        |
| **P1-032** | Zoom in/out PDF viewer                       | E2E         | -         | Zoom levels: 50%, 100%, 150%, 200%              |
| **P1-033** | Navigate PDF pages (prev/next)               | E2E         | -         | Page navigation buttons work                    |
| **P1-034** | Select all pending documents                 | E2E         | -         | Checkbox "Select All Pending"                   |
| **P1-035** | Filter documents by status                   | E2E         | -         | Tous, En attente, Signés, Erreur                |
| **P1-036** | Search documents by student name             | E2E         | -         | Real-time search filtering                      |
| **P1-037** | Dark theme applied consistently              | E2E         | -         | All UI elements use dark palette                |
| **P1-038** | Status indicators show service health        | E2E         | -         | e-Sign API, S3, SIS status (green/red)          |
| **P1-039** | Error messages displayed clearly             | E2E         | -         | No technical jargon, actionable messages        |
| **P1-040** | Desktop app auto-update mechanism            | E2E         | -         | New version downloaded and installed            |
| **P1-041** | Window state persisted (size, position)      | E2E         | -         | User preferences saved                          |
| **P1-042** | Keyboard shortcuts work (Ctrl+A, Ctrl+S)     | E2E         | -         | Power user efficiency                           |
| **P1-043** | Drag and drop files (future feature)         | E2E         | -         | Upload templates via drag-drop                  |
| **P1-044** | Multi-language support (French/Arabic)       | E2E         | -         | UI strings localized                            |
| **P1-045** | Accessibility: keyboard navigation           | E2E         | -         | Tab order logical, focus visible                |

#### Security & Compliance (15 tests)

| Test ID    | Requirement                                  | Test Level  | Risk Link | Notes                                           |
| ---------- | -------------------------------------------- | ----------- | --------- | ----------------------------------------------- |
| **P1-046** | TLS 1.3 enforced (min TLS 1.2)               | Integration | -         | SSL Labs A+ rating                              |
| **P1-047** | HTTPS redirect from HTTP                     | Integration | -         | All traffic encrypted                           |
| **P1-048** | SQL injection protection (parameterized queries)| Integration | -         | No raw SQL concatenation                        |
| **P1-049** | XSS protection (input sanitization)          | Integration | -         | User input escaped                              |
| **P1-050** | CSRF protection (anti-forgery tokens)        | Integration | -         | State-changing requests protected               |
| **P1-051** | Password hashing (bcrypt, Argon2)            | Unit        | -         | No plaintext passwords                          |
| **P1-052** | Rate limiting per IP address                 | Integration | -         | 1000 req/min per IP                             |
| **P1-053** | Audit trail immutable (append-only)          | Integration | R-4       | No UPDATE or DELETE on audit logs               |
| **P1-054** | Audit trail retention 10 years               | Integration | -         | Old logs archived, not deleted                  |
| **P1-055** | Document retention 30 years                  | Integration | -         | S3 lifecycle policy configured                  |
| **P1-056** | PII masking in logs                          | Integration | R-4       | CIN, CNE, email masked in Serilog               |
| **P1-057** | Data minimization (only required fields)     | Integration | -         | No unnecessary PII collected                    |
| **P1-058** | Student consent recorded                     | Integration | -         | Consent timestamp in DB                         |
| **P1-059** | GDPR-style data export (JSON)                | Integration | R-10      | Student can export all their data               |
| **P1-060** | Backup and restore (RTO < 4h, RPO < 1h)      | Integration | -         | Daily backups, tested restore                   |

#### Performance & Scalability (10 tests)

| Test ID    | Requirement                                  | Test Level  | Risk Link | Notes                                           |
| ---------- | -------------------------------------------- | ----------- | --------- | ----------------------------------------------- |
| **P1-061** | Load test: 50 concurrent API clients         | Performance | -         | No degradation, response time stable            |
| **P1-062** | Load test: 10 concurrent desktop apps        | Performance | -         | Batch signing parallelized                      |
| **P1-063** | Stress test: 100 req/s for 10 minutes        | Performance | -         | System stable under sustained load              |
| **P1-064** | Spike test: 500 req/s burst                  | Performance | -         | Graceful degradation, no crashes                |
| **P1-065** | Endurance test: 24h continuous operation     | Performance | -         | No memory leaks, CPU stable                     |
| **P1-066** | Database query performance (audit trail)     | Performance | R-9       | Queries < 1s even with 10M records              |
| **P1-067** | S3 upload performance (10MB PDF)             | Performance | -         | Upload < 5s                                     |
| **P1-068** | PDF generation performance (single doc)      | Performance | -         | < 3s per document                               |
| **P1-069** | Signature performance (single doc)           | Performance | -         | < 30s per signature (p95)                       |
| **P1-070** | Batch 500 docs performance                   | Performance | R-3       | < 15min total time                              |

**Total P1:** 70 tests

---

### P2 (Medium) - 40 Tests

**Criteria:** Secondary features + Low risk (1-2) + Edge cases + Regression prevention

#### Edge Cases & Error Handling (20 tests)

| Test ID    | Requirement                                  | Test Level  | Risk Link | Notes                                           |
| ---------- | -------------------------------------------- | ----------- | --------- | ----------------------------------------------- |
| **P2-001** | Handle malformed JSON in API request         | Integration | -         | 400 Bad Request with clear error message        |
| **P2-002** | Handle missing required fields               | Integration | -         | Validation errors returned                      |
| **P2-003** | Handle duplicate student ID                  | Integration | -         | 409 Conflict                                    |
| **P2-004** | Handle invalid certificate format            | Integration | -         | Clear error message                             |
| **P2-005** | Handle PDF generation failure                | Integration | -         | Retry logic, error logged                       |
| **P2-006** | Handle S3 upload failure                     | Integration | -         | Retry with exponential backoff                  |
| **P2-007** | Handle SMTP server down                      | Integration | -         | Email queued for retry                          |
| **P2-008** | Handle SIS API timeout                       | Integration | -         | Circuit breaker opens                           |
| **P2-009** | Handle database connection loss              | Integration | -         | Reconnect automatically                         |
| **P2-010** | Handle disk full on desktop app              | E2E         | -         | Clear error message to user                     |
| **P2-011** | Handle network disconnect during batch       | E2E         | -         | Pause batch, resume when network restored       |
| **P2-012** | Handle invalid QR code scan                  | E2E         | -         | Error message: "QR code invalide"               |
| **P2-013** | Handle expired pre-signed URL                | Integration | -         | 403 Forbidden                                   |
| **P2-014** | Handle concurrent batch signing              | Integration | -         | Lock mechanism prevents conflicts               |
| **P2-015** | Handle very long student names (>100 chars)  | Integration | -         | Truncation or validation                        |
| **P2-016** | Handle special characters in names           | Integration | -         | Arabic diacritics, French accents               |
| **P2-017** | Handle empty result sets                     | Integration | -         | No errors, empty array returned                 |
| **P2-018** | Handle pagination edge cases (page 0, -1)    | Integration | -         | Validation errors                               |
| **P2-019** | Handle timezone differences                  | Integration | -         | UTC timestamps, localized display               |
| **P2-020** | Handle daylight saving time transitions      | Integration | -         | No timestamp bugs                               |

#### Regression & Compatibility (10 tests)

| Test ID    | Requirement                                  | Test Level  | Risk Link | Notes                                           |
| ---------- | -------------------------------------------- | ----------- | --------- | ----------------------------------------------- |
| **P2-021** | Backward compatibility with v1 API           | Integration | -         | Old clients still work                          |
| **P2-022** | Database migration rollback                  | Integration | -         | Can revert to previous schema                   |
| **P2-023** | Desktop app works on Windows 10              | E2E         | -         | Minimum OS version supported                    |
| **P2-024** | Desktop app works on Windows 11              | E2E         | -         | Latest OS version supported                     |
| **P2-025** | Desktop app works with .NET 10 runtime       | E2E         | -         | Runtime dependency verified                     |
| **P2-026** | PDF readable in Adobe Acrobat Reader         | E2E         | -         | Cross-platform compatibility                    |
| **P2-027** | PDF readable in Chrome PDF viewer            | E2E         | -         | Browser compatibility                           |
| **P2-028** | Signature valid in Adobe Acrobat             | E2E         | R-5       | PAdES signature recognized                      |
| **P2-029** | QR code scannable with mobile apps           | E2E         | -         | iOS/Android QR scanners work                    |
| **P2-030** | Email readable in Gmail, Outlook             | E2E         | -         | HTML email rendering                            |

#### Monitoring & Observability (10 tests)

| Test ID    | Requirement                                  | Test Level  | Risk Link | Notes                                           |
| ---------- | -------------------------------------------- | ----------- | --------- | ----------------------------------------------- |
| **P2-031** | Grafana dashboard displays metrics           | Integration | -         | CPU, memory, request rate, error rate           |
| **P2-032** | Prometheus scrapes metrics every 15s         | Integration | -         | Metrics up-to-date                              |
| **P2-033** | Alertmanager sends alerts to Slack           | Integration | -         | Alert routing configured                        |
| **P2-034** | Seq receives structured logs                 | Integration | -         | Logs searchable and filterable                  |
| **P2-035** | Correlation IDs in all logs                  | Integration | -         | End-to-end request tracing                      |
| **P2-036** | Error logs include stack traces              | Integration | -         | Debugging information available                 |
| **P2-037** | Performance metrics tracked (p50, p95, p99)  | Integration | -         | Latency percentiles calculated                  |
| **P2-038** | Storage metrics tracked (S3 usage)           | Integration | R-6       | Alert at 80% capacity                           |
| **P2-039** | Certificate expiry metric tracked            | Integration | R-2       | Days until expiry exposed                       |
| **P2-040** | Dead-letter queue size metric tracked        | Integration | R-7       | Alert if DLQ > 100 items                        |

**Total P2:** 40 tests

---

### P3 (Low) - 10 Tests

**Criteria:** Nice-to-have + Exploratory + Performance benchmarks + Documentation validation

| Test ID    | Requirement                                  | Test Level  | Notes                                           |
| ---------- | -------------------------------------------- | ----------- | ----------------------------------------------- |
| **P3-001** | Benchmark: PDF generation throughput         | Performance | Measure docs/second on different hardware       |
| **P3-002** | Benchmark: Signature throughput              | Performance | Measure signatures/second with real dongle      |
| **P3-003** | Benchmark: Database query performance        | Performance | Measure query time vs data volume               |
| **P3-004** | Exploratory: UI/UX usability testing         | Manual      | User feedback on Desktop app                    |
| **P3-005** | Exploratory: Bilingual PDF layout validation| Manual      | Visual inspection of RTL/LTR rendering          |
| **P3-006** | Documentation: OpenAPI spec accuracy         | Manual      | Swagger UI matches actual API behavior          |
| **P3-007** | Documentation: README completeness           | Manual      | Setup instructions work for new developers      |
| **P3-008** | Documentation: Architecture diagrams         | Manual      | Diagrams match actual implementation            |
| **P3-009** | Chaos: Kill PostgreSQL during batch          | Chaos       | System recovers gracefully                      |
| **P3-010** | Chaos: Kill MinIO during upload              | Chaos       | Retry logic works, no data loss                 |

**Total P3:** 10 tests

---

## Execution Strategy

**Philosophy:** Run everything in PRs unless there's significant infrastructure overhead.

**Organized by TOOL TYPE:**

### Every PR: .NET Tests (~10-15 min)

**All functional tests** (from any priority level):

- Unit tests (xUnit, NUnit): ~80 tests
- Integration tests (Testcontainers): ~60 tests
- Functional tests (WebApplicationFactory): ~30 tests
- Parallelized across 4 cores
- Total: ~170 .NET tests (includes P0, P1, P2, P3)

**Why run in PRs:** Fast feedback, no expensive infrastructure

### Nightly: Performance Tests (~30-60 min)

**All performance tests** (from any priority level):

- Load tests (BenchmarkDotNet, k6): ~10 tests
- Stress tests: ~5 tests
- Endurance tests: ~2 tests
- Total: ~17 performance tests (may include P0, P1, P2)

**Why defer to nightly:** Long-running (10-40 min per test)

### Weekly: Chaos & Long-Running (~hours)

**Special infrastructure tests** (from any priority level):

- Chaos tests (kill services): ~2 tests
- Disaster recovery (backup restore): ~1 test
- 24h endurance test: ~1 test

**Why defer to weekly:** Very long-running, infrequent validation sufficient

**Manual tests** (excluded from automation):

- Visual layout validation (bilingual PDFs): ~5 tests
- Usability testing: ~1 test
- Documentation validation: ~3 tests

---

## QA Effort Estimate

**QA test development effort only** (excludes DevOps, Backend work):

| Priority  | Count | Effort Range  | Notes                                             |
| --------- | ----- | ------------- | ------------------------------------------------- |
| P0        | ~60   | ~3-4 weeks    | Complex setup (security, performance, multi-step) |
| P1        | ~70   | ~2-3 weeks    | Standard coverage (integration, API tests)        |
| P2        | ~40   | ~1 week       | Edge cases, simple validation                     |
| P3        | ~10   | ~2-3 days     | Exploratory, benchmarks                           |
| **Total** | ~180  | **~4-6 weeks**| **2 QA engineers, full-time**                     |

**Assumptions:**

- Includes test design, implementation, debugging, CI integration
- Excludes ongoing maintenance (~10% effort)
- Assumes test infrastructure (factories, fixtures, Testcontainers) ready
- Assumes pre-implementation blockers resolved (ISignatureProvider, mock servers, Pact.js)

**Dependencies from other teams:**

- See "Dependencies & Test Blockers" section for what QA needs from Backend, DevOps

---

## Implementation Planning Handoff

**Use this to inform implementation planning; if no dedicated QA, assign to Dev owners.**

| Work Item                                    | Owner    | Target Milestone | Dependencies/Notes                                    |
| -------------------------------------------- | -------- | ---------------- | ----------------------------------------------------- |
| Create ISignatureProvider abstraction        | Dev Team | Sprint 1         | Blocks signature tests                                |
| Implement Barid e-Sign mock server           | QA Team  | Sprint 1         | Blocks signature validation tests                     |
| Implement Pact.js consumer tests             | Dev Team | Sprint 1         | Blocks SIS integration tests                          |
| Configure Hangfire in-memory mode for tests  | Dev Team | Sprint 1         | Blocks background job tests                           |
| Setup Testcontainers (PostgreSQL, MinIO)     | QA Team  | Sprint 1         | Blocks integration tests                              |
| Create test data factories (Student, Doc)    | QA Team  | Sprint 1         | Blocks all tests                                      |
| Setup MailHog for email testing              | QA Team  | Sprint 2         | Blocks email tests                                    |
| Configure GitHub Actions CI/CD               | DevOps   | Sprint 1         | Blocks automated test execution                       |
| Obtain Barid test certificates               | Ops      | Sprint 1         | Blocks signature integration tests                    |
| Setup SIS Laravel sandbox                    | SIS Team | Sprint 1         | Blocks SIS integration tests                          |

---

## Tooling & Access

| Tool or Service          | Purpose                              | Access Required                  | Status  |
| ------------------------ | ------------------------------------ | -------------------------------- | ------- |
| **xUnit / NUnit**        | Unit testing framework               | NuGet package                    | Ready   |
| **Testcontainers**       | Integration test isolation           | Docker Desktop                   | Ready   |
| **Moq / NSubstitute**    | Mocking framework                    | NuGet package                    | Ready   |
| **FluentAssertions**     | Assertion library                    | NuGet package                    | Ready   |
| **BenchmarkDotNet**      | Performance benchmarking             | NuGet package                    | Ready   |
| **Respawn**              | Database cleanup                     | NuGet package                    | Ready   |
| **Bogus (Faker)**        | Test data generation                 | NuGet package                    | Ready   |
| **MailHog / smtp4dev**   | Email testing                        | Docker container                 | Pending |
| **Pact.js**              | Contract testing                     | npm package                      | Pending |
| **GitHub Actions**       | CI/CD pipeline                       | GitHub repo access               | Ready   |
| **Barid Test Certs**     | Signature testing                    | Barid Al-Maghrib test environment| Pending |
| **SIS Laravel Sandbox**  | Integration testing                  | SIS team provisioning            | Pending |
| **USB Dongle (x2)**      | Desktop app testing                  | Physical hardware                | Pending |

---

**End of QA Document**

**Next Steps for QA Team:**

1. Wait for pre-implementation blockers to be resolved (ISignatureProvider, mock servers, Pact.js, Hangfire sync mode)
2. Setup test infrastructure (Testcontainers, factories, fixtures, MailHog)
3. Begin P0 test implementation (60 tests, ~3-4 weeks)
4. Parallelize with P1 tests once P0 infrastructure stable
5. Coordinate with Dev team on testability gaps (see Architecture doc)

**Next Steps for Dev Team:**

1. Review Architecture doc (test-design-architecture.md) for blockers and high-priority risks
2. Implement ISignatureProvider abstraction (TC-1, Sprint 1)
3. Implement circuit breaker for Barid e-Sign API (R-7, Sprint 1)
4. Implement Pact.js consumer tests (TC-3, Sprint 1)
5. Configure Hangfire in-memory mode for tests (TC-7, Sprint 1)
