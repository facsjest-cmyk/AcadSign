---
stepsCompleted: ['step-01-detect-mode', 'step-02-load-context', 'step-03-risk-and-testability']
lastStep: 'step-03-risk-and-testability'
lastSaved: '2026-03-05'
workflowType: 'testarch-test-design'
mode: 'system-level'
project: 'AcadSign'
inputDocuments:
  - '/Users/macbookpro/e-sign/_bmad-output/planning-artifacts/prd.md'
  - '/Users/macbookpro/e-sign/_bmad-output/planning-artifacts/architecture.md'
  - '/Users/macbookpro/e-sign/_bmad-output/planning-artifacts/epics.md'
  - '/Users/macbookpro/e-sign/_bmad-output/planning-artifacts/ux-design-specification.md'
---

# Test Design for Architecture: AcadSign - Electronic Signature Platform

**Purpose:** Architectural concerns, testability gaps, and NFR requirements for review by Architecture/Dev teams. Serves as a contract between QA and Engineering on what must be addressed before test development begins.

**Date:** 2026-03-05
**Author:** Test Architect (TEA)
**Status:** Architecture Review Pending
**Project:** AcadSign
**PRD Reference:** `/Users/macbookpro/e-sign/_bmad-output/planning-artifacts/prd.md`
**ADR Reference:** `/Users/macbookpro/e-sign/_bmad-output/planning-artifacts/architecture.md`

---

## Executive Summary

**Scope:** Complete AcadSign platform - .NET 10 Backend API + WPF Desktop Client for qualified electronic signature of academic documents in Moroccan universities.

**Business Context** (from PRD):

- **Revenue/Impact:** 60% reduction in operational costs for document issuance, 100,000+ documents/year at scale
- **Problem:** Students must physically visit campus for official documents, creating administrative bottlenecks and long wait times
- **GA Launch:** Pilot Phase (3 months) → Scale Phase (12 months)

**Architecture** (from ADR):

- **Key Decision 1:** Hybrid Architecture (Backend API + Desktop Client) due to USB dongle constraint for qualified e-signature
- **Key Decision 2:** Clean Architecture (.NET 10) with Domain/Application/Infrastructure/Web layers
- **Key Decision 3:** Stack: .NET 10, PostgreSQL 15+, MinIO S3, WPF MVVM, OpenIddict OAuth 2.0, Hangfire, Serilog+Seq, Prometheus+Grafana

**Expected Scale** (from ADR):

- 5,000+ documents/month (pilot), scaling to 100,000+ documents/year
- 50 concurrent API clients, 10 concurrent desktop apps
- 99% uptime, 99.5% signature success rate
- API < 500ms (p95), Signature < 30s, Batch 500 docs < 15min

**Risk Summary:**

- **Total risks**: 10
- **High-priority (≥6)**: 3 risks requiring immediate mitigation (R-1: USB Dongle failure, R-2: Certificat expiré, R-7: Barid API down)
- **Test effort**: ~150-200 tests (~4-6 weeks for 2 QAs)

---

## Quick Guide

### 🚨 BLOCKERS - Team Must Decide (Can't Proceed Without)

**Pre-Implementation Critical Path** - These MUST be completed before QA can write integration tests:

1. **TC-1: USB Dongle Abstraction** - Create `ISignatureProvider` interface with mock implementation for CI/CD testing (recommended owner: Dev Team, Sprint 1)
2. **TC-2: Barid e-Sign Mock Server** - Implement mock OCSP/CRL/RFC3161 server for automated tests (recommended owner: QA Team, Sprint 1)
3. **TC-3: SIS Laravel Contract Testing** - Implement Pact.js consumer tests for Backend ↔ SIS integration (recommended owner: Dev Team, Sprint 1)

**What we need from team:** Complete these 3 items pre-implementation or test development is blocked.

---

### ⚠️ HIGH PRIORITY - Team Should Validate (We Provide Recommendation, You Approve)

1. **R-1: USB Dongle Failure** - Implement retry logic + pause/resume batch + user notification (Dev Team approval, Sprint 1)
2. **R-2: Certificat Expiré** - Implement alerting 30j before expiry + pre-flight validation (Dev Team approval, Sprint 1)
3. **R-7: Barid API Down** - Implement circuit breaker + graceful degradation + dead-letter queue (Dev Team approval, Sprint 1)

**What we need from team:** Review recommendations and approve (or suggest changes).

---

### 📋 INFO ONLY - Solutions Provided (Review, No Decisions Needed)

1. **Test strategy**: 60% Unit, 25% Integration, 10% Functional, 5% E2E (Risk-based pyramid)
2. **Tooling**: xUnit, NUnit, Testcontainers, Respawn, Moq, FluentAssertions, BenchmarkDotNet
3. **Tiered CI/CD**: Tier 1 (Unit, 2min), Tier 2 (Integration, 10min), Tier 3 (E2E, 30min)
4. **Coverage**: ~180 test scenarios prioritized P0-P3 with risk-based classification
5. **Quality gates**: 80% code coverage, 0 P0 failures, <5% P1 failures

**What we need from team:** Just review and acknowledge (we already have the solution).

---

## For Architects and Devs - Open Topics 👷

### Risk Assessment

**Total risks identified**: 10 (3 high-priority score ≥6, 4 medium, 3 low)

#### High-Priority Risks (Score ≥6) - IMMEDIATE ATTENTION

| Risk ID    | Category  | Description                                    | Probability | Impact     | Score | Mitigation                                                      | Owner | Timeline |
| ---------- | --------- | ---------------------------------------------- | ----------- | ---------- | ----- | --------------------------------------------------------------- | ----- | -------- |
| **R-1**    | **TECH**  | **USB Dongle failure pendant batch signing**   | 3 (High)    | 3 (High)   | **9** | Retry logic + pause/resume batch + user notification            | Dev   | Sprint 1 |
| **R-2**    | **SEC**   | **Certificat Barid expiré non détecté**        | 2 (Medium)  | 3 (High)   | **6** | Alerting proactif 30j avant expiry + validation pre-flight      | Dev   | Sprint 1 |
| **R-7**    | **TECH**  | **Barid e-Sign API down bloque tout**          | 2 (Medium)  | 3 (High)   | **6** | Circuit breaker + graceful degradation + retry queue            | Dev   | Sprint 1 |

#### Medium-Priority Risks (Score 3-5)

| Risk ID | Category | Description                                           | Probability | Impact     | Score | Mitigation                                              | Owner |
| ------- | -------- | ----------------------------------------------------- | ----------- | ---------- | ----- | ------------------------------------------------------- | ----- |
| R-3     | PERF     | Batch 500 docs dépasse 15min SLA                      | 2 (Medium)  | 2 (Medium) | 4     | Load testing + optimization parallel processing         | QA    |
| R-6     | OPS      | MinIO S3 storage full bloque génération               | 2 (Medium)  | 2 (Medium) | 4     | Monitoring storage + alerting 80% capacity              | Ops   |
| R-9     | PERF     | PostgreSQL query lent sur audit trail (30 ans data)   | 2 (Medium)  | 2 (Medium) | 4     | Indexing + partitioning + archiving old data            | Dev   |

#### Low-Priority Risks (Score 1-2)

| Risk ID | Category | Description                                          | Probability | Impact    | Score | Action  |
| ------- | -------- | ---------------------------------------------------- | ----------- | --------- | ----- | ------- |
| R-4     | DATA     | PII leak via logs non chiffrés                       | 1 (Low)     | 3 (High)  | 3     | Monitor |
| R-5     | BUS      | Documents signés rejetés par employeurs              | 1 (Low)     | 3 (High)  | 3     | Monitor |
| R-8     | SEC      | JWT tokens volés permettent accès non autorisé       | 1 (Low)     | 3 (High)  | 3     | Monitor |
| R-10    | DATA     | Student rights API permet suppression non autorisée  | 1 (Low)     | 3 (High)  | 3     | Monitor |

#### Risk Category Legend

- **TECH**: Technical/Architecture (flaws, integration, scalability)
- **SEC**: Security (access controls, auth, data exposure)
- **PERF**: Performance (SLA violations, degradation, resource limits)
- **DATA**: Data Integrity (loss, corruption, inconsistency)
- **BUS**: Business Impact (UX harm, logic errors, revenue)
- **OPS**: Operations (deployment, config, monitoring)

---

### Testability Concerns and Architectural Gaps

**🚨 ACTIONABLE CONCERNS - Architecture Team Must Address**

#### 1. Blockers to Fast Feedback (WHAT WE NEED FROM ARCHITECTURE)

| Concern                                    | Impact                                      | What Architecture Must Provide                                                   | Owner    | Timeline |
| ------------------------------------------ | ------------------------------------------- | -------------------------------------------------------------------------------- | -------- | -------- |
| **No USB Dongle abstraction**              | Cannot run signature tests in CI/CD         | Provide `ISignatureProvider` interface with mock implementation                  | Dev Team | Sprint 1 |
| **No Barid e-Sign mock/sandbox**           | Tests depend on external API (flaky/costly) | Provide mock OCSP/CRL/RFC3161 server or use test certificates                   | QA Team  | Sprint 1 |
| **No SIS Laravel contract**                | Integration tests fragile                   | Implement Pact.js consumer tests with SIS provider verification                 | Dev Team | Sprint 1 |
| **No Hangfire synchronous mode for tests** | Async job tests flaky with timing issues   | Configure Hangfire in-memory mode with synchronous execution for tests          | Dev Team | Sprint 1 |
| **No S3 cleanup automation**               | Tests leave orphaned objects                | Implement Testcontainers for MinIO + cleanup hooks in `[TearDown]`              | Dev Team | Sprint 2 |
| **No SMTP mock for email tests**           | Email tests send real emails (spam)         | Integrate MailHog or smtp4dev for email capture without real sending            | QA Team  | Sprint 2 |

#### 2. Architectural Improvements Needed (WHAT SHOULD BE CHANGED)

1. **Signature Provider Abstraction**
   - **Current problem**: Desktop app directly calls PKCS#11/Windows CSP, impossible to mock
   - **Required change**: Create `ISignatureProvider` interface, inject via DI, provide `MockSignatureProvider` for tests
   - **Impact if not fixed**: Cannot test signature logic in CI/CD, manual testing only
   - **Owner**: Dev Team
   - **Timeline**: Sprint 1 (before signature tests)

2. **External API Circuit Breaker**
   - **Current problem**: No resilience pattern for Barid e-Sign API calls
   - **Required change**: Implement Polly circuit breaker with retry + fallback
   - **Impact if not fixed**: API downtime cascades to all signing operations
   - **Owner**: Dev Team
   - **Timeline**: Sprint 1 (critical for reliability)

3. **Contract Testing Infrastructure**
   - **Current problem**: No contract validation between Backend and SIS Laravel
   - **Required change**: Implement Pact.js consumer tests, publish contracts to Pact Broker
   - **Impact if not fixed**: Integration breaks silently when SIS changes API
   - **Owner**: Dev Team
   - **Timeline**: Sprint 1 (before SIS integration tests)

4. **Test Data Seeding API**
   - **Current problem**: Tests manually seed data, slow and error-prone
   - **Required change**: Provide `POST /api/test/seed` endpoint (test environment only) for deterministic data
   - **Impact if not fixed**: Tests slow, flaky, hard to parallelize
   - **Owner**: Dev Team
   - **Timeline**: Sprint 2 (nice-to-have)

---

### Testability Assessment Summary

**📊 CURRENT STATE - FYI**

#### What Works Well

- ✅ **Clean Architecture** - Domain/Application/Infrastructure separation facilitates mocking and unit testing
- ✅ **Dependency Injection** - Native .NET DI allows easy substitution of dependencies in tests
- ✅ **Existing Test Projects** - `Application.UnitTests`, `Domain.UnitTests`, `Infrastructure.IntegrationTests`, `Application.FunctionalTests` already configured
- ✅ **Observability Built-in** - Serilog + Seq structured logging, Prometheus + Grafana metrics, Correlation IDs for traceability
- ✅ **API-First Design** - OpenAPI 3.0 spec enables contract testing, JSON Schema validation
- ✅ **Immutable Audit Trail** - Facilitates verification of compliance (CNDP Loi 53-05)
- ✅ **Testcontainers Support** - PostgreSQL and MinIO can run in isolated containers for integration tests

#### Accepted Trade-offs (No Action Required)

For AcadSign Phase 1 (Pilot), the following trade-offs are acceptable:

- **No Visual Regression Testing for Bilingual PDFs** - Layout validation (RTL Arabic + LTR French) will be manual. Unit tests verify text content only. Visual regression (Percy.io/Applitools) can be added post-GA if needed.
- **External API Tests Marked as `[Category("External")]`** - Real Barid e-Sign API tests run separately from CI/CD due to cost/reliability. Mock tests cover 95% of scenarios.
- **USB Dongle Tests Require Local Hardware** - Integration tests with real dongle run on developer workstations only. CI/CD uses mock implementation.

This is acceptable technical debt for Phase 1 that should be revisited post-GA based on defect rates.

---

### Risk Mitigation Plans (High-Priority Risks ≥6)

**Purpose**: Detailed mitigation strategies for all 3 high-priority risks (score ≥6). These risks MUST be addressed before Pilot GA launch.

#### R-1: USB Dongle Failure During Batch Signing (Score: 9) - CRITICAL

**Mitigation Strategy:**

1. **Implement Retry Logic**
   - Detect dongle disconnect via PKCS#11 error codes
   - Pause batch signing immediately (do not fail entire batch)
   - Retry signature operation with exponential backoff (1s, 5s, 15s)
   
2. **Pause/Resume Batch Capability**
   - Save batch state to DB (documents signed, documents pending)
   - Allow user to resume batch after dongle reconnect
   - No data loss - all documents tracked in audit trail

3. **User Notification Workflow**
   - Display clear message: "Dongle USB déconnecté. Reconnectez le dongle et entrez le PIN pour continuer."
   - Desktop app shows progress: "50/100 documents signés. En pause..."
   - After reconnect, auto-resume or prompt user to continue

**Owner:** Dev Team
**Timeline:** Sprint 1 (before batch signing implementation)
**Status:** Planned
**Verification:** 
- Unit tests: Mock dongle disconnect, verify retry logic
- Integration tests: Physically disconnect dongle during batch, verify pause/resume
- E2E tests: User sees notification and can resume successfully

---

#### R-2: Certificat Barid Expiré Non Détecté (Score: 6) - HIGH

**Mitigation Strategy:**

1. **Pre-Flight Certificate Validation**
   - Desktop app validates certificate expiry before allowing signature
   - Check: `NotAfter` date > current date + 7 days buffer
   - Refuse signature if certificate expires within 7 days

2. **Proactive Alerting (30 days before expiry)**
   - Backend cron job checks certificate expiry daily
   - Send email to admin 30 days, 15 days, 7 days, 1 day before expiry
   - Monitoring dashboard displays certificate status (green/yellow/red)

3. **Graceful Degradation**
   - If certificate expired, desktop app shows: "Certificat expiré. Contactez l'admin IT pour renouvellement."
   - Block all signature operations until certificate renewed
   - Audit trail logs all certificate validation failures

**Owner:** Dev Team
**Timeline:** Sprint 1 (before signature implementation)
**Status:** Planned
**Verification:**
- Unit tests: Mock expired certificate, verify validation logic
- Integration tests: Set system clock forward, verify alerting triggers
- E2E tests: Admin receives email 30 days before expiry

---

#### R-7: Barid e-Sign API Down Bloque Tout (Score: 6) - HIGH

**Mitigation Strategy:**

1. **Circuit Breaker Pattern (Polly)**
   - Implement circuit breaker for OCSP/CRL/RFC3161 calls
   - Open circuit after 3 consecutive failures
   - Half-open after 30 seconds, retry with exponential backoff

2. **Graceful Degradation**
   - If OCSP/CRL unavailable, use cached validation results (max 24h old)
   - If RFC 3161 timestamping unavailable, use local timestamp (log warning)
   - Desktop app shows: "Service de signature temporairement indisponible. Réessayez dans quelques minutes."

3. **Dead-Letter Queue for Failed Signatures**
   - Documents that fail signature go to Hangfire dead-letter queue
   - Automatic retry every 15 minutes for 24 hours
   - After 24h, manual intervention required (admin notification)

**Owner:** Dev Team
**Timeline:** Sprint 1 (before signature implementation)
**Status:** Planned
**Verification:**
- Unit tests: Mock API down, verify circuit breaker opens
- Integration tests: Simulate API timeout, verify retry logic
- Chaos tests: Kill Barid API during batch, verify graceful degradation

---

### Assumptions and Dependencies

#### Assumptions

1. **Barid Al-Maghrib e-Sign API** has 99% uptime SLA (per vendor contract)
2. **SIS Laravel** provides stable JSON API with versioning (no breaking changes without notice)
3. **USB Dongle** hardware is reliable (MTBF > 5 years per vendor spec)
4. **PostgreSQL** can handle 30 years of audit trail data with proper indexing/partitioning
5. **MinIO S3** storage capacity is monitored and scaled proactively by Ops team
6. **Test environments** (dev, staging, prod) are isolated with separate databases and S3 buckets

#### Dependencies

1. **Barid Al-Maghrib Test Certificates** - Required by Sprint 1 for integration tests
2. **SIS Laravel Sandbox Environment** - Required by Sprint 1 for contract testing
3. **USB Dongle Test Hardware** - Required by Sprint 1 for desktop app testing (2 dongles minimum)
4. **MinIO S3 Test Bucket** - Required by Sprint 1 for storage integration tests
5. **SMTP Test Server (MailHog/smtp4dev)** - Required by Sprint 2 for email tests
6. **Testcontainers License** - Required by Sprint 1 for PostgreSQL/MinIO isolation

#### Risks to Plan

- **Risk**: Barid Al-Maghrib delays test certificate issuance
  - **Impact**: Cannot test signature validation, blocks Sprint 1
  - **Contingency**: Use self-signed certificates for initial tests, switch to real certificates when available

- **Risk**: SIS Laravel team unavailable for contract testing collaboration
  - **Impact**: Cannot implement Pact.js provider verification
  - **Contingency**: Use JSON fixtures from SIS documentation, implement contract tests unilaterally

- **Risk**: USB Dongle hardware unavailable for QA team
  - **Impact**: Cannot test desktop app signature flow
  - **Contingency**: Dev team tests with their dongles, QA focuses on backend API tests

---

**End of Architecture Document**

**Next Steps for Architecture Team:**

1. Review Quick Guide (🚨/⚠️/📋) and prioritize blockers (TC-1, TC-2, TC-3)
2. Assign owners and timelines for high-priority risks (R-1, R-2, R-7)
3. Validate assumptions and dependencies (test certificates, SIS sandbox, dongles)
4. Provide feedback to QA on testability gaps (signature abstraction, circuit breaker, contract testing)

**Next Steps for QA Team:**

1. Wait for pre-implementation blockers to be resolved (ISignatureProvider, mock servers, Pact.js)
2. Refer to companion QA doc (test-design-qa.md) for detailed test scenarios
3. Begin test infrastructure setup (Testcontainers, fixtures, mock servers)
