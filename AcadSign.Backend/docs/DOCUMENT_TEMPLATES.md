# Templates de Documents Académiques - Guide d'Utilisation

## Vue d'ensemble

Ce document explique comment utiliser les 4 templates de documents académiques bilingues implémentés dans AcadSign Backend API.

## Types de Documents

### 1. Attestation de Scolarité (شهادة مدرسية)

Certifie qu'un étudiant est régulièrement inscrit dans l'établissement.

**Contenu:**
- Déclaration bilingue
- Nom de l'étudiant (AR/FR)
- CIN, CNE, date de naissance
- Programme d'études (AR/FR)
- Faculté (AR/FR)
- Année académique
- Déclaration de validité légale

**Exemple d'utilisation:**
```json
POST /api/v1/documents/generate
{
  "documentType": "AttestationScolarite",
  "studentId": "uuid",
  "studentData": {
    "firstNameAr": "أحمد",
    "lastNameAr": "بنعلي",
    "firstNameFr": "Ahmed",
    "lastNameFr": "Benali",
    "cin": "AB123456",
    "cne": "R123456789",
    "dateOfBirth": "2000-01-15",
    "programNameAr": "ماجستير في علوم الحاسوب",
    "programNameFr": "Master en Informatique",
    "facultyAr": "كلية العلوم",
    "facultyFr": "Faculté des Sciences",
    "academicYear": "2025-2026"
  }
}
```

### 2. Relevé de Notes (كشف النقاط)

Présente les notes obtenues par l'étudiant avec tableau détaillé.

**Contenu:**
- Tableau des notes par matière (AR/FR)
- Note sur 20
- Crédits ECTS
- Moyenne Générale (GPA)
- Mention
- Total des crédits

**Exemple d'utilisation:**
```json
POST /api/v1/documents/generate
{
  "documentType": "ReleveNotes",
  "studentId": "uuid",
  "studentData": {
    "firstNameAr": "أحمد",
    "lastNameAr": "بنعلي",
    "firstNameFr": "Ahmed",
    "lastNameFr": "Benali",
    "cin": "AB123456",
    "cne": "R123456789",
    "dateOfBirth": "2000-01-15",
    "programNameAr": "ماجستير في علوم الحاسوب",
    "programNameFr": "Master en Informatique",
    "facultyAr": "كلية العلوم",
    "facultyFr": "Faculté des Sciences",
    "academicYear": "2025-2026",
    "grades": [
      {
        "subjectNameAr": "الخوارزميات",
        "subjectNameFr": "Algorithmes",
        "score": 16.5,
        "credits": 6
      },
      {
        "subjectNameAr": "قواعد البيانات",
        "subjectNameFr": "Bases de Données",
        "score": 15.0,
        "credits": 5
      }
    ],
    "gpa": 15.8,
    "mention": "Bien"
  }
}
```

### 3. Attestation de Réussite (شهادة نجاح)

Certifie qu'un étudiant a obtenu son diplôme avec succès.

**Contenu:**
- Déclaration de certification
- Nom de l'étudiant (AR/FR)
- Diplôme obtenu (AR/FR)
- Année d'obtention
- Mention

**Exemple d'utilisation:**
```json
POST /api/v1/documents/generate
{
  "documentType": "AttestationReussite",
  "studentId": "uuid",
  "studentData": {
    "firstNameAr": "أحمد",
    "lastNameAr": "بنعلي",
    "firstNameFr": "Ahmed",
    "lastNameFr": "Benali",
    "cin": "AB123456",
    "cne": "R123456789",
    "dateOfBirth": "2000-01-15",
    "degreeNameAr": "ماجستير في علوم الحاسوب",
    "degreeNameFr": "Master en Informatique",
    "graduationYear": 2025,
    "mention": "Très Bien"
  }
}
```

### 4. Attestation d'Inscription (شهادة تسجيل)

Certifie l'inscription d'un étudiant pour l'année académique en cours.

**Contenu:**
- Déclaration d'inscription
- Nom complet (AR/FR)
- Programme d'inscription
- Année académique
- Date d'inscription
- Statut d'inscription

**Exemple d'utilisation:**
```json
POST /api/v1/documents/generate
{
  "documentType": "AttestationInscription",
  "studentId": "uuid",
  "studentData": {
    "firstNameAr": "أحمد",
    "lastNameAr": "بنعلي",
    "firstNameFr": "Ahmed",
    "lastNameFr": "Benali",
    "cin": "AB123456",
    "cne": "R123456789",
    "dateOfBirth": "2000-01-15",
    "programNameAr": "ماجستير في علوم الحاسوب",
    "programNameFr": "Master en Informatique",
    "academicYear": "2025-2026",
    "enrollmentDate": "2025-09-01",
    "enrollmentStatus": "Régulièrement inscrit(e)"
  }
}
```

## Réponse API

Toutes les requêtes retournent une réponse standardisée:

```json
{
  "documentId": "uuid-v4",
  "status": "UNSIGNED",
  "unsignedPdfUrl": "/api/v1/documents/{id}/unsigned",
  "createdAt": "2026-03-04T20:00:00Z"
}
```

## Mentions Disponibles

- **Passable**: 10-11.99/20
- **Assez Bien**: 12-13.99/20
- **Bien**: 14-15.99/20
- **Très Bien**: 16-20/20

## Conformité

✅ **FR1**: Génération de 4 types de documents  
✅ **FR2**: Support bilingue (Arabe/Français)  
✅ **FR3**: UUID v4 unique pour chaque document  
✅ **NFR-P1**: Performance < 3 secondes

## Évolutions Futures

- **Story 3.3**: QR codes avec données sécurisées
- **Story 3.4**: Stockage MinIO S3
- **Story 3.5**: Pre-signed URLs
- **Story 3.6**: Template management

## Références

- **Architecture**: `_bmad-output/planning-artifacts/architecture.md`
- **Story 3.2**: `_bmad-output/implementation-artifacts/3-2-implementer-generation-des-4-types-de-documents.md`
