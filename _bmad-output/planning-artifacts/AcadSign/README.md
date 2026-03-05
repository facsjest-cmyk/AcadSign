# AcadSign — Système de Signature Électronique Académique

Application WPF .NET 8 pour la génération et la signature électronique de
documents académiques via **Barid Al-Maghrib e-Sign**, avec stockage S3 et envoi email.

---

## 🏗 Architecture

```
AcadSign/
├── Models/
│   └── Models.cs               — Student, DocumentRequest, Settings, DTOs
├── ViewModels/
│   └── MainViewModel.cs        — MVVM central (CommunityToolkit.Mvvm)
├── Views/
│   ├── MainWindow.xaml         — Interface principale (3 panneaux)
│   ├── MainWindow.xaml.cs      — Code-behind (chrome, PIN bridge)
│   └── PdfDocumentView.xaml    — Rendu inline du document
├── Services/
│   └── Services.cs             — ISisApiService, IESignService,
│                                  IS3StorageService, IEmailService,
│                                  IPdfGeneratorService, IPdfViewerService
├── Converters/
│   └── Converters.cs           — IValueConverter pour le binding XAML
├── Resources/
│   ├── Colors.xaml             — Palette AcadSign (dark theme)
│   └── Styles.xaml             — Tous les styles WPF
└── App.xaml / App.xaml.cs      — DI container (Microsoft.Extensions.DI)
```

---

## 📦 Dépendances NuGet

| Package                          | Rôle                                 |
|----------------------------------|--------------------------------------|
| CommunityToolkit.Mvvm 8.3        | MVVM, ObservableObject, RelayCommand |
| Microsoft.Extensions.DI 8.0      | Injection de dépendances             |
| itext7 + bouncy-castle-adapter   | Signature PAdES, embedding dans PDF  |
| QuestPDF 2024.3                  | Génération de PDF depuis templates   |
| AWSSDK.S3                        | Upload S3 compatible (MinIO, OVH...) |
| MailKit 4.7                      | Envoi SMTP (TLS, OAuth2)             |
| PdfiumViewer 2.13                | Rendu PDF dans le viewer WPF         |
| MaterialDesignThemes 5.1         | Composants UI supplémentaires        |
| Newtonsoft.Json 13               | Sérialisation JSON API               |

---

## ⚙️ Configuration

Créez `appsettings.json` (ou utilisez des variables d'environnement) :

```json
{
  "ESign": {
    "BaseUrl": "https://esign.barid.ma/api/v1",
    "CertificatePath": "C:\\certs\\uh2-sign.p12",
    "CertificateSerial": "UH2-SIGN-2024",
    "ApiKey": "YOUR_BARID_API_KEY"
  },
  "S3": {
    "Endpoint": "https://s3.uh2.ac.ma",
    "AccessKey": "YOUR_ACCESS_KEY",
    "SecretKey": "YOUR_SECRET_KEY",
    "BucketName": "uh2-docs-signed",
    "UsePathStyle": true
  },
  "Email": {
    "SmtpHost": "smtp.uh2.ac.ma",
    "SmtpPort": 587,
    "Username": "scolarite@uh2.ac.ma",
    "Password": "YOUR_SMTP_PASSWORD",
    "FromAddress": "scolarite@uh2.ac.ma",
    "FromName": "Service de Scolarité UH2"
  },
  "SisApi": {
    "BaseUrl": "https://sis.uh2.ac.ma/api/",
    "ApiKey": "YOUR_SIS_API_KEY",
    "InstitutionCode": "UH2"
  }
}
```

---

## 🔏 Intégration Barid Al-Maghrib e-Sign

Le service `ESignService` implémente le flux complet :

1. **Authentification** — POST `/auth/session` avec le serial du certificat + PIN
2. **Hash SHA-256** du document PDF
3. **Signature HSM** — POST `/sign/pades` avec format PAdES-B-LT
4. **Horodatage RFC 3161** — POST `/timestamp/rfc3161`
5. **Embedding PAdES** dans le PDF via iText7 `PdfSigner`

> **Note** : Remplacez les endpoints par ceux fournis par Barid Al-Maghrib dans
> votre contrat de service. Demandez la documentation technique à :
> `esign-support@barid.ma`

---

## 📄 Génération PDF — QuestPDF

Remplacez `PdfGeneratorService.GenerateMockPdf()` par des templates QuestPDF :

```csharp
var doc = Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.A4);
        page.Margin(40);
        page.Header().Element(ComposeHeader);
        page.Content().Element(c => ComposeContent(c, request));
    });
});
return doc.GeneratePdf();
```

---

## ☁️ Stockage S3

Compatible avec :
- **MinIO** (on-premise) → `UsePathStyle = true`
- **AWS S3** → `UsePathStyle = false`
- **OVH Object Storage**
- **Wasabi**

Structure des clés : `docs/{année}/{type}/{studentId}/{docId}.pdf`

---

## 🔑 Commandes ViewModel (RelayCommand)

| Commande                    | Action                                          |
|-----------------------------|-------------------------------------------------|
| `FetchFromApiCommand`       | Récupère les demandes depuis le SIS via API     |
| `SignDocumentCommand`       | Signe le document sélectionné                   |
| `OpenBatchSignDialogCommand`| Ouvre la dialog de signature par lot            |
| `StartBatchSignCommand`     | Lance la signature de tous les éléments cochés  |
| `SendEmailCommand`          | Envoie le document signé par email              |
| `ShowBeforeSignatureCommand`| Affiche le PDF avant signature                  |
| `ShowAfterSignatureCommand` | Affiche le PDF après signature                  |
| `SelectAllPendingCommand`   | Coche tous les documents en attente             |
| `ZoomInCommand`             | Zoom avant sur le viewer PDF                    |
| `ZoomOutCommand`            | Zoom arrière sur le viewer PDF                  |

---

## 🚀 Lancement

```bash
dotnet restore
dotnet build
dotnet run
```

---

## 📋 Conformité

- **Loi marocaine n° 43-20** — Services de confiance numérique
- **Loi marocaine n° 53-05** — Protection des données personnelles
- **PAdES-B-LT** — PDF Advanced Electronic Signatures (ETSI EN 319 132)
- **RFC 3161** — Horodatage cryptographique
- **eIDAS** compatible (reconnaissance internationale)
