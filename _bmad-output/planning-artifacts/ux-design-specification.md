---
stepsCompleted: [1, 2, 3, 4]
inputDocuments: 
  - '/Users/macbookpro/e-sign/_bmad-output/planning-artifacts/prd.md'
  - '/Users/macbookpro/e-sign/_bmad-output/planning-artifacts/architecture.md'
  - '/Users/macbookpro/e-sign/_bmad-output/planning-artifacts/WPF UI Design.png'
  - '/Users/macbookpro/e-sign/_bmad-output/planning-artifacts/AcadSign/' (prototype WPF complet)
workflowType: 'ux-design'
project_name: 'AcadSign'
user_name: 'Macbookpro'
date: '2026-03-05'
prototypeSource: '/Users/macbookpro/e-sign/_bmad-output/planning-artifacts/AcadSign/'
---

# UX Design Specification AcadSign

**Author:** Macbookpro
**Date:** 2026-03-05

---

## Executive Summary

### Project Vision

**AcadSign Desktop** est une application WPF moderne qui digitalise complètement le processus de délivrance de documents académiques officiels dans les universités marocaines. L'application permet au personnel de scolarité de générer, signer électroniquement (via USB dongle Barid Al-Maghrib), et distribuer des centaines de documents en quelques minutes, transformant un processus manuel de plusieurs jours en un workflow automatisé quasi-instantané.

**Transformation clé:** De 40 documents/jour traités manuellement → 500+ documents/jour signés en batch avec signatures électroniques qualifiées légalement valides au Maroc.

### Target Users

**Utilisateur Principal: Responsable du Service de Scolarité**
- **Profil:** Fatima, 42 ans, 15 ans d'expérience administrative universitaire
- **Contexte d'usage:** Bureau, 8h-18h, traitement quotidien de demandes étudiantes
- **Compétences techniques:** Intermédiaires (utilise SIS, email, Word/Excel)
- **Pain points actuels:** Files d'attente physiques, processus manuel répétitif, erreurs de saisie, stress quotidien
- **Objectif:** Traiter le maximum de demandes rapidement et sans erreur, réduire les files d'attente

**Utilisateur Secondaire: Administrateur IT**
- **Profil:** Karim, 35 ans, ingénieur système
- **Contexte d'usage:** Configuration initiale, monitoring, maintenance
- **Compétences techniques:** Avancées (Docker, API, certificats PKI)
- **Objectif:** Déploiement fiable, monitoring proactif, zéro downtime

**Bénéficiaires Indirects: Étudiants**
- **Profil:** Youssef, 24 ans, étudiant en Master
- **Interaction:** Reçoit documents par email, vérifie authenticité via QR code
- **Attente:** Réception rapide (< 5 minutes), documents légalement valides

### Key Design Challenges

**1. Complexité Cryptographique Masquée**
- Signature PAdES avec USB dongle nécessite ~30 secondes par document
- PIN code requis (sécurité) vs friction utilisateur
- Gestion d'erreurs cryptographiques complexes (certificat expiré, validation OCSP/CRL, dongle déconnecté)
- **Défi UX:** Rendre un processus technique complexe simple et rassurant

**2. Batch Operations à Grande Échelle**
- Traitement de 500 documents simultanément sans erreur
- Progress tracking granulaire (document par document)
- Gestion des échecs partiels (retry automatique vs intervention manuelle)
- **Défi UX:** Feedback clair et continu, gestion d'erreurs gracieuse, pas de perte de travail

**3. Bilinguisme Technique (Français/Arabe)**
- Interface bilingue avec support RTL (Right-to-Left) pour l'arabe
- Documents générés en arabe + français dans le même PDF
- Messages d'erreur et feedback localisés
- **Défi UX:** Cohérence visuelle malgré directions de lecture opposées

**4. Fiabilité Perçue et Confiance**
- Signatures électroniques légalement contraignantes (Loi 43-20)
- Dépendance à des services externes (Barid Al-Maghrib e-Sign API, SIS, S3)
- Audit trail immuable requis (conformité CNDP)
- **Défi UX:** Indicateurs de statut temps réel, logs visibles, feedback rassurant sur validité légale

**5. Workflow Interrompu (USB Dongle)**
- Dongle peut être déconnecté accidentellement
- PIN code oublié (3 tentatives max avant blocage)
- Certificat peut expirer pendant une session
- **Défi UX:** Détection proactive, messages d'erreur actionnables, recovery gracieux

### Design Opportunities

**1. Dark Theme Professionnel et Moderne**
- Thème sombre (#070C14, #0D1520) réduit fatigue visuelle pour usage prolongé
- Palette de couleurs sémantiques (vert=succès, orange=warning, rouge=erreur, indigo=accent)
- Look premium qui inspire confiance et modernité
- **Opportunité:** Différenciation visuelle vs applications administratives traditionnelles

**2. Dashboard Temps Réel avec Indicateurs Visuels**
- Compteurs en direct: Pending, Signed, Total, Selected
- Indicateurs de statut colorés pour services externes (e-Sign API, S3, SIS)
- Progress bars détaillées pour batch operations
- **Opportunité:** Visibilité complète de l'état du système en un coup d'œil

**3. Visualisation PDF Inline Avant/Après Signature**
- Aperçu document directement dans l'interface (PdfiumViewer)
- Comparaison visuelle avant/après signature
- Zoom, navigation pages, pas besoin d'ouvrir lecteur PDF externe
- **Opportunité:** Validation visuelle immédiate, workflow fluide sans changement de contexte

**4. Batch Operations Intelligentes**
- Sélection multiple avec "Select All Pending"
- Signature par lot avec progress granulaire (document par document)
- Retry automatique avec exponential backoff
- Dead-letter queue pour échecs persistants
- **Opportunité:** Efficacité maximale pour traitement de masse, résilience aux erreurs

**5. Feedback Contextuel et Logs Visibles**
- Logs de signature en temps réel (FlowDocument avec couleurs)
- Messages d'erreur actionnables (ex: "Dongle déconnecté → Reconnectez le dongle USB")
- Correlation IDs pour traçabilité complète
- **Opportunité:** Transparence totale, debugging facile, confiance utilisateur

**6. Automation Intelligente**
- Envoi email automatique post-signature
- Upload S3 automatique avec URLs pré-signées
- Refresh automatique des demandes depuis SIS
- **Opportunité:** Réduction des étapes manuelles, workflow end-to-end automatisé

## Core User Experience

### Defining Experience

**L'expérience centrale d'AcadSign Desktop** est la **signature par lot (batch signing)** de documents académiques avec feedback transparent en temps réel. L'utilisateur (personnel de scolarité) charge les demandes depuis l'API SIS, sélectionne un batch de documents en attente, entre son PIN USB dongle une seule fois, et lance la signature de centaines de documents simultanément. Le système gère automatiquement la signature cryptographique PAdES, l'upload S3, et l'envoi email, tout en affichant un progress tracking granulaire et des logs colorés pour chaque document.

**Transformation du workflow:** De "traiter 40 documents manuellement en une journée" à "signer 500 documents en batch en 10 minutes" - une amélioration de 100x en efficacité.

### Platform Strategy

**Plateforme:** Application Desktop WPF Windows (.NET 10)

**Contrainte technique imposée:** La signature électronique qualifiée via USB dongle Barid Al-Maghrib nécessite un accès local au hardware (PKCS#11/Windows CSP). La clé privée ne peut jamais quitter le dongle, rendant impossible une architecture web pure. L'application desktop est donc obligatoire.

**Environnement d'usage:**
- Bureau fixe, écran 1920x1080 minimum
- Interaction souris + clavier (pas de touch)
- Sessions longues (8h-18h de travail continu)
- Multi-tâches (SIS, email, AcadSign ouverts simultanément)

**Implications UX:**
- Interface optimisée pour écran large (3 panneaux: liste, détails, viewer PDF)
- Raccourcis clavier pour power users
- Dark theme pour réduire fatigue visuelle sur sessions longues
- Pas de responsive mobile nécessaire

### Effortless Interactions

**1. Sélection Batch Intelligente**
- Bouton "Select All Pending" coche automatiquement tous les documents en attente
- Filtres visuels par statut (Tous, En attente, Signés, Erreur)
- Recherche temps réel dans la liste de documents
- **Résultat:** Sélectionner 100 documents = 1 clic au lieu de 100

**2. Progress Tracking Transparent**
- Progress bar globale pour le batch entier
- Progress individuel visible pour chaque document
- Logs colorés en temps réel (vert=succès, rouge=erreur, orange=warning)
- **Résultat:** Fatima sait toujours exactement où en est le processus

**3. Gestion d'Erreurs Automatique**
- Retry automatique avec exponential backoff (1min, 5min, 15min, 1h)
- Dead-letter queue capture 100% des échecs pour analyse
- Messages d'erreur actionnables: "Dongle déconnecté → Reconnectez le dongle USB"
- **Résultat:** Erreurs temporaires résolues automatiquement, pas d'intervention manuelle

**4. Automation Post-Signature**
- Upload S3 automatique après chaque signature réussie
- Envoi email automatique avec lien de téléchargement sécurisé
- Mise à jour statut dans SIS via webhook
- **Résultat:** Workflow end-to-end sans étapes manuelles supplémentaires

**5. Visualisation PDF Inline**
- Aperçu avant/après signature directement dans l'interface (PdfiumViewer)
- Zoom, navigation pages, pas besoin d'ouvrir Adobe Reader
- Comparaison visuelle immédiate de la signature électronique
- **Résultat:** Validation visuelle sans changement de contexte

### Critical Success Moments

**Moment de Succès #1: Batch Completed (8 minutes)**
- **Scénario:** Fatima lance un batch de 50 attestations de scolarité
- **Expérience:** Progress bar atteint 100%, logs affichent "50/50 documents signés avec succès ✓"
- **Réalisation:** "Ce qui me prenait 2 jours est fait en 8 minutes"
- **UX Critical:** Progress bar doit être rassurante (couleur verte, pourcentage clair), pas stressante

**Moment de Succès #2: Zero Physical Queue**
- **Scénario:** Fatima arrive au bureau, aucun étudiant en file d'attente
- **Expérience:** Dashboard montre "127 demandes en attente" mais toutes traitées digitalement
- **Réalisation:** "Plus de stress, plus de files d'attente physiques"
- **UX Critical:** Indicateurs de statut temps réel (e-Sign API, S3, SIS) montrent que tout fonctionne

**Moment de Succès #3: Visual Signature Validation**
- **Scénario:** Fatima clique sur "Après signature" pour voir le PDF signé
- **Expérience:** Viewer PDF affiche le document avec signature électronique visible + QR code
- **Réalisation:** "Le document est légalement valide, je peux le voir"
- **UX Critical:** Feedback visuel immédiat et clair de la signature PAdES

**Moment d'Échec Potentiel: Dongle Disconnected**
- **Scénario:** Dongle USB déconnecté accidentellement pendant batch de 100 documents
- **Expérience Actuelle (mauvaise):** Batch échoue complètement, perte de travail
- **Expérience Cible (bonne):** 
  - Détection immédiate avec alerte visuelle/sonore
  - Pause automatique du batch (pas d'échec)
  - Message: "Dongle déconnecté → Reconnectez le dongle USB et entrez le PIN"
  - Après reconnexion: Reprise automatique du batch là où il s'était arrêté
- **UX Critical:** Recovery gracieux sans perte de travail, retry automatique

### Experience Principles

**1. Transparence Totale**
- Tous les états système visibles en permanence via indicateurs colorés (e-Sign API vert, S3 vert, SIS orange)
- Logs de signature en temps réel avec correlation IDs pour traçabilité
- Pas de "boîte noire" - l'utilisateur sait toujours ce qui se passe et pourquoi
- **Application:** Dashboard avec indicateurs de statut, logs visibles, messages d'erreur explicites

**2. Résilience Gracieuse**
- Les erreurs ne bloquent jamais complètement le workflow
- Retry automatique intelligent avec exponential backoff
- Recovery sans perte de travail (batch pause/resume)
- Dead-letter queue pour échecs persistants
- **Application:** Gestion d'erreurs proactive, retry automatique, pas de crash

**3. Efficacité Maximale**
- Batch operations par défaut (pas document par document)
- Automation intelligente (email, S3, webhooks) sans intervention manuelle
- Raccourcis clavier pour power users (Ctrl+A = Select All, Ctrl+S = Sign Batch)
- **Application:** Workflow optimisé pour traiter 500+ documents/jour

**4. Confiance Visuelle**
- Dark theme professionnel (#070C14, #0D1520) inspire sérieux et modernité
- Indicateurs de statut colorés sémantiques (vert=succès, orange=warning, rouge=erreur, indigo=accent)
- Feedback immédiat sur chaque action (bouton cliqué → feedback visuel instantané)
- **Application:** Design qui inspire confiance dans la validité légale des signatures

**5. Simplicité Malgré la Complexité**
- Masquer la complexité cryptographique (PAdES, OCSP, CRL, RFC 3161, SHA-256)
- Interface simple: Charger → Sélectionner → PIN → Signer (4 étapes)
- Messages d'erreur en langage clair: "Certificat expiré → Contactez l'admin IT" (pas "OCSP validation failed: certificate revoked")
- **Application:** UX accessible pour utilisateurs non-techniques

## Desired Emotional Response

### Primary Emotional Goals

**Pour Fatima (Personnel de Scolarité) - Utilisateur Principal:**

**Émotion Primaire: SOULAGEMENT & CONTRÔLE**
- Passage du stress quotidien (files d'attente, pression temporelle) à la sérénité opérationnelle
- Sentiment de maîtrise totale du workflow de signature
- Réalisation: "Je peux enfin respirer, tout est sous contrôle"

**Émotions Secondaires:**
- **Confiance** - Le système est fiable, les signatures sont légalement valides et traçables
- **Efficacité** - Accomplir en 8 minutes ce qui prenait 2 jours manuellement
- **Fierté** - Utiliser un outil moderne et professionnel qui valorise son travail

**Émotions à Éviter:**
- **Anxiété** - Peur de perdre du travail si erreur système ou dongle déconnecté
- **Confusion** - Interface trop complexe ou messages techniques incompréhensibles
- **Frustration** - Processus lent, bloqué, ou nécessitant trop d'interventions manuelles

### Emotional Journey Mapping

**1. Première Découverte (Onboarding Initial)**
- **Émotion Cible:** Curiosité optimiste + Légère appréhension technique
- **Moment:** Fatima lance l'application pour la première fois après formation IT
- **Expérience:** Interface claire avec indicateurs de statut rassurants (e-Sign API vert, S3 vert)
- **Design:** Tooltips explicatifs, dashboard simple, pas de jargon technique

**2. Premier Batch Sign (Moment Critique de Validation)**
- **Émotion Cible:** Confiance croissante → Émerveillement progressif
- **Moment:** Elle lance son premier batch de 10 documents test
- **Expérience:** Progress bar avance régulièrement, logs verts s'affichent en temps réel
- **Design:** Feedback visuel constant, pas de "boîte noire", messages rassurants ("Document 1/10 signé ✓")

**3. Batch Completed (Moment "Aha!" Transformationnel)**
- **Émotion Cible:** Accomplissement + Soulagement + Joie
- **Moment:** "50/50 documents signés avec succès ✓" s'affiche après 8 minutes
- **Expérience:** Réalisation que ce qui prenait 2 jours est fait en 8 minutes
- **Design:** Animation de succès subtile, son de notification, message de félicitations

**4. Usage Quotidien (Routine Établie)**
- **Émotion Cible:** Efficacité sereine + Confiance routinière
- **Moment:** Fatima traite 500 documents par jour sans stress
- **Expérience:** Workflow fluide, automation intelligente, pas de friction
- **Design:** Raccourcis clavier, "Select All Pending" en 1 clic, email automatique

**5. Gestion d'Erreur (Dongle Déconnecté - Test de Résilience)**
- **Émotion Cible:** Calme + Contrôle (pas de panique ni de frustration)
- **Moment:** Dongle USB déconnecté accidentellement pendant batch de 100 documents
- **Expérience:** Alerte claire, pause automatique (pas d'échec), instructions actionnables
- **Design:** Message: "Dongle déconnecté → Reconnectez le dongle USB et entrez le PIN", reprise automatique

**6. Retour Utilisateur (Fidélité et Advocacy)**
- **Émotion Cible:** Satisfaction profonde + Recommandation enthousiaste
- **Moment:** Fatima recommande l'application à d'autres universités
- **Expérience:** Zéro surprise négative, fiabilité constante, support réactif
- **Design:** Expérience cohérente, updates transparents, feedback utilisateur valorisé

### Micro-Emotions

**Confiance vs. Scepticisme**
- **Cible:** Confiance totale dans la validité légale des signatures électroniques
- **Importance:** Critique - signatures engagent légalement l'université
- **Design:** 
  - Indicateurs de statut certificat Barid Al-Maghrib (valide jusqu'à 2027)
  - Logs détaillés avec correlation IDs pour audit
  - Visualisation signature PAdES visible dans le PDF
  - QR code embedded pour vérification tierce

**Contrôle vs. Impuissance**
- **Cible:** Sentiment de contrôle total du processus de signature
- **Importance:** Élevée - utilisateur doit pouvoir intervenir si nécessaire
- **Design:**
  - Pause/resume batch à tout moment
  - Retry manuel disponible pour documents en erreur
  - Logs visibles en temps réel (pas de processus caché)
  - Sélection granulaire (document par document ou batch)

**Accomplissement vs. Frustration**
- **Cible:** Sentiment d'accomplissement après chaque batch signé
- **Importance:** Élevée - motivation quotidienne de l'utilisateur
- **Design:**
  - Progress bars claires avec pourcentage (50/100 = 50%)
  - Messages de succès encourageants ("Excellent travail! 50 documents signés ✓")
  - Compteurs visuels (Pending: 127 → 77 après batch)
  - Historique des batches complétés

**Sérénité vs. Anxiété**
- **Cible:** Sérénité même pendant batch de 500 documents
- **Importance:** Critique - stress réduit = productivité accrue
- **Design:**
  - Retry automatique avec exponential backoff (erreurs temporaires résolues seules)
  - Dead-letter queue (aucun document perdu, tous récupérables)
  - Recovery gracieux sans perte de travail (pause/resume)
  - Indicateurs de statut temps réel (tout fonctionne = vert)

**Modernité vs. Obsolescence**
- **Cible:** Fierté d'utiliser un outil moderne et professionnel
- **Importance:** Moyenne - valorisation du travail administratif
- **Design:**
  - Dark theme professionnel (#070C14, #0D1520)
  - Animations fluides (progress bars, transitions)
  - UI Material Design avec composants modernes
  - Palette de couleurs sémantiques (vert/orange/rouge/indigo)

### Design Implications

**1. Soulagement & Contrôle → Transparence Totale**
- **Principe:** L'utilisateur doit toujours savoir ce qui se passe et pourquoi
- **Implémentation:**
  - Indicateurs de statut colorés pour tous les services externes (e-Sign API, S3, SIS)
  - Logs de signature en temps réel avec correlation IDs pour traçabilité
  - Pas de "loading..." générique sans explication
  - Messages explicites: "Signature en cours... (Document 5/50, ~30s par document)"
- **Résultat:** Fatima sait toujours exactement où en est le processus

**2. Confiance → Feedback Visuel Immédiat**
- **Principe:** Validation visuelle de la légalité des signatures
- **Implémentation:**
  - Signature PAdES visible dans le viewer PDF (panneau "Après signature")
  - QR code embedded pour vérification tierce instantanée
  - Certificat Barid Al-Maghrib affiché avec date d'expiration
  - Timestamp RFC 3161 visible dans les métadonnées
- **Résultat:** Fatima voit immédiatement que le document est légalement valide

**3. Efficacité → Automation Intelligente**
- **Principe:** Minimiser les interventions manuelles répétitives
- **Implémentation:**
  - Email automatique post-signature avec lien de téléchargement sécurisé
  - Upload S3 automatique après chaque signature réussie
  - Retry automatique sur erreurs temporaires (API timeout, network glitch)
  - Webhook automatique vers SIS pour mise à jour statut
- **Résultat:** Workflow end-to-end sans étapes manuelles supplémentaires

**4. Sérénité → Gestion d'Erreurs Gracieuse**
- **Principe:** Les erreurs ne doivent jamais créer de panique ou de perte de travail
- **Implémentation:**
  - Détection proactive (dongle déconnecté, certificat proche expiration, API down)
  - Messages actionnables en langage clair (pas de jargon technique)
  - Recovery automatique sans perte de travail (batch pause/resume)
  - Dead-letter queue pour échecs persistants (100% récupérables)
- **Résultat:** Erreurs temporaires ne bloquent pas le workflow

**5. Fierté → Design Professionnel Moderne**
- **Principe:** L'interface doit inspirer confiance et valoriser le travail
- **Implémentation:**
  - Dark theme (#070C14, #0D1520) réduit fatigue visuelle
  - Animations fluides (progress bars, transitions de page)
  - Palette de couleurs sémantiques (vert=succès, orange=warning, rouge=erreur, indigo=accent)
  - Typography claire (Consolas pour code, Segoe UI pour texte)
- **Résultat:** Interface premium qui inspire confiance et professionnalisme

### Emotional Design Principles

**1. "Toujours Rassurant, Jamais Anxiogène"**
- Progress bars vertes (couleur de succès, pas rouge stressant)
- Messages positifs: "50 documents signés ✓" (pas "50 documents restants")
- Erreurs présentées comme opportunités de recovery (pas d'échecs définitifs)
- Feedback immédiat sur chaque action (clic bouton → feedback visuel instantané)

**2. "Transparent Mais Pas Technique"**
- Logs visibles mais en langage clair et accessible
- "Certificat expiré → Contactez l'admin IT pour renouvellement" (pas "OCSP validation failed: certificate revoked")
- Correlation IDs disponibles pour debugging mais pas imposés à l'utilisateur
- Détails techniques accessibles via tooltips (optionnel)

**3. "Efficace Sans Être Robotique"**
- Automation intelligente mais avec contrôle utilisateur (pause/resume)
- Messages de succès chaleureux ("Excellent travail! Tous les documents sont signés ✓")
- Animations subtiles qui humanisent l'interface (pas de transitions brutales)
- Feedback sonore discret sur événements importants (batch completed)

**4. "Professionnel Sans Être Froid"**
- Dark theme sérieux mais accueillant (pas noir pur, nuances de bleu foncé)
- Couleurs sémantiques vivantes (vert #10B981, pas gris neutre)
- Feedback immédiat sur chaque action (bouton hover, click states)
- Micro-interactions qui rendent l'interface vivante

**5. "Résilient Sans Être Permissif"**
- Retry automatique sur erreurs temporaires (network timeout, API glitch)
- Mais alertes claires sur problèmes critiques (certificat expiré, dongle manquant)
- Recovery gracieux sans perte de travail (batch pause/resume)
- Traçabilité complète via logs et correlation IDs pour audit
