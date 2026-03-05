# Architecture MVVM - AcadSign Desktop App

## Vue d'ensemble

Cette documentation décrit l'architecture MVVM (Model-View-ViewModel) de l'application Desktop AcadSign, implémentée avec WPF et CommunityToolkit.Mvvm.

## Pattern MVVM

### Principes

**Model-View-ViewModel (MVVM)** est un pattern architectural qui sépare:
- **Model**: Données et logique métier
- **View**: Interface utilisateur (XAML)
- **ViewModel**: Logique de présentation et état de l'UI

### Avantages

- ✅ Séparation des préoccupations
- ✅ Testabilité accrue
- ✅ Réutilisabilité du code
- ✅ Support du data binding
- ✅ Développement parallèle (UI/Logique)

## Structure du Projet

```
AcadSign.Desktop/
├── Views/                      # Vues XAML
│   ├── LoginView.xaml
│   ├── MainView.xaml
│   ├── SigningView.xaml
│   └── SettingsView.xaml
├── ViewModels/                 # ViewModels
│   ├── LoginViewModel.cs
│   ├── MainViewModel.cs
│   ├── SigningViewModel.cs
│   └── SettingsViewModel.cs
├── Models/                     # Modèles de données
│   └── DocumentDto.cs
├── Services/                   # Services
│   ├── Navigation/
│   │   ├── INavigationService.cs
│   │   ├── NavigationService.cs
│   │   └── INavigationAware.cs
│   ├── Authentication/
│   │   ├── IAuthenticationService.cs
│   │   └── AuthenticationService.cs
│   ├── Api/
│   │   ├── IApiClientService.cs
│   │   └── ApiClientService.cs
│   ├── Signature/
│   │   ├── ISignatureService.cs
│   │   └── SignatureService.cs
│   ├── Dongle/
│   │   ├── IDongleService.cs
│   │   └── DongleService.cs
│   └── Storage/
│       ├── ITokenStorageService.cs
│       └── TokenStorageService.cs
├── Resources/                  # Ressources multilingues
│   ├── Strings.resx           # Français
│   └── Strings.ar.resx        # Arabe
├── Properties/
│   ├── Settings.settings      # Configuration
│   └── Settings.Designer.cs
└── App.xaml.cs                # Point d'entrée + DI
```

## CommunityToolkit.Mvvm

### ObservableProperty

Génère automatiquement les propriétés avec `INotifyPropertyChanged`:

```csharp
[ObservableProperty]
private string _statusMessage = string.Empty;

// Génère automatiquement:
public string StatusMessage
{
    get => _statusMessage;
    set => SetProperty(ref _statusMessage, value);
}
```

### RelayCommand

Génère automatiquement les commandes:

```csharp
[RelayCommand]
private async Task LoginAsync()
{
    // Logique de connexion
}

// Génère automatiquement:
public IAsyncRelayCommand LoginCommand { get; }
```

### ObservableObject

Classe de base pour les ViewModels:

```csharp
public partial class LoginViewModel : ObservableObject
{
    // Implémente INotifyPropertyChanged automatiquement
}
```

## ViewModels

### LoginViewModel

**Responsabilités:**
- Gestion de l'authentification OAuth 2.0
- Navigation vers MainView après connexion
- Affichage des messages d'état

**Propriétés:**
- `IsLoggingIn`: Indicateur de connexion en cours
- `StatusMessage`: Message d'état/erreur

**Commandes:**
- `LoginCommand`: Déclenche l'authentification

### MainViewModel

**Responsabilités:**
- Affichage de la liste des documents
- Rafraîchissement des données
- Navigation vers SigningView
- Navigation vers SettingsView

**Propriétés:**
- `Documents`: Collection de documents à signer
- `SelectedDocument`: Document sélectionné
- `IsLoading`: Indicateur de chargement
- `StatusText`: Texte de statut

**Commandes:**
- `LoadDocumentsCommand`: Charge les documents
- `SignDocumentCommand`: Navigue vers signature
- `NavigateToSettingsCommand`: Ouvre les paramètres
- `LogoutCommand`: Déconnexion

### SigningViewModel

**Responsabilités:**
- Gestion du processus de signature
- Affichage de la progression
- Communication avec le dongle USB

**Propriétés:**
- `DocumentName`: Nom du document
- `StatusMessage`: Message d'état
- `Progress`: Pourcentage de progression (0-100)
- `ProgressText`: Texte de progression
- `CanCancel`: Possibilité d'annuler
- `IsCompleted`: Signature terminée

**Commandes:**
- `CancelCommand`: Annule la signature
- `GoBackCommand`: Retour au dashboard

**Interface:**
- Implémente `INavigationAware` pour recevoir le document

### SettingsViewModel

**Responsabilités:**
- Configuration de l'application
- Gestion des paramètres utilisateur
- Support multilingue

**Propriétés:**
- `ApiEndpoint`: URL de l'API Backend
- `AutoDetectDongle`: Détection automatique
- `SelectedLanguage`: Langue sélectionnée
- `AvailableLanguages`: Langues disponibles
- `AppVersion`: Version de l'application
- `CurrentUser`: Utilisateur connecté

**Commandes:**
- `SaveCommand`: Sauvegarde les paramètres
- `CancelCommand`: Annule les modifications

## Services

### INavigationService

**Responsabilité:** Navigation entre les vues

**Méthodes:**
```csharp
void NavigateTo<TViewModel>(object? parameter = null);
void GoBack();
```

**Utilisation:**
```csharp
_navigationService.NavigateTo<SigningViewModel>(document);
```

**Implémentation:**
- Résolution des ViewModels via DI
- Mapping ViewModel → View par convention
- Support des paramètres de navigation
- Gestion du cycle de vie des fenêtres

### IAuthenticationService

**Responsabilité:** Authentification OAuth 2.0

**Méthodes:**
```csharp
Task<AuthenticationResult> LoginAsync();
Task LogoutAsync();
Task<string> RefreshTokenAsync(string refreshToken);
```

### IApiClientService

**Responsabilité:** Communication avec le Backend API

**Méthodes:**
```csharp
Task<List<DocumentDto>> GetPendingDocumentsAsync();
Task<byte[]> DownloadDocumentAsync(Guid documentId);
Task UploadSignedDocumentAsync(Guid documentId, byte[] signedData);
```

### ISignatureService

**Responsabilité:** Signature électronique PAdES

**Méthodes:**
```csharp
Task<byte[]> SignDocumentAsync(Guid documentId);
Task<bool> ValidateSignatureAsync(byte[] signedDocument);
```

### IDongleService

**Responsabilité:** Gestion du dongle USB

**Méthodes:**
```csharp
Task<bool> DetectDongleAsync();
Task<string> GetCertificateAsync();
Task<bool> ValidatePinAsync(string pin);
```

### ITokenStorageService

**Responsabilité:** Stockage sécurisé des tokens

**Méthodes:**
```csharp
Task SaveTokensAsync(string accessToken, string refreshToken);
Task<(string AccessToken, string RefreshToken)> GetTokensAsync();
Task ClearTokensAsync();
```

## Dependency Injection

### Configuration (App.xaml.cs)

```csharp
private void ConfigureServices(IServiceCollection services)
{
    // Services - Singleton
    services.AddSingleton<INavigationService, NavigationService>();
    services.AddSingleton<ITokenStorageService, TokenStorageService>();
    services.AddSingleton<IAuthenticationService, AuthenticationService>();
    services.AddSingleton<IApiClientService, ApiClientService>();
    services.AddSingleton<ISignatureService, SignatureService>();
    services.AddSingleton<IDongleService, DongleService>();
    
    // ViewModels - Transient
    services.AddTransient<LoginViewModel>();
    services.AddTransient<MainViewModel>();
    services.AddTransient<SigningViewModel>();
    services.AddTransient<SettingsViewModel>();
}
```

### Injection dans les ViewModels

```csharp
public LoginViewModel(
    IAuthenticationService authService,
    INavigationService navigationService,
    ITokenStorageService tokenStorage)
{
    _authService = authService;
    _navigationService = navigationService;
    _tokenStorage = tokenStorage;
}
```

## Data Binding

### Binding depuis XAML

```xml
<!-- Propriété -->
<TextBlock Text="{Binding StatusMessage}"/>

<!-- Commande -->
<Button Command="{Binding LoginCommand}" Content="Se connecter"/>

<!-- Collection -->
<DataGrid ItemsSource="{Binding Documents}"/>

<!-- Two-Way Binding -->
<TextBox Text="{Binding ApiEndpoint, UpdateSourceTrigger=PropertyChanged}"/>
```

### Binding avec Converter

```xml
<ProgressBar Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisibilityConverter}}"/>
```

## Navigation

### Navigation Simple

```csharp
_navigationService.NavigateTo<MainViewModel>();
```

### Navigation avec Paramètre

```csharp
_navigationService.NavigateTo<SigningViewModel>(document);
```

### Réception du Paramètre

```csharp
public class SigningViewModel : ObservableObject, INavigationAware
{
    public void OnNavigatedTo(object parameter)
    {
        if (parameter is DocumentDto document)
        {
            _document = document;
            DocumentName = document.StudentName;
        }
    }
}
```

## Support Multilingue

### Fichiers de Ressources

- `Resources/Strings.resx` - Français (défaut)
- `Resources/Strings.ar.resx` - Arabe

### Utilisation dans XAML

```xml
<TextBlock Text="{x:Static properties:Strings.Login_Title}"/>
```

### Changement de Langue

```csharp
Thread.CurrentThread.CurrentUICulture = new CultureInfo("ar");
```

## Configuration

### Settings.settings

Stockage des paramètres utilisateur:
- `ApiEndpoint`: URL de l'API
- `AutoDetectDongle`: Détection automatique
- `Language`: Langue de l'interface

### Accès aux Settings

```csharp
var apiUrl = Properties.Settings.Default.ApiEndpoint;
Properties.Settings.Default.Language = "Français";
Properties.Settings.Default.Save();
```

## Flux de Données

### 1. Connexion

```
LoginView → LoginViewModel → AuthenticationService
    ↓
TokenStorageService (sauvegarde tokens)
    ↓
NavigationService → MainView
```

### 2. Chargement Documents

```
MainView → MainViewModel → ApiClientService
    ↓
Documents (ObservableCollection)
    ↓
DataGrid (binding automatique)
```

### 3. Signature

```
MainView (clic "Signer") → MainViewModel.SignDocumentCommand
    ↓
NavigationService → SigningView (avec DocumentDto)
    ↓
SigningViewModel → SignatureService + DongleService
    ↓
ApiClientService (upload document signé)
    ↓
NavigationService → MainView
```

## Best Practices

### ViewModels

✅ **À faire:**
- Utiliser `[ObservableProperty]` pour les propriétés
- Utiliser `[RelayCommand]` pour les commandes
- Injecter les dépendances via constructeur
- Gérer les erreurs avec try/catch
- Afficher les messages d'état à l'utilisateur

❌ **À éviter:**
- Référencer directement les Views
- Logique métier dans les ViewModels
- Appels synchrones bloquants
- Propriétés sans notification

### Services

✅ **À faire:**
- Définir des interfaces
- Implémenter async/await
- Gérer les timeouts
- Logger les opérations
- Retourner des DTOs

❌ **À éviter:**
- Dépendances circulaires
- État mutable partagé
- Exceptions non gérées

### Views

✅ **À faire:**
- Utiliser le data binding
- Définir DataContext via DI
- Utiliser des Converters pour la logique UI
- Respecter les guidelines UX

❌ **À éviter:**
- Code-behind complexe
- Logique métier dans XAML
- Manipulation directe des données

## Testing

### Unit Tests ViewModels

```csharp
[Test]
public async Task LoginAsync_ValidCredentials_NavigatesToMain()
{
    // Arrange
    var authService = new Mock<IAuthenticationService>();
    var navigationService = new Mock<INavigationService>();
    var viewModel = new LoginViewModel(authService.Object, navigationService.Object);
    
    // Act
    await viewModel.LoginCommand.ExecuteAsync(null);
    
    // Assert
    navigationService.Verify(x => x.NavigateTo<MainViewModel>(null), Times.Once);
}
```

## Évolutions Futures

- **Refit**: Client HTTP typé pour l'API
- **PKCS#11**: Intégration dongle USB réelle
- **iText 7**: Signature PAdES complète
- **Validation**: FluentValidation pour les formulaires
- **Caching**: Cache local des documents
- **Offline Mode**: Mode hors ligne

## Références

- **CommunityToolkit.Mvvm**: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/
- **WPF MVVM**: https://learn.microsoft.com/en-us/dotnet/desktop/wpf/
- **Dependency Injection**: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
