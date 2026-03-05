---
stepsCompleted: ['step-01-init', 'step-02-discovery', 'step-02b-vision', 'step-02c-executive-summary', 'step-03-success', 'step-04-journeys', 'step-05-domain', 'step-06-innovation', 'step-07-project-type', 'step-08-scoping', 'step-09-functional', 'step-10-nonfunctional', 'step-11-polish', 'step-12-complete']
workflowStatus: 'complete'
completedDate: '2026-03-03'
inputDocuments: []
workflowType: 'prd'
projectName: 'AcadSign'
briefCount: 0
researchCount: 0
brainstormingCount: 0
projectDocsCount: 0
userProvidedContext: 'Detailed AcadSign requirements provided inline by user'
classification:
  projectType: 'api_backend'
  techStack: '.NET 10, PostgreSQL 15+, MinIO'
  domain: 'legaltech_edtech'
  complexity: 'high'
  projectContext: 'greenfield'
  keySignals: 'REST API, PKI integration, qualified e-signature, Moroccan legal compliance, bilingual documents, immutable audit trail'
---

# Product Requirements Document - AcadSign

**Author:** Macbookpro
**Date:** 2026-03-03

## Executive Summary

**AcadSign** is a .NET 10 REST API platform designed to digitally transform academic document issuance in Moroccan higher education institutions. The system generates bilingual (Arabic/French) official academic documents (enrollment certificates, transcripts, diplomas) and applies legally-binding qualified electronic signatures via integration with **Barid Al-Maghrib e-Sign**, Morocco's national PKI infrastructure operated by Barid eBank.

**Target users:** University registrar staff (document issuers), IT administrators (system operators), and students (document recipients).

**Problem solved:** Moroccan universities currently require students to physically visit campus to obtain official academic documents, creating administrative bottlenecks, long wait times, and high operational costs for registrar services. AcadSign eliminates physical document collection entirely while ensuring full legal validity through Morocco's national electronic signature framework.

**Business impact:**
- **For students:** Zero campus visits required — receive legally-valid official documents digitally within minutes
- **For registrar staff:** Drastic reduction in administrative workload — automated document generation and signing replaces manual processing
- **For institutions:** Accelerated issuance timelines (days/weeks → minutes), reduced operational costs, fraud prevention via cryptographic verification

### What Makes This Special

**Three core differentiators:**

1. **Native Moroccan legal compliance** — Purpose-built to comply with Loi n° 53-05 (electronic data protection) and Loi n° 43-20 (digital trust services and e-signature), ensuring documents have full legal standing in Morocco without adaptation layers

2. **National PKI integration** — Direct integration with Barid Al-Maghrib e-Sign service for qualified/advanced electronic signatures (PAdES format), leveraging Morocco's trusted national infrastructure rather than foreign signature providers

3. **Academic domain specialization** — Pre-configured bilingual templates (Arabic RTL + French LTR), academic document type taxonomy (attestation de scolarité, relevé de notes, attestation de réussite), and university-specific workflows optimized for Moroccan higher education administration

**Core insight:** By combining Morocco's national PKI infrastructure with academic-specific document workflows, AcadSign delivers legally-compliant digital transformation without requiring universities to navigate complex international e-signature regulations or build custom integrations.

**Verification & trust:** Each signed document embeds a QR code linking to a public verification portal, enabling instant authenticity validation by employers, government agencies, or other third parties — creating a tamper-proof, auditable document ecosystem with 30-year retention compliance.

## Project Classification

- **Project Type:** API Backend (REST API)
- **Technology Stack:** .NET 10 (C#), S3-compatible object storage, HSM/soft certificate management
- **Domain:** LegalTech / EdTech hybrid
- **Complexity Level:** High
  - Regulatory compliance (Moroccan electronic signature and data protection laws)
  - Cryptographic operations (qualified signatures, PAdES/CAdES, certificate chain validation, RFC 3161 timestamps)
  - Bilingual document generation (Arabic RTL + French LTR layout)
  - Long-term legal retention (30-year immutable audit trail)
- **Project Context:** Greenfield (new system)

## Success Criteria

### User Success

**For Students (Document Recipients):**
- **Primary success moment:** Receive download link via email within 2 minutes of document request submission
- **Verification confidence:** Scan embedded QR code and see instant authenticity validation on public portal
- **Zero friction:** Obtain official academic documents without physical campus visit — 100% digital workflow
- **Employer acceptance:** Third-party verifiers (employers, government agencies) accept digitally-signed documents without questioning validity

**Measurable outcome:** 95% of students report they would recommend the digital document service to peers; average time from request to document receipt < 3 minutes.

**For Registrar Staff (Document Issuers):**
- **Workload transformation:** Zero physical queues at registrar office for document pickup
- **Efficiency gain:** Process 100+ document requests in under 1 hour (vs. 2+ days with manual workflow)
- **Error reduction:** Automated document generation eliminates manual data entry errors and template inconsistencies
- **Administrative relief:** Staff reallocate time from repetitive document processing to higher-value student services

**Measurable outcome:** 80% reduction in time spent on document issuance; registrar staff satisfaction score ≥ 4/5 for system usability.

### Business Success

**3-Month Success (Pilot Phase):**
- **Adoption:** 3-5 pilot universities actively using AcadSign in production
- **Volume:** 5,000+ documents successfully generated and signed with qualified e-signatures
- **Reliability:** 99%+ signing success rate (excluding Barid e-Sign API downtime)
- **Validation:** Zero legal challenges to document validity; acceptance by at least 10 external verifiers (employers, government agencies)

**12-Month Success (Scale Phase):**
- **Market penetration:** 20+ Moroccan universities adopted AcadSign
- **Cost impact:** 60% reduction in operational costs for document issuance per institution
- **Volume scale:** 100,000+ documents signed; system handles peak loads (e.g., end-of-semester transcript requests)
- **Compliance validation:** Full audit by Moroccan regulatory authority confirms compliance with Loi 53-05 and 43-20

**Key business metric:** Average cost per document issued drops from MAD 15 (manual) to MAD 3 (automated).

### Technical Success

**Performance:**
- Single document generation + signing: < 5 seconds (95th percentile)
- Batch processing: 500 documents in < 10 minutes
- API response time: < 500ms for metadata queries (p95)

**Reliability:**
- System uptime: 99.5% during business hours (8am-6pm Morocco time)
- Signing success rate: 99%+ (excluding external API failures)
- Zero data loss: 100% document persistence with 30-year retention guarantee

**Security & Compliance:**
- All documents signed with qualified/advanced electronic signatures via Barid Al-Maghrib PKI
- Immutable audit trail for every document lifecycle event
- Certificate validation: 100% of signatures include valid certificate chain and RFC 3161 timestamp
- Zero security incidents (unauthorized access, data breaches, signature forgery)

**Integration:**
- Barid Al-Maghrib e-Sign API integration: < 3 second response time for signature operations (p95)
- S3-compatible storage: 99.99% object durability
- Dead-letter queue: 100% of failed signing jobs captured for retry

### Measurable Outcomes

**User Experience:**
- Student satisfaction: ≥ 4.5/5 rating for document request experience
- Registrar staff: ≥ 80% report "significantly easier" vs. manual process

**Operational Efficiency:**
- Document issuance time: 95% reduction (days → minutes)
- Administrative workload: 80% reduction in staff hours spent on document processing

**Legal & Compliance:**
- 100% of documents legally valid under Moroccan law
- Zero rejected documents by external verifiers due to signature issues
- Full audit trail compliance for 30-year retention period

**Technical Performance:**
- 99.5% API availability during business hours
- < 5 second end-to-end document generation + signing (p95)
- Zero critical security vulnerabilities in production

## Product Scope

### MVP - Minimum Viable Product

**Core Document Types (Must Have):**
- Attestation de scolarité (Enrollment Certificate)
- Relevé de notes (Transcript)
- Attestation de réussite (Certificate of Achievement)

**Essential Features:**
- ✅ JSON input API for student data ingestion
- ✅ Bilingual PDF generation (Arabic RTL + French LTR) from configurable templates
- ✅ Barid Al-Maghrib e-Sign integration for qualified electronic signatures (PAdES format)
- ✅ S3-compatible storage with pre-signed download URLs
- ✅ QR code embedding for document verification
- ✅ Public verification portal (scan QR → validate signature authenticity)
- ✅ Immutable audit trail (generation, signing, download events)
- ✅ REST API endpoints: generate, sign, generate-and-sign, download, verify
- ✅ OAuth 2.0 / JWT authentication
- ✅ Role-based access control (Admin, Registrar, API Client)
- ✅ Batch processing for bulk document generation

**Technical Requirements:**
- .NET 10 REST API
- HSM or soft certificate management for signing keys
- SHA-256 hashing, PAdES signature format
- Certificate chain validation + RFC 3161 timestamping
- Health check endpoints + structured logging
- Retry logic with exponential backoff for signing failures

**Compliance (MVP Critical):**
- Full compliance with Loi n° 53-05 (data protection) and Loi n° 43-20 (e-signature)
- TLS 1.2+ for all communications
- Audit log: append-only, immutable

**Out of MVP Scope:**
- Admin dashboard (metrics visualization)
- Integration with existing university SIS systems
- Multi-institution branding/customization
- Additional document types beyond the 3 core types

### Growth Features (Post-MVP)

**Admin Dashboard:**
- Real-time metrics: documents generated per day, signing success rate, storage usage
- Certificate expiry alerts
- Batch job status monitoring
- User activity logs

**SIS Integration:**
- Pre-built connectors for common Moroccan university SIS platforms
- Webhook support for automatic document generation triggers
- Bulk student data import from SIS

**Multi-Institution Support:**
- Institution-specific branding (logos, watermarks, color schemes)
- Per-institution template customization
- Multi-tenant architecture with isolated data

**Enhanced Document Types:**
- Attestation d'inscription (Registration Certificate)
- Diplôme (Diploma)
- Custom document types via template builder

**Advanced Features:**
- Email notification service (auto-send download links to students)
- Student self-service portal (request documents directly)
- Batch status webhooks for async processing
- Advanced analytics (document request patterns, peak usage times)

### Vision (Future)

**National Document Registry:**
- Centralized Moroccan academic document verification service
- Cross-university document lookup by employers/government
- Blockchain-based document hash registry for ultimate tamper-proofing

**International Recognition:**
- eIDAS-compatible signatures for EU recognition
- Integration with international credential verification networks (e.g., Europass, Digitary)

**AI-Powered Fraud Detection:**
- Anomaly detection for suspicious document requests
- Pattern recognition for fraudulent data inputs

**Mobile App:**
- Student mobile app for document requests and digital wallet storage
- Push notifications for document readiness

## User Journeys

### Journey 1: Fatima — Responsable du Service de Scolarité

**Situation actuelle (avant AcadSign):**
Fatima, 42 ans, travaille au service de scolarité de l'Université Hassan II à Casablanca depuis 15 ans. Chaque matin, elle arrive au bureau et trouve déjà une file de 30 étudiants qui attendent des attestations de scolarité. Elle doit manuellement vérifier l'identité de chaque étudiant, chercher leurs données dans le SIS, remplir le template Word (parfois avec des erreurs de frappe), imprimer le document, faire signer par le directeur (qui n'est pas toujours disponible), tamponner et remettre à l'étudiant. Elle traite 40-50 documents par jour, travaille souvent en heures supplémentaires, et les étudiants attendent 2-3 jours minimum.

**Parcours avec AcadSign:**

**Matin, 9h00** — Fatima se connecte à l'interface AcadSign avec ses credentials OAuth. Elle voit un dashboard simple : 127 demandes en attente.

**9h05** — Elle sélectionne un batch de 50 demandes d'attestations de scolarité. Le système a déjà récupéré les données étudiants depuis le SIS via l'API. Elle vérifie rapidement les données affichées (nom, CNE, programme).

**9h10** — Elle clique sur "Générer et Signer le Batch". Le système génère 50 PDFs bilingues AR/FR, envoie les documents à Barid Al-Maghrib e-Sign, applique la signature électronique qualifiée, stocke les documents signés sur S3, et envoie automatiquement les liens de téléchargement aux étudiants par email.

**9h18** — Batch terminé. 50 étudiants ont reçu leurs documents. Fatima prend son café, soulagée.

**Moment "aha!":** Quand elle réalise qu'elle vient de traiter en 8 minutes ce qui lui prenait 2 jours auparavant. Plus de file d'attente, plus de stress.

**Nouvelle réalité:** Fatima traite maintenant 500+ documents par jour sans effort. Elle consacre son temps à aider les étudiants avec des questions complexes plutôt qu'à imprimer des papiers.

### Journey 2: Youssef — Étudiant en Master

**Situation actuelle (avant AcadSign):**
Youssef, 24 ans, étudiant en Master Informatique à Rabat, a postulé pour un stage en France. L'entreprise lui demande une attestation de scolarité urgente — deadline dans 3 jours. Il habite à 40 km du campus. Il doit prendre un bus (1h30 aller), faire la queue au service de scolarité (2h d'attente), revenir 2 jours plus tard pour récupérer le document, et re-prendre le bus (1h30 retour). Coût total : 2 déplacements, 6 heures perdues, 100 MAD de transport, stress énorme.

**Parcours avec AcadSign:**

**Lundi, 14h00** — Youssef reçoit l'email de l'entreprise française demandant l'attestation. Il se connecte au portail étudiant de son université.

**14h02** — Il clique sur "Demander une attestation de scolarité". Un formulaire simple s'affiche. Il confirme ses données (déjà pré-remplies).

**14h03** — Il soumet la demande. Un message s'affiche : "Votre demande est en cours de traitement. Vous recevrez votre document par email dans quelques minutes."

**14h06** — Email reçu : "Votre attestation de scolarité est prête !"

**14h07** — Youssef clique sur le lien sécurisé, télécharge le PDF signé électroniquement. Il voit son attestation bilingue (arabe + français), un QR code en bas du document, et la signature électronique qualifiée visible.

**14h10** — Il scanne le QR code avec son téléphone par curiosité. Le portail de vérification s'ouvre et affiche : "✅ Document authentique — Signé le 03/03/2026 par Université Hassan II"

**14h12** — Il envoie le PDF à l'entreprise française par email.

**Moment "aha!":** Quand il réalise qu'il vient d'obtenir un document officiel légal sans bouger de chez lui, en 10 minutes au lieu de 2 jours et 2 déplacements.

**Nouvelle réalité:** Youssef recommande le système à tous ses amis. Il obtient son stage en France.

### Journey 3: Karim — Administrateur IT Universitaire

**Situation:** Karim, 35 ans, ingénieur système, est responsable du déploiement d'AcadSign à l'Université Mohammed V.

**Parcours de déploiement:**

**Semaine 1 — Configuration initiale**
Karim déploie l'API AcadSign sur l'infrastructure universitaire (conteneurs Docker). Il configure les credentials Barid Al-Maghrib e-Sign (certificat client HSM), configure le bucket S3 MinIO pour le stockage des documents, et crée les rôles RBAC : Admin, Registrar, API Client.

**Semaine 2 — Intégration SIS**
Karim configure l'intégration avec le SIS existant de l'université. Il mappe les champs JSON requis par AcadSign (student ID, name, program, etc.) et teste l'API avec des données de test.

**Semaine 3 — Templates bilingues**
Karim upload les templates PDF personnalisés (logo université, en-têtes AR/FR). Il configure les 3 types de documents : attestation scolarité, relevé notes, attestation réussite.

**Semaine 4 — Tests et formation**
Il lance un batch test de 10 documents, forme le personnel de scolarité à l'utilisation de l'interface, et monitore les métriques : signing success rate 99.8%, temps moyen 4.2 secondes.

**Moment "aha!":** Quand il voit le dashboard Prometheus afficher 1000 documents signés en une journée sans aucune erreur système.

**Nouvelle réalité:** Karim reçoit des félicitations du recteur. Le système tourne en production avec 99.7% uptime. Il dort tranquille.

### Journey 4: Sarah — Recruteuse RH dans une entreprise

**Situation:** Sarah, recruteuse chez une multinationale à Casablanca, reçoit un CV d'un candidat avec une attestation de réussite AcadSign.

**Parcours de vérification:**

**10h00** — Sarah ouvre le PDF de l'attestation. Elle voit un QR code en bas.

**10h01** — Elle scanne le QR code avec son smartphone.

**10h02** — Le portail public de vérification AcadSign s'ouvre et affiche : "✅ Document Authentique | Type : Attestation de Réussite | Émis par : Université Hassan II Casablanca | Date de signature : 15/02/2026 | Signature électronique : Valide | Certificat : Barid Al-Maghrib PKI (Valide jusqu'au 2027)"

**10h03** — Sarah est rassurée. Elle sait que le document est authentique, impossible à falsifier.

**Moment "aha!":** Vérification instantanée, zéro doute, zéro appel à l'université pour confirmer.

**Nouvelle réalité:** Sarah valide le candidat en toute confiance. Le processus de recrutement est accéléré.

### Journey 5: Omar — Développeur intégrant AcadSign avec le SIS

**Situation:** Omar, développeur backend dans une ESN, doit intégrer AcadSign avec le SIS d'une université cliente.

**Parcours API:**

**Jour 1 — Découverte de l'API**
Omar lit la documentation OpenAPI d'AcadSign. Il teste les endpoints avec Postman : `POST /api/documents/generate-and-sign` avec un payload JSON test, et `GET /api/documents/{id}/download` pour récupérer le document signé.

**Jour 2 — Intégration**
Il crée un service dans le SIS qui écoute les demandes étudiants, construit le payload JSON (student data + document type), appelle l'API AcadSign avec JWT auth, et récupère le document signé pour envoyer le lien à l'étudiant.

**Jour 3 — Batch processing**
Il implémente un job nocturne qui récupère toutes les demandes de la journée, envoie un batch de 500 documents à AcadSign, et monitore le statut via `GET /api/documents/batch/{batchId}/status`.

**Moment "aha!":** L'API est claire, bien documentée, et fonctionne du premier coup. Le batch de 500 documents est traité en 8 minutes.

**Nouvelle réalité:** Omar livre le projet en avance. Le client est ravi.

### Journey Requirements Summary

**From these journeys, the following capabilities are required:**

1. **Authentication & Authorization**
   - OAuth 2.0 / JWT for API clients
   - RBAC (Admin, Registrar, API Client, Auditor)
   - Secure credential management

2. **Document Generation**
   - Configurable bilingual templates (Arabic RTL + French LTR)
   - PDF generation from JSON payloads
   - Multi-document type support (attestation scolarité, relevé notes, attestation réussite)
   - Institution branding (logos, watermarks)

3. **Electronic Signature**
   - Barid Al-Maghrib e-Sign integration
   - PAdES signature format
   - Certificate chain validation
   - RFC 3161 timestamping
   - HSM/soft certificate management

4. **Storage & Distribution**
   - S3-compatible object storage
   - Pre-signed download URLs with time-limited access
   - Email notification service with secure links
   - 30-year retention compliance

5. **Public Verification**
   - QR code embedding in documents
   - Public verification portal (no authentication required)
   - Signature validation + certificate status check
   - Document metadata display

6. **Batch Processing**
   - Asynchronous bulk document generation
   - Status polling endpoints
   - Dead-letter queue for failed jobs
   - Retry logic with exponential backoff

7. **Monitoring & Operations**
   - Admin dashboard (real-time metrics)
   - Health check endpoints
   - Immutable audit trail (generation, signing, download events)
   - Alerting (certificate expiry, signing failures, storage thresholds)
   - Structured logging with correlation IDs

8. **SIS Integration**
   - Well-documented REST API (OpenAPI specification)
   - Webhook support for event notifications
   - Bulk data import capabilities
   - Clear JSON schema for student data

## Domain-Specific Requirements

### 1. Compliance & Regulatory (CNDP - Loi 53-05)

**Données Académiques Sensibles:**
Les données étudiants (notes, CIN, CNE, origine sociale pour bourses) sont considérées **sensibles** par la CNDP (Commission Nationale de contrôle de la protection des Données à caractère Personnel).

**Obligations légales:**

**Finalité précise:**
- Le traitement doit être limité à : gestion des inscriptions, suivi pédagogique, délivrance de documents académiques officiels
- Toute autre finalité nécessite une justification et potentiellement une nouvelle déclaration CNDP

**Proportionnalité:**
- Collecter uniquement les données strictement nécessaires
- CIN/CNE : justifié pour identification officielle
- Numéro sécurité sociale, données de santé : **NON collectées** (hors scope AcadSign)

**Information des étudiants (Transparence):**
Affichage obligatoire d'une mention d'information sur tous les formulaires précisant :
- Identité du responsable du traitement (l'université)
- Finalité du traitement (délivrance de documents académiques signés)
- Destinataires des données (service de scolarité, Barid Al-Maghrib pour signature, stockage S3)
- Droits des étudiants : accès, rectification, opposition, suppression

**Sécurité technique obligatoire:**
- Chiffrement des données sensibles (CIN, CNE, email, téléphone)
- Gestion des accès par login/mot de passe robuste
- Mesures contre les fuites de données académiques

**Déclaration CNDP Obligatoire:**

| Type de traitement | Régime CNDP | Formulaire |
|-------------------|-------------|------------|
| Gestion administrative standard (fichiers élèves, notes, documents) | **Déclaration Préalable** | **Formulaire F211 (Normal)** |
| Données sensibles (santé, biométrie, origine ethnique) | Autorisation Préalable | Formulaire F112 |
| Transfert de données à l'étranger (Cloud hors Maroc) | Autorisation Préalable | Formulaire F112 |

**Action requise avant déploiement:**
- Remplir et soumettre le **Formulaire F211** à la CNDP
- Attendre l'accusé de réception (délai typique : 2-4 semaines)
- Conserver la preuve de déclaration pour audits futurs

**Conservation légale:**
- Documents académiques : **30 ans** (exigence légale marocaine pour diplômes et relevés de notes)
- Logs d'audit : **10 ans minimum** (recommandation CNDP)
- Données personnelles temporaires (requêtes API) : **suppression après traitement** (minimisation)

### 2. Technical Constraints — Electronic Signature Architecture

**PRODUCTION ARCHITECTURE: Desktop Application + USB Dongle**

**Signature method:**
- **Client-side signing** using physical USB dongle (Barid Al-Maghrib token class 3)
- Certificate never leaves the USB key
- Signature computation happens on the workstation where the dongle is plugged

**Architecture choice for production:**
- **Desktop application (.NET 10 WPF or WinForms)**
- Installed on registrar staff workstations (Fatima's computer)
- Direct access to USB dongle via PKCS#11 or Windows CSP (Cryptographic Service Provider)
- Communication with AcadSign backend API to fetch documents to sign
- Signature performed locally, then signed document uploaded to backend

**Technical flow:**
1. Registrar staff opens desktop application
2. Application authenticates to AcadSign backend (OAuth 2.0 / JWT)
3. Staff selects batch of documents to generate and sign
4. Backend generates unsigned PDFs and returns them to desktop app
5. Desktop app detects USB dongle, prompts for PIN code
6. Desktop app signs each PDF using dongle certificate (PAdES format)
7. Desktop app uploads signed PDFs to backend
8. Backend stores signed documents on S3 and sends download links to students

**USB Dongle Specifications (Barid Al-Maghrib):**

**Certificate type:**
- **Class 3** (highest level) for Qualified Electronic Signature (QES)
- Stored in tamper-proof USB token (impossible to extract private key)

**Certificate validity:**
- **2 years** for physical person certificates (dongle)
- **3 years** for server seal certificates (rare)

**Certificate rotation:**
- Alert **3 months before expiration**
- Renewal process: physical visit to Barid Bank agency with ID documents

**HSM requirement:**
- **Production:** USB Dongle Barid Al-Maghrib (provided by Barid eBank) = portable HSM (certified)
- **MVP/Testing:** Soft certificate (.p12/.pfx) acceptable for integration testing only (limited legal value)

**Certificate pinning:**
- **DO NOT implement** for Barid Al-Maghrib infrastructure
- Reason: Barid may renew intermediate/root certificates without major notice → risk of breaking production
- **Standard rigorous TLS validation** is sufficient

**Signature formats supported:**
- **PAdES** (PDF Advanced Electronic Signature) for PDFs — primary format
- **CAdES** (CMS Advanced Electronic Signature) for other formats if needed
- **XAdES** (XML Advanced Electronic Signature) — less common for this use case

**Dongle access security:**
- PIN code required to unlock dongle (3 attempts max before lock)
- Physical security: dongle stored in secure location when not in use
- Access logs for dongle usage
- Immediate certificate revocation via OCSP if theft detected

### 3. Security & Cryptography

**Encryption in transit:**
- **TLS 1.3** (target standard for 2026)
- TLS 1.2 = strict minimum (legacy support)
- Benefits of TLS 1.3: Perfect Forward Secrecy (PFS) native, better performance

**Encryption at rest (S3 storage):**
- **SSE-KMS** (Server-Side Encryption with KMS) **recommended**
- Advantages:
  - Role separation: bucket access ≠ decrypted document access
  - Audit trail: CloudTrail/equivalent logs for each decryption operation
  - **Major positive point during CNDP audit**

**Application-level PII encryption:**
- **Mandatory** for sensitive fields: CIN, CNE, Email, Phone
- Reason: Protection against database administrator (DBA) access and backup leaks
- **Approach:** Encryption at application level **before** insertion into JSON/database
- Recommended algorithm: AES-256-GCM with key management via KMS

**Document hashing:**
- **SHA-256** for hash computation before signature
- Hash stored in audit trail for integrity verification

**Password policies:**
- Minimum 12 characters for admin/registrar accounts
- Password rotation every 90 days
- Multi-factor authentication (MFA) for admin accounts

### 4. Integration Requirements

**SIS (Student Information System) — Laravel Application**

**Supported exchange formats:**
- **JSON** (primary)
- **XML** (secondary)
- **CSV** (batch import)

**Authentication SIS → AcadSign:**
- **JWT (JSON Web Tokens)** — already used by Laravel SIS
- OAuth 2.0 Client Credentials flow for machine-to-machine
- JWT secret rotation every 90 days

**Integration endpoints:**
```
SIS (Laravel) → AcadSign Backend API
POST /api/documents/generate
Headers: Authorization: Bearer <JWT>
Body: JSON payload (student data + document type)
Response: { documentId, unsignedPdfUrl }

Desktop App → AcadSign Backend API
GET /api/documents/{documentId}/unsigned
Headers: Authorization: Bearer <JWT>
Response: PDF binary (unsigned)

Desktop App → AcadSign Backend API
POST /api/documents/{documentId}/upload-signed
Headers: Authorization: Bearer <JWT>
Body: Signed PDF binary
Response: { documentId, signedPdfUrl, downloadLink }
```

**Webhook support (optional for MVP):**
```
AcadSign → SIS (Laravel)
POST /webhook/document-ready
Body: { documentId, status, downloadUrl }
```

**JSON Schema validation:**
- Strict validation of student data payloads
- Required fields: studentId, firstName, lastName, CIN/CNE, program, academicYear, documentType
- Optional fields: grades (for transcripts), GPA, mention

**Rate limiting:**
- Max 100 requests/minute per JWT client
- Prevents abuse and protects backend resources

### 5. Risk Mitigations

**Risk 1: Signature repudiation**
- **Mitigation:**
  - Immutable audit trail with RFC 3161 timestamping
  - Class 3 qualified certificate (legal non-repudiation)
  - Signature logs retained for 30 years

**Risk 2: Document fraud (falsification of student data)**
- **Mitigation:**
  - Data validation at source (Laravel SIS with JWT authentication)
  - QR code linked to non-predictable UUID v4
  - Public verification portal with cryptographic validation
  - Application-level PII encryption (CIN, CNE) to prevent database modifications

**Risk 3: USB dongle compromise (theft or loss)**
- **Mitigation:**
  - Dongle stored in physical safe (controlled access)
  - Physical access logs to signature workstation
  - PIN code required to unlock dongle (3 attempts max)
  - Immediate certificate revocation via OCSP if theft detected
  - Backup dongle available for business continuity

**Risk 4: Desktop application unavailability (dongle disconnected)**
- **Mitigation:**
  - Dead-letter queue for retry automation
  - Dongle availability monitoring (health check every 5 minutes)
  - Immediate alert if dongle is disconnected
  - Fallback: unsigned document generation with deferred signing

**Risk 5: CNDP non-compliance (illegal data processing)**
- **Mitigation:**
  - F211 declaration before production deployment
  - Data minimization (collect only necessary data)
  - Limited retention periods (30 years for documents, 10 years for logs)
  - Student rights API (access/rectification/deletion)
  - SSE-KMS encryption + application-level PII encryption

**Risk 6: Data leak via Laravel SIS backup**
- **Mitigation:**
  - Database backup encryption
  - Restricted backup access (strict RBAC)
  - Application-level PII encryption → even if backup stolen, data unreadable

**Risk 7: Malicious data injection from SIS**
- **Mitigation:**
  - Strict JSON payload validation (JSON Schema)
  - Input sanitization (name, program) to prevent PDF injection
  - Rate limiting on AcadSign API (max 100 requests/minute per JWT client)
  - SQL injection protection in Laravel SIS (parameterized queries)

**Risk 8: Desktop application compromise (malware)**
- **Mitigation:**
  - Code signing of desktop application executable
  - Antivirus/EDR on registrar workstations
  - Application whitelisting (only signed apps can run)
  - Regular security updates for desktop app

### 6. Moroccan Legal Framework

**Applicable laws:**
- **Loi n° 53-05** (protection des données à caractère personnel) — Moroccan GDPR equivalent
- **Loi n° 43-20** (services de confiance numérique et signature électronique) — Digital trust and e-signature law

**Regulatory authority:**
- **CNDP** (Commission Nationale de contrôle de la protection des Données à caractère Personnel)
- **DGSSI** (Direction Générale de la Sécurité des Systèmes d'Information) — for PKI oversight

**Legal validity of signed documents:**
- Documents signed with Barid Al-Maghrib Class 3 certificate have **full legal standing** in Morocco
- Equivalent to handwritten signature + official stamp
- Accepted by all Moroccan government agencies, courts, and private sector

**International recognition:**
- **eIDAS-compatible** signatures for EU recognition (future enhancement)
- Moroccan qualified signatures recognized in France and other Francophone countries via bilateral agreements

## API Backend & Desktop Application — Technical Architecture

### Architecture Overview

**AcadSign** is built as a **hybrid architecture** combining:

1. **Backend API** (.NET 10 ASP.NET Core Web API) — REST API server
2. **Desktop Application** (.NET 10 WPF with MVVM pattern) — Client-side signing application
3. **SIS Integration** (Laravel application) — Student Information System

**Architecture rationale:**
The hybrid architecture is required due to the **USB dongle constraint** for electronic signature. The Barid Al-Maghrib Class 3 certificate resides in a physical USB token that cannot be accessed remotely. Therefore, signature operations must occur on the workstation where the dongle is plugged (client-side signing).

### Component Architecture

**1. Backend API (.NET 10 REST API)**

**Responsibilities:**
- Generate unsigned PDF documents from JSON payloads
- Manage document templates (bilingual Arabic/French)
- Store signed documents in S3-compatible storage
- Provide pre-signed download URLs
- Manage authentication and authorization (OAuth 2.0 / JWT)
- Maintain immutable audit trail
- Expose public verification portal

**Technology stack:**
- ASP.NET Core 10.0 Web API (.NET 10)
- Entity Framework Core (database ORM)
- PostgreSQL (database management system)
- MinIO SDK (S3-compatible object storage)
- QuestPDF or iTextSharp (PDF generation)
- Serilog (structured logging)
- Hangfire or MassTransit (background jobs)

**Deployment:**
- Containerized (Docker)
- Kubernetes or standalone server
- Hosted on-premise or cloud (Morocco-based datacenter for CNDP compliance)

**2. Desktop Application (.NET 10 WPF MVVM)**

**Responsibilities:**
- Authenticate registrar staff to backend API
- Fetch unsigned PDF documents from backend
- Detect and access USB dongle (Barid Al-Maghrib token)
- Perform local PAdES signature using dongle certificate
- Upload signed PDFs to backend
- Display batch processing status
- Handle dongle errors and retry logic

**Technology stack:**
- WPF (Windows Presentation Foundation) with MVVM pattern
- Prism or CommunityToolkit.Mvvm (MVVM framework)
- PKCS#11 or Windows CSP (Cryptographic Service Provider) for dongle access
- iTextSharp 7 or BouncyCastle (PAdES signature implementation)
- HttpClient (REST API communication)

**Deployment:**
- MSI installer or ClickOnce deployment
- Installed on registrar staff workstations (Windows 10/11)
- Auto-update mechanism for version management

**3. SIS Integration (Laravel)**

**Responsibilities:**
- Provide student data to AcadSign backend
- Trigger document generation requests
- Receive webhook notifications when documents are ready

**Integration method:**
- REST API calls (JSON payloads)
- JWT authentication
- Webhook endpoints for async notifications

### API Endpoint Specification

**Base URL:** `https://acadsign.university.ma/api/v1`

**Authentication:** OAuth 2.0 / JWT Bearer token

#### Document Generation & Signing Endpoints

**1. Generate Unsigned Document**
```
POST /api/v1/documents/generate
Headers: Authorization: Bearer <JWT>
Content-Type: application/json

Request Body:
{
  "studentId": "string",
  "firstName": "string",
  "lastName": "string",
  "firstNameAr": "string",
  "lastNameAr": "string",
  "cin": "string (encrypted)",
  "cne": "string (encrypted)",
  "program": "string",
  "academicYear": "string",
  "documentType": "ATTESTATION_SCOLARITE | RELEVE_NOTES | ATTESTATION_REUSSITE",
  "institutionId": "string",
  "grades": [ ... ] (optional, for transcripts),
  "gpa": "number" (optional),
  "mention": "string" (optional)
}

Response: 201 Created
{
  "documentId": "uuid",
  "status": "UNSIGNED",
  "unsignedPdfUrl": "string",
  "createdAt": "ISO8601 timestamp"
}
```

**2. Get Unsigned Document (Desktop App)**
```
GET /api/v1/documents/{documentId}/unsigned
Headers: Authorization: Bearer <JWT>

Response: 200 OK
Content-Type: application/pdf
Body: PDF binary (unsigned)
```

**3. Upload Signed Document (Desktop App)**
```
POST /api/v1/documents/{documentId}/upload-signed
Headers: Authorization: Bearer <JWT>
Content-Type: multipart/form-data

Request Body:
- signedPdf: PDF file (binary)
- certificateSerial: string
- signatureTimestamp: ISO8601 timestamp

Response: 200 OK
{
  "documentId": "uuid",
  "status": "SIGNED",
  "signedPdfUrl": "string",
  "downloadLink": "string (pre-signed URL)",
  "qrCodeData": "string"
}
```

**4. Get Document Metadata**
```
GET /api/v1/documents/{documentId}
Headers: Authorization: Bearer <JWT>

Response: 200 OK
{
  "documentId": "uuid",
  "studentId": "string",
  "documentType": "string",
  "status": "UNSIGNED | SIGNED | FAILED",
  "createdAt": "ISO8601",
  "signedAt": "ISO8601" (nullable),
  "certificateSerial": "string" (nullable),
  "downloadLink": "string" (nullable)
}
```

**5. Get Download Link**
```
GET /api/v1/documents/{documentId}/download
Headers: Authorization: Bearer <JWT>

Response: 200 OK
{
  "downloadUrl": "string (pre-signed S3 URL, valid 1 hour)",
  "expiresAt": "ISO8601 timestamp"
}
```

#### Batch Processing Endpoints

**6. Submit Batch**
```
POST /api/v1/documents/batch
Headers: Authorization: Bearer <JWT>
Content-Type: application/json

Request Body:
{
  "batchId": "uuid (optional)",
  "documents": [
    { ... student data object ... },
    { ... student data object ... }
  ]
}

Response: 202 Accepted
{
  "batchId": "uuid",
  "totalDocuments": 500,
  "status": "PROCESSING",
  "createdAt": "ISO8601"
}
```

**7. Get Batch Status**
```
GET /api/v1/documents/batch/{batchId}/status
Headers: Authorization: Bearer <JWT>

Response: 200 OK
{
  "batchId": "uuid",
  "status": "PROCESSING | COMPLETED | FAILED",
  "totalDocuments": 500,
  "processedDocuments": 350,
  "failedDocuments": 2,
  "documents": [
    {
      "documentId": "uuid",
      "status": "SIGNED | FAILED",
      "error": "string (nullable)"
    }
  ]
}
```

#### Verification Endpoint (Public)

**8. Verify Document Signature**
```
GET /api/v1/documents/verify/{documentId}
No authentication required (public endpoint)

Response: 200 OK
{
  "documentId": "uuid",
  "isValid": true,
  "documentType": "Attestation de Scolarité",
  "issuedBy": "Université Hassan II Casablanca",
  "studentName": "string",
  "signedAt": "ISO8601",
  "certificateSerial": "string",
  "certificateStatus": "VALID | EXPIRED | REVOKED",
  "certificateValidUntil": "ISO8601"
}
```

#### Template Management Endpoints

**9. List Templates**
```
GET /api/v1/templates
Headers: Authorization: Bearer <JWT>

Response: 200 OK
{
  "templates": [
    {
      "templateId": "uuid",
      "documentType": "ATTESTATION_SCOLARITE",
      "institutionId": "string",
      "version": "1.0",
      "createdAt": "ISO8601"
    }
  ]
}
```

**10. Upload Template**
```
POST /api/v1/templates
Headers: Authorization: Bearer <JWT>
Content-Type: multipart/form-data

Request Body:
- templateFile: PDF template file
- documentType: string
- institutionId: string

Response: 201 Created
{
  "templateId": "uuid",
  "documentType": "string",
  "version": "1.0"
}
```

#### Audit Trail Endpoint

**11. Get Audit Trail**
```
GET /api/v1/audit/{documentId}
Headers: Authorization: Bearer <JWT>

Response: 200 OK
{
  "documentId": "uuid",
  "events": [
    {
      "eventType": "DOCUMENT_GENERATED",
      "timestamp": "ISO8601",
      "userId": "string",
      "ipAddress": "string",
      "metadata": { ... }
    },
    {
      "eventType": "DOCUMENT_SIGNED",
      "timestamp": "ISO8601",
      "certificateSerial": "string",
      "metadata": { ... }
    },
    {
      "eventType": "DOCUMENT_DOWNLOADED",
      "timestamp": "ISO8601",
      "userId": "string",
      "metadata": { ... }
    }
  ]
}
```

### Authentication & Authorization Model

**OAuth 2.0 Flows:**

**1. SIS Laravel → Backend API (Machine-to-Machine)**
- Flow: **Client Credentials Grant**
- Client ID + Client Secret
- JWT access token (1 hour validity)
- No refresh token (re-authenticate when expired)

**2. Desktop App → Backend API (User Authentication)**
- Flow: **Authorization Code with PKCE**
- User login (registrar staff credentials)
- JWT access token (1 hour validity)
- Refresh token (7 days validity)
- Token stored securely in Windows Credential Manager

**3. Public Verification Portal**
- No authentication required
- Rate-limited by IP address (1000 requests/minute)

**RBAC (Role-Based Access Control):**

| Role | Permissions |
|------|-------------|
| **Admin** | Full access: manage templates, view audit logs, configure system |
| **Registrar** | Generate documents, sign documents, view own documents |
| **API Client** (SIS) | Generate documents, view own documents |
| **Auditor** | Read-only access to audit logs |

**JWT Claims:**
```json
{
  "sub": "user-id",
  "role": "Registrar",
  "institutionId": "university-hassan-ii",
  "exp": 1234567890,
  "iat": 1234567890
}
```

### Data Schemas

**Student Data JSON Schema:**
```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "required": ["studentId", "firstName", "lastName", "cin", "program", "academicYear", "documentType"],
  "properties": {
    "studentId": { "type": "string" },
    "firstName": { "type": "string" },
    "lastName": { "type": "string" },
    "firstNameAr": { "type": "string" },
    "lastNameAr": { "type": "string" },
    "cin": { "type": "string", "pattern": "^[A-Z]{1,2}[0-9]{6}$" },
    "cne": { "type": "string", "pattern": "^[A-Z0-9]{10}$" },
    "dateOfBirth": { "type": "string", "format": "date" },
    "program": { "type": "string" },
    "faculty": { "type": "string" },
    "department": { "type": "string" },
    "academicYear": { "type": "string", "pattern": "^[0-9]{4}-[0-9]{4}$" },
    "enrollmentStatus": { "type": "string", "enum": ["ACTIVE", "SUSPENDED", "GRADUATED"] },
    "documentType": { "type": "string", "enum": ["ATTESTATION_SCOLARITE", "RELEVE_NOTES", "ATTESTATION_REUSSITE", "ATTESTATION_INSCRIPTION"] },
    "grades": {
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "subject": { "type": "string" },
          "grade": { "type": "number", "minimum": 0, "maximum": 20 },
          "credits": { "type": "number" }
        }
      }
    },
    "gpa": { "type": "number", "minimum": 0, "maximum": 20 },
    "mention": { "type": "string", "enum": ["Passable", "Assez Bien", "Bien", "Très Bien"] }
  }
}
```

### Error Handling & HTTP Status Codes

**Standard HTTP Status Codes:**

| Code | Meaning | Usage |
|------|---------|-------|
| 200 | OK | Successful GET request |
| 201 | Created | Successful POST (resource created) |
| 202 | Accepted | Async operation started (batch) |
| 400 | Bad Request | Invalid JSON schema, missing required fields |
| 401 | Unauthorized | Missing or invalid JWT token |
| 403 | Forbidden | Valid token but insufficient permissions |
| 404 | Not Found | Document ID not found |
| 409 | Conflict | Document already signed |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Server Error | Unexpected server error |
| 503 | Service Unavailable | S3 storage or external service down |

**Error Response Format:**
```json
{
  "error": {
    "code": "INVALID_STUDENT_DATA",
    "message": "CIN format is invalid",
    "details": {
      "field": "cin",
      "expectedFormat": "^[A-Z]{1,2}[0-9]{6}$"
    },
    "timestamp": "ISO8601",
    "requestId": "uuid"
  }
}
```

### Rate Limiting

**Per-endpoint rate limits:**

| Endpoint | Limit | Scope |
|----------|-------|-------|
| `POST /api/v1/documents/generate` | 100 req/min | Per JWT client |
| `POST /api/v1/documents/batch` | 10 req/min | Per JWT client |
| `GET /api/v1/documents/verify` | 1000 req/min | Global (all clients) |
| All other endpoints | 200 req/min | Per JWT client |

**Rate limit headers:**
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 45
X-RateLimit-Reset: 1234567890 (Unix timestamp)
```

**429 Response:**
```
HTTP/1.1 429 Too Many Requests
Retry-After: 60
Content-Type: application/json

{
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Rate limit exceeded. Retry after 60 seconds."
  }
}
```

### API Versioning

**Versioning strategy:** URL-based versioning

- **Current version:** `/api/v1/`
- **Future versions:** `/api/v2/`, `/api/v3/`

**Backward compatibility:**
- v1 endpoints maintained for minimum 12 months after v2 release
- Deprecation warnings in response headers: `Deprecation: true`, `Sunset: 2027-01-01`

### API Documentation

**OpenAPI 3.0 Specification:**
- Full OpenAPI spec available at `/api/v1/swagger.json`
- Interactive Swagger UI at `/api/v1/docs`
- Includes request/response schemas, authentication flows, error codes

**SDK (Post-MVP):**
- .NET NuGet package: `AcadSign.Client`
- Auto-generated from OpenAPI spec
- Includes typed models, authentication helpers, retry logic

## Project Scoping & Deployment Strategy

### Project Approach

**AcadSign** is scoped as a **production-ready solution**, not an iterative MVP. All documented features are required for production deployment. The project follows a **pilot-then-scale** deployment strategy to validate technical, legal, and operational feasibility before full rollout.

**Strategic rationale:**
- Universities require a **complete, legally-compliant solution** — partial implementations create legal and operational risks
- Electronic signature infrastructure must be **production-grade from day one** (no room for "beta" signatures)
- CNDP compliance and Barid Al-Maghrib integration require **full implementation** before any real student documents can be issued

### Deployment Phases

#### Phase 1: Pilot Validation (1 Month)

**Objective:** Validate technical feasibility, legal compliance, and operational workflows before full production deployment.

**Scope:**
- **Single faculty** within one university (e.g., Faculty of Sciences, Université Hassan II Casablanca)
- **Single workstation** with one Barid Al-Maghrib USB dongle
- **100 documents** signed successfully across all document types
- **Full feature set** deployed (not a reduced subset)

**Success criteria:**
- 100% signature success rate (no failed signatures)
- Average signing time < 30 seconds per document
- Zero security incidents or data leaks
- CNDP compliance validated (F211 declaration approved)
- Barid Al-Maghrib certificate validation working correctly
- Public verification portal accessible and functional
- Registrar staff trained and comfortable with Desktop App

**Deliverables:**
- Pilot validation report
- Performance metrics (signing time, error rates, user satisfaction)
- Security audit results
- CNDP compliance confirmation
- Lessons learned and operational improvements

**Exit criteria to proceed to Phase 2:**
- All 100 documents signed without errors
- Registrar staff approval and confidence in the system
- IT department validation of security and infrastructure
- Legal department confirmation of CNDP compliance
- University administration approval for full deployment

#### Phase 2: Production Deployment (Multi-Faculty)

**Objective:** Deploy AcadSign across all faculties of the university with full operational support.

**Scope:**
- **All faculties** within the university
- **Multiple workstations** (one per faculty registrar office)
- **Multiple USB dongles** (one per workstation)
- **Unlimited document volume** (production scale)

**Rollout strategy:**
- **Week 1-2:** Deploy to 2-3 additional faculties (low-volume faculties first)
- **Week 3-4:** Deploy to remaining faculties
- **Week 5-6:** Monitor, optimize, and stabilize
- **Week 7+:** Full production operation

**Training & Support:**
- On-site training for registrar staff (2 hours per faculty)
- Desktop App user manual (French/Arabic)
- IT administrator training (infrastructure, troubleshooting)
- Helpdesk support (email + phone)
- Escalation process for critical issues

**Monitoring & Optimization:**
- Daily monitoring of signing success rates
- Weekly performance reports to university administration
- Monthly security audits
- Quarterly CNDP compliance reviews

### Production Feature Set

**All features are production-ready and deployed in Phase 1 (Pilot):**

#### Document Management
- ✅ **4 document types:**
  - Attestation de Scolarité (Enrollment Certificate)
  - Relevé de Notes (Transcript)
  - Attestation de Réussite (Certificate of Achievement)
  - Attestation d'Inscription (Registration Certificate)
- ✅ **Bilingual templates** (Arabic/French)
- ✅ **Dynamic data population** from SIS Laravel
- ✅ **QR code generation** with UUID v4 (non-predictable)
- ✅ **Batch processing** (up to 500 documents per batch)

#### Electronic Signature
- ✅ **Desktop App WPF MVVM** (.NET 10)
- ✅ **USB dongle integration** (Barid Al-Maghrib Class 3 certificate)
- ✅ **PAdES signature format** (PDF Advanced Electronic Signature)
- ✅ **Client-side signing** (signature computed on workstation)
- ✅ **Certificate validation** (OCSP/CRL)
- ✅ **RFC 3161 timestamping** (immutable proof of signature time)

#### Backend API
- ✅ **11 REST endpoints** (document generation, signing, verification, audit, templates, batch)
- ✅ **OAuth 2.0 / JWT authentication** (Client Credentials + Authorization Code PKCE)
- ✅ **RBAC** (Admin, Registrar, Auditor, API Client)
- ✅ **Rate limiting** (per endpoint, per client)
- ✅ **API versioning** (`/api/v1/`)
- ✅ **OpenAPI 3.0 documentation** (Swagger UI)

#### Storage & Security
- ✅ **S3-compatible storage** with SSE-KMS encryption
- ✅ **Application-level PII encryption** (CIN, CNE, email, phone)
- ✅ **TLS 1.3** for data in transit
- ✅ **Immutable audit trail** (30 years retention)
- ✅ **Pre-signed download URLs** (1 hour validity)

#### Integration
- ✅ **SIS Laravel integration** (JWT authentication, JSON payloads)
- ✅ **Webhook support** (async notifications when documents ready)
- ✅ **CSV batch import** (bulk document generation)

#### Verification & Compliance
- ✅ **Public verification portal** (QR code scan → signature validation)
- ✅ **Certificate status check** (VALID | EXPIRED | REVOKED)
- ✅ **CNDP compliance** (F211 declaration, data minimization, student rights)
- ✅ **30-year document retention** (legal requirement)

#### Administration & Monitoring
- ✅ **Admin dashboard** (metrics, document stats, signing success rates)
- ✅ **Template management** (upload, version, multi-institution branding)
- ✅ **Audit log access** (read-only for auditors)
- ✅ **Email notifications** (automatic download links to students)
- ✅ **Alerting** (certificate expiry, signing failures, storage thresholds)

### Resource Requirements

#### Development Team (Production Delivery)

**Core team:**
- **1 Backend Developer** (.NET 10, ASP.NET Core, Entity Framework, S3, OAuth 2.0)
- **1 Desktop Developer** (.NET 10, WPF MVVM, PKCS#11, PAdES signature, cryptography)
- **1 Product Manager / QA** (requirements, testing, CNDP compliance, documentation)

**Optional (for faster delivery):**
- **1 DevOps Engineer** (Docker, Kubernetes, CI/CD, monitoring)
- **1 Security Specialist** (penetration testing, CNDP audit, cryptography review)

**Timeline:** 4-6 months for full production delivery

#### Infrastructure Requirements

**Backend API:**
- **Server:** 4 vCPU, 8 GB RAM, 100 GB SSD (containerized .NET 10 app)
- **Database:** PostgreSQL 15+ (4 vCPU, 8 GB RAM, 200 GB SSD)
- **S3 Storage:** MinIO (1 TB initial capacity, scalable)
- **Load Balancer:** Optional for multi-server deployment
- **Hosting:** Morocco-based datacenter (CNDP compliance) or on-premise university infrastructure

**Desktop Application:**
- **Workstations:** Windows 10/11 Pro (4 GB RAM minimum, 8 GB recommended)
- **USB Dongles:** Barid Al-Maghrib Class 3 tokens (one per workstation)
- **Network:** Stable internet connection (minimum 10 Mbps upload for PDF uploads)

**SIS Integration:**
- **Laravel SIS:** Existing university infrastructure (no additional requirements)
- **API connectivity:** HTTPS access from SIS to AcadSign backend

### Risk Mitigation Strategy

#### Technical Risks

**Risk 1: USB dongle integration complexity**
- **Mitigation:** Pilot phase with 1 dongle on 1 workstation validates integration before scaling
- **Fallback:** If PKCS#11 fails, use Windows CSP (Cryptographic Service Provider) as alternative
- **Validation:** 100 successful signatures in pilot = proof of technical feasibility

**Risk 2: Barid Al-Maghrib API reliability**
- **Mitigation:** Dead-letter queue for retry automation, monitoring of signature success rates
- **Fallback:** Unsigned document generation with deferred signing if API is down
- **SLA:** Monitor Barid Al-Maghrib uptime, escalate if < 99%

**Risk 3: S3 storage unavailability**
- **Mitigation:** Use S3-compatible storage with high availability (99.9%+)
- **Fallback:** Local temporary storage on backend server, sync to S3 when available
- **Monitoring:** Alerting on S3 connection failures

#### Legal & Compliance Risks

**Risk 4: CNDP non-compliance**
- **Mitigation:** F211 declaration submitted before pilot, legal review of data handling
- **Validation:** CNDP approval received before any real student documents are signed
- **Ongoing:** Quarterly compliance reviews, annual CNDP audit

**Risk 5: Signature legal validity challenged**
- **Mitigation:** Use Barid Al-Maghrib Class 3 certificate (highest legal standing in Morocco)
- **Proof:** Immutable audit trail with RFC 3161 timestamping (non-repudiation)
- **Documentation:** Legal opinion from university legal department confirming validity

#### Operational Risks

**Risk 6: Low user adoption (registrar staff resistance)**
- **Mitigation:** Early involvement of registrar staff in pilot, comprehensive training
- **Change management:** Demonstrate time savings (manual process: 10 min/doc → automated: 30 sec/doc)
- **Support:** Dedicated helpdesk during first 3 months

**Risk 7: Certificate expiration causing production outage**
- **Mitigation:** Alerts 3 months before expiration, documented renewal process
- **Backup:** Backup dongle with valid certificate available for business continuity
- **Monitoring:** Daily certificate validity checks

**Risk 8: Insufficient resources for full deployment**
- **Mitigation:** Pilot phase validates resource requirements before full commitment
- **Contingency:** If resources are insufficient, deploy to fewer faculties initially (phased rollout)
- **Minimum viable deployment:** 1 faculty with 1 dongle = functional system

### Success Metrics (Production)

**Technical metrics:**
- **Signature success rate:** > 99.5%
- **Average signing time:** < 30 seconds per document
- **System uptime:** > 99% (excluding planned maintenance)
- **API response time:** < 500ms (p95)

**Operational metrics:**
- **Documents signed per month:** 5,000+ (across all faculties)
- **Batch processing efficiency:** 500 documents signed in < 15 minutes
- **Registrar staff satisfaction:** > 4/5 (post-deployment survey)

**Business metrics:**
- **Time to issue document:** Days/weeks → Minutes (90%+ reduction)
- **Student campus visits eliminated:** 100% (zero physical visits required)
- **Operational cost savings:** 50%+ reduction in registrar administrative workload

**Compliance metrics:**
- **CNDP compliance:** 100% (F211 declaration approved, ongoing compliance)
- **Audit trail completeness:** 100% (all events logged for 30 years)
- **Certificate validity:** 100% (no expired or revoked certificates in use)

## Functional Requirements

### 1. Document Generation & Management

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

### 2. Electronic Signature

**FR12:** Registrar staff can sign PDF documents using USB dongle (Barid Al-Maghrib Class 3 certificate)

**FR13:** Desktop application can detect and access USB dongle via PKCS#11 or Windows CSP

**FR14:** Desktop application can prompt for PIN code to unlock USB dongle

**FR15:** Desktop application can apply PAdES signature format to PDF documents

**FR16:** System can validate certificate status (VALID, EXPIRED, REVOKED) via OCSP/CRL

**FR17:** System can apply RFC 3161 timestamping to signatures for non-repudiation

**FR18:** Desktop application can upload signed PDFs to backend API

**FR19:** System can handle signature failures and retry logic

**FR20:** Desktop application can display batch signing progress and status

### 3. Document Verification

**FR21:** Public users can verify document authenticity by scanning QR code or entering document ID

**FR22:** Verification portal can validate electronic signature cryptographically

**FR23:** Verification portal can display document metadata (type, issuing institution, student name, signature date)

**FR24:** Verification portal can display certificate status (VALID, EXPIRED, REVOKED)

**FR25:** Verification portal can display certificate validity period

**FR26:** System can verify signatures without requiring authentication (public endpoint)

### 4. User & Access Management

**FR27:** System can authenticate registrar staff via OAuth 2.0 Authorization Code with PKCE

**FR28:** System can authenticate SIS Laravel via OAuth 2.0 Client Credentials

**FR29:** System can issue JWT access tokens (1 hour validity) and refresh tokens (7 days validity)

**FR30:** System can enforce role-based access control (Admin, Registrar, Auditor, API Client)

**FR31:** Admin users can manage user accounts and assign roles

**FR32:** System can store JWT tokens securely in Windows Credential Manager (Desktop App)

**FR33:** System can rotate JWT secrets every 90 days

### 5. SIS Integration

**FR34:** SIS Laravel can submit document generation requests via REST API with student data (JSON/XML/CSV)

**FR35:** System can validate student data against JSON schema before processing

**FR36:** System can return document generation status to SIS via API response

**FR37:** System can send webhook notifications to SIS when documents are ready (optional)

**FR38:** System can accept batch document generation requests from SIS

**FR39:** System can provide batch status endpoint for SIS to poll progress

### 6. Template Management

**FR40:** Admin users can upload document templates (PDF format)

**FR41:** Admin users can associate templates with document types and institutions

**FR42:** System can version document templates

**FR43:** Admin users can list available templates

**FR44:** System can support multi-institution branding (different templates per university)

### 7. Audit & Compliance

**FR45:** System can log all document lifecycle events (generation, signing, download) in immutable audit trail

**FR46:** System can store audit logs for 30 years (legal requirement)

**FR47:** Auditor users can retrieve audit trail for specific documents

**FR48:** System can encrypt sensitive student data (CIN, CNE, email, phone) at application level

**FR49:** System can enforce data minimization (collect only necessary data)

**FR50:** System can provide student data access/rectification/deletion capabilities (CNDP compliance)

**FR51:** System can retain documents for 30 years in compliance with Moroccan law

**FR52:** System can generate CNDP compliance reports

### 8. Administration & Monitoring

**FR53:** Admin users can view dashboard with document generation metrics (total documents, success rate, volume by type)

**FR54:** Admin users can view signing success rate metrics

**FR55:** System can alert administrators when certificate expiration is approaching (3 months before)

**FR56:** System can alert administrators on signing failures

**FR57:** System can alert administrators on storage threshold warnings

**FR58:** Admin users can view system health status (API uptime, S3 availability, dongle connectivity)

**FR59:** System can enforce rate limiting per endpoint and per client

**FR60:** System can return rate limit headers (X-RateLimit-Limit, X-RateLimit-Remaining, X-RateLimit-Reset)

**FR61:** System can provide OpenAPI 3.0 specification and interactive Swagger UI documentation

## Non-Functional Requirements

### Performance

**NFR-P1:** API response time for document generation must be < 3 seconds (p95)

**NFR-P2:** API response time for all other endpoints must be < 500ms (p95)

**NFR-P3:** Desktop application signature operation must complete in < 30 seconds per document

**NFR-P4:** Batch signing of 500 documents must complete in < 15 minutes

**NFR-P5:** Public verification portal must respond in < 2 seconds (p95)

**NFR-P6:** Pre-signed S3 download URLs must be generated in < 1 second

**NFR-P7:** System must support concurrent signing operations (up to 10 simultaneous desktop app users)

### Security

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

### Reliability & Availability

**NFR-R1:** Backend API must maintain 99% uptime (excluding planned maintenance)

**NFR-R2:** Signature success rate must be > 99.5%

**NFR-R3:** S3 storage must maintain 99.9% availability

**NFR-R4:** System must implement dead-letter queue for failed signature operations with automatic retry

**NFR-R5:** System must implement graceful degradation (unsigned document generation if signature service unavailable)

**NFR-R6:** Database backups must be performed daily with 30-day retention

**NFR-R7:** System must support disaster recovery with Recovery Time Objective (RTO) < 4 hours

**NFR-R8:** System must support disaster recovery with Recovery Point Objective (RPO) < 1 hour

### Scalability

**NFR-SC1:** System must support 5,000+ documents signed per month across all faculties

**NFR-SC2:** System must support up to 50 concurrent API clients (SIS integrations)

**NFR-SC3:** System must support up to 10 concurrent desktop application users (registrar staff)

**NFR-SC4:** System must scale to support 10x document volume growth with < 10% performance degradation

**NFR-SC5:** S3 storage must scale to support 1 TB initial capacity with automatic expansion

**NFR-SC6:** Database must support horizontal scaling for read replicas

### Compliance & Legal

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

### Integration

**NFR-I1:** System must support JSON, XML, and CSV data formats for SIS integration

**NFR-I2:** System must provide OpenAPI 3.0 specification for all REST endpoints

**NFR-I3:** System must provide interactive Swagger UI documentation

**NFR-I4:** System must support webhook notifications for async document status updates

**NFR-I5:** System must validate all incoming student data against JSON schema

**NFR-I6:** System must support OAuth 2.0 Client Credentials flow for machine-to-machine authentication

**NFR-I7:** System must support OAuth 2.0 Authorization Code with PKCE for user authentication

**NFR-I8:** System must enforce rate limiting (100 req/min for document generation, 1000 req/min for verification)

### Maintainability & Operability

**NFR-M1:** System must provide structured logging with correlation IDs for request tracing

**NFR-M2:** System must provide monitoring dashboards for key metrics (signature success rate, API uptime, storage usage)

**NFR-M3:** System must provide alerting for critical events (certificate expiry, signature failures, storage thresholds)

**NFR-M4:** Desktop application must support auto-update mechanism for version management

**NFR-M5:** System must support containerized deployment (Docker)

**NFR-M6:** System must support infrastructure-as-code for reproducible deployments

**NFR-M7:** System must provide health check endpoints for monitoring

### Usability

**NFR-U1:** Desktop application must support French and Arabic languages

**NFR-U2:** Desktop application must provide clear error messages for dongle connectivity issues

**NFR-U3:** Desktop application must display batch signing progress with estimated time remaining

**NFR-U4:** Admin dashboard must be accessible via modern web browsers (Chrome, Firefox, Edge, Safari)

**NFR-U5:** Public verification portal must be mobile-responsive

