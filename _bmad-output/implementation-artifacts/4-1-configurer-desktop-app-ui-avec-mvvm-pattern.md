# Story 4.1: Configurer Desktop App UI avec MVVM Pattern

Status: done

## Story

As a **Fatima (registrar staff)**,
I want **une interface Desktop App intuitive pour signer des documents**,
So that **je peux facilement gérer mes tâches de signature quotidiennes**.

## Acceptance Criteria

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

## Tasks / Subtasks

- [x] Créer la structure Views (AC: Views créées)
  - [x] LoginView.xaml créée avec UI moderne
  - [x] MainView.xaml créée avec DataGrid
  - [x] SigningView.xaml créée avec ProgressBar
  - [x] SettingsView.xaml créée avec configuration
  
- [x] Créer les ViewModels (AC: ViewModels créés)
  - [x] LoginViewModel avec CommunityToolkit.Mvvm
  - [x] MainViewModel avec ObservableCollection
  - [x] SigningViewModel avec INavigationAware
  - [x] SettingsViewModel avec Settings binding
  
- [x] Créer les interfaces Services (AC: Services créés)
  - [x] IAuthenticationService + implémentation mock
  - [x] IApiClientService + implémentation mock
  - [x] ISignatureService + implémentation mock
  - [x] IDongleService + implémentation mock
  - [x] ITokenStorageService (déjà existant)
  
- [x] Implémenter NavigationService (AC: Navigation fonctionnelle)
  - [x] INavigationService créée
  - [x] NavigationService implémentée avec mapping ViewModel→View
  - [x] INavigationAware pour paramètres de navigation
  - [x] Enregistré dans DI comme Singleton
  
- [x] Configurer les ressources multilingues (AC: FR/AR support)
  - [x] Resources/Strings.resx (français)
  - [x] Resources/Strings.ar.resx (arabe)
  - [x] Support NFR-U1 implémenté
  
- [x] Configurer DI dans App.xaml.cs (AC: DI configuré)
  - [x] ServiceCollection configurée
  - [x] Tous les services enregistrés (Singleton)
  - [x] Tous les ViewModels enregistrés (Transient)
  - [x] Navigation démarre avec LoginView

## Dev Notes

### Contexte

Cette story configure l'architecture MVVM complète de la Desktop App avec CommunityToolkit.Mvvm, navigation, et support multilingue.

**Epic 4: Electronic Signature (Desktop App)** - Story 1/6

### Structure du Projet

```
AcadSign.Desktop/
├── Views/
│   ├── LoginView.xaml
│   ├── MainView.xaml
│   ├── SigningView.xaml
│   └── SettingsView.xaml
├── ViewModels/
│   ├── LoginViewModel.cs
│   ├── MainViewModel.cs
│   ├── SigningViewModel.cs
│   └── SettingsViewModel.cs
├── Services/
│   ├── Authentication/
│   │   └── IAuthenticationService.cs
│   ├── Api/
│   │   └── IApiClientService.cs
│   ├── Signature/
│   │   └── ISignatureService.cs
│   ├── Dongle/
│   │   └── IDongleService.cs
│   ├── Navigation/
│   │   └── INavigationService.cs
│   └── Storage/
│       └── ITokenStorageService.cs
├── Models/
│   └── DocumentDto.cs
├── Resources/
│   ├── Resources.resx
│   └── Resources.ar.resx
└── App.xaml.cs
```

### LoginView.xaml

**Fichier: `AcadSign.Desktop/Views/LoginView.xaml`**

```xml
<Window x:Class="AcadSign.Desktop.Views.LoginView"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="clr-namespace:AcadSign.Desktop.ViewModels"
        Title="AcadSign - Connexion" 
        Height="500" Width="400"
        WindowStartupLocation="CenterScreen"
        ResizeMode="NoResize">
    
    <Window.DataContext>
        <vm:LoginViewModel />
    </Window.DataContext>
    
    <Grid Background="#F5F5F5">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <!-- Header -->
        <StackPanel Grid.Row="0" Background="#2196F3" Padding="20">
            <TextBlock Text="AcadSign" 
                       FontSize="32" 
                       FontWeight="Bold" 
                       Foreground="White"
                       HorizontalAlignment="Center"/>
            <TextBlock Text="Signature Électronique Académique" 
                       FontSize="14" 
                       Foreground="White"
                       HorizontalAlignment="Center"
                       Margin="0,5,0,0"/>
        </StackPanel>
        
        <!-- Content -->
        <StackPanel Grid.Row="1" 
                    Margin="40"
                    VerticalAlignment="Center">
            
            <TextBlock Text="Bienvenue" 
                       FontSize="24" 
                       FontWeight="SemiBold"
                       Margin="0,0,0,20"/>
            
            <TextBlock Text="Connectez-vous avec votre compte universitaire" 
                       FontSize="14" 
                       Foreground="#666"
                       TextWrapping="Wrap"
                       Margin="0,0,0,30"/>
            
            <Button Content="Se connecter avec OAuth 2.0"
                    Command="{Binding LoginCommand}"
                    Height="45"
                    FontSize="16"
                    Background="#2196F3"
                    Foreground="White"
                    BorderThickness="0"
                    Cursor="Hand"
                    IsEnabled="{Binding IsNotLoggingIn}">
                <Button.Style>
                    <Style TargetType="Button">
                        <Setter Property="Template">
                            <Setter.Value>
                                <ControlTemplate TargetType="Button">
                                    <Border Background="{TemplateBinding Background}"
                                            CornerRadius="5"
                                            Padding="10">
                                        <ContentPresenter HorizontalAlignment="Center"
                                                         VerticalAlignment="Center"/>
                                    </Border>
                                </ControlTemplate>
                            </Setter.Value>
                        </Setter>
                    </Style>
                </Button.Style>
            </Button>
            
            <!-- Status Message -->
            <TextBlock Text="{Binding StatusMessage}"
                       Foreground="#F44336"
                       FontSize="12"
                       Margin="0,10,0,0"
                       TextWrapping="Wrap"
                       HorizontalAlignment="Center"/>
            
            <!-- Loading Indicator -->
            <ProgressBar IsIndeterminate="True"
                         Height="4"
                         Margin="0,20,0,0"
                         Visibility="{Binding IsLoggingIn, Converter={StaticResource BoolToVisibilityConverter}}"/>
        </StackPanel>
        
        <!-- Footer -->
        <TextBlock Grid.Row="2" 
                   Text="© 2026 Université Hassan II - Casablanca"
                   FontSize="12"
                   Foreground="#999"
                   HorizontalAlignment="Center"
                   Margin="0,0,0,20"/>
    </Grid>
</Window>
```

### LoginViewModel.cs

**Fichier: `AcadSign.Desktop/ViewModels/LoginViewModel.cs`**

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AcadSign.Desktop.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;
    private readonly ITokenStorageService _tokenStorage;
    
    [ObservableProperty]
    private bool _isLoggingIn;
    
    [ObservableProperty]
    private string _statusMessage;
    
    public bool IsNotLoggingIn => !IsLoggingIn;
    
    public LoginViewModel(
        IAuthenticationService authService,
        INavigationService navigationService,
        ITokenStorageService tokenStorage)
    {
        _authService = authService;
        _navigationService = navigationService;
        _tokenStorage = tokenStorage;
    }
    
    [RelayCommand]
    private async Task LoginAsync()
    {
        try
        {
            IsLoggingIn = true;
            StatusMessage = "Connexion en cours...";
            
            // Authentifier via OAuth 2.0 Authorization Code + PKCE
            var result = await _authService.LoginAsync();
            
            // Sauvegarder les tokens
            await _tokenStorage.SaveTokensAsync(result.AccessToken, result.RefreshToken);
            
            StatusMessage = "Connexion réussie!";
            
            // Naviguer vers MainView
            _navigationService.NavigateTo<MainViewModel>();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsLoggingIn = false;
            OnPropertyChanged(nameof(IsNotLoggingIn));
        }
    }
}
```

### MainView.xaml

**Fichier: `AcadSign.Desktop/Views/MainView.xaml`**

```xml
<Window x:Class="AcadSign.Desktop.Views.MainView"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="AcadSign - Documents à Signer" 
        Height="600" Width="900"
        WindowStartupLocation="CenterScreen">
    
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <!-- Toolbar -->
        <StackPanel Grid.Row="0" 
                    Background="#2196F3" 
                    Orientation="Horizontal"
                    Padding="10">
            <Button Content="Rafraîchir"
                    Command="{Binding LoadDocumentsCommand}"
                    Margin="5"/>
            <Button Content="Paramètres"
                    Command="{Binding NavigateToSettingsCommand}"
                    Margin="5"/>
            <Button Content="Déconnexion"
                    Command="{Binding LogoutCommand}"
                    Margin="5"
                    HorizontalAlignment="Right"/>
        </StackPanel>
        
        <!-- Documents List -->
        <DataGrid Grid.Row="1"
                  ItemsSource="{Binding Documents}"
                  SelectedItem="{Binding SelectedDocument}"
                  AutoGenerateColumns="False"
                  IsReadOnly="True"
                  SelectionMode="Single"
                  Margin="10">
            <DataGrid.Columns>
                <DataGridTextColumn Header="Étudiant" 
                                    Binding="{Binding StudentName}" 
                                    Width="*"/>
                <DataGridTextColumn Header="Type Document" 
                                    Binding="{Binding DocumentType}" 
                                    Width="200"/>
                <DataGridTextColumn Header="Date Création" 
                                    Binding="{Binding CreatedAt, StringFormat=dd/MM/yyyy}" 
                                    Width="120"/>
                <DataGridTextColumn Header="Statut" 
                                    Binding="{Binding Status}" 
                                    Width="100"/>
                <DataGridTemplateColumn Header="Actions" Width="150">
                    <DataGridTemplateColumn.CellTemplate>
                        <DataTemplate>
                            <Button Content="Signer"
                                    Command="{Binding DataContext.SignDocumentCommand, 
                                             RelativeSource={RelativeSource AncestorType=DataGrid}}"
                                    CommandParameter="{Binding}"
                                    Background="#4CAF50"
                                    Foreground="White"
                                    Padding="10,5"/>
                        </DataTemplate>
                    </DataGridTemplateColumn.CellTemplate>
                </DataGridTemplateColumn>
            </DataGrid.Columns>
        </DataGrid>
        
        <!-- Status Bar -->
        <StatusBar Grid.Row="2">
            <StatusBarItem>
                <TextBlock Text="{Binding StatusText}"/>
            </StatusBarItem>
            <StatusBarItem HorizontalAlignment="Right">
                <StackPanel Orientation="Horizontal">
                    <TextBlock Text="Documents: "/>
                    <TextBlock Text="{Binding Documents.Count}"/>
                </StackPanel>
            </StatusBarItem>
        </StatusBar>
    </Grid>
</Window>
```

### MainViewModel.cs

**Fichier: `AcadSign.Desktop/ViewModels/MainViewModel.cs`**

```csharp
public partial class MainViewModel : ObservableObject
{
    private readonly IApiClientService _apiClient;
    private readonly INavigationService _navigationService;
    
    [ObservableProperty]
    private ObservableCollection<DocumentDto> _documents;
    
    [ObservableProperty]
    private DocumentDto _selectedDocument;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _statusText;
    
    public MainViewModel(
        IApiClientService apiClient,
        INavigationService navigationService)
    {
        _apiClient = apiClient;
        _navigationService = navigationService;
        Documents = new ObservableCollection<DocumentDto>();
        
        // Charger les documents au démarrage
        _ = LoadDocumentsAsync();
    }
    
    [RelayCommand]
    private async Task LoadDocumentsAsync()
    {
        try
        {
            IsLoading = true;
            StatusText = "Chargement des documents...";
            
            var docs = await _apiClient.GetPendingDocumentsAsync();
            Documents.Clear();
            foreach (var doc in docs)
            {
                Documents.Add(doc);
            }
            
            StatusText = $"{Documents.Count} document(s) en attente";
        }
        catch (Exception ex)
        {
            StatusText = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async Task SignDocumentAsync(DocumentDto document)
    {
        _navigationService.NavigateTo<SigningViewModel>(document);
    }
    
    [RelayCommand]
    private void NavigateToSettings()
    {
        _navigationService.NavigateTo<SettingsViewModel>();
    }
    
    [RelayCommand]
    private async Task LogoutAsync()
    {
        // Implémenter la déconnexion
        _navigationService.NavigateTo<LoginViewModel>();
    }
}
```

### NavigationService

**Fichier: `AcadSign.Desktop/Services/Navigation/INavigationService.cs`**

```csharp
public interface INavigationService
{
    void NavigateTo<TViewModel>(object parameter = null) where TViewModel : class;
    void GoBack();
}
```

**Implémentation:**

```csharp
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private Window _currentWindow;
    
    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public void NavigateTo<TViewModel>(object parameter = null) where TViewModel : class
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        
        // Mapper ViewModel vers View
        var viewType = GetViewType(typeof(TViewModel));
        var view = (Window)Activator.CreateInstance(viewType);
        view.DataContext = viewModel;
        
        // Passer le paramètre si nécessaire
        if (parameter != null && viewModel is INavigationAware aware)
        {
            aware.OnNavigatedTo(parameter);
        }
        
        // Fermer la fenêtre actuelle et ouvrir la nouvelle
        _currentWindow?.Close();
        _currentWindow = view;
        view.Show();
    }
    
    private Type GetViewType(Type viewModelType)
    {
        var viewName = viewModelType.Name.Replace("ViewModel", "View");
        var viewTypeName = $"AcadSign.Desktop.Views.{viewName}";
        return Type.GetType(viewTypeName);
    }
    
    public void GoBack()
    {
        // Implémenter si nécessaire
    }
}
```

### Configuration DI

**Fichier: `AcadSign.Desktop/App.xaml.cs`**

```csharp
public partial class App : Application
{
    private IServiceProvider _serviceProvider;
    
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
        
        // Démarrer avec LoginView ou MainView selon les tokens
        var startupService = _serviceProvider.GetRequiredService<IStartupService>();
        startupService.InitializeAsync().Wait();
    }
    
    private void ConfigureServices(IServiceCollection services)
    {
        // Services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<ITokenStorageService, TokenStorageService>();
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<IApiClientService, ApiClientService>();
        services.AddSingleton<ISignatureService, SignatureService>();
        services.AddSingleton<IDongleService, DongleService>();
        
        // ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<SigningViewModel>();
        services.AddTransient<SettingsViewModel>();
        
        // Startup
        services.AddSingleton<IStartupService, StartupService>();
    }
}
```

### Ressources Multilingues

**Fichier: `AcadSign.Desktop/Resources/Resources.resx`**

```xml
<data name="Login_Title" xml:space="preserve">
    <value>Connexion</value>
</data>
<data name="Login_Button" xml:space="preserve">
    <value>Se connecter</value>
</data>
<data name="Documents_Title" xml:space="preserve">
    <value>Documents à Signer</value>
</data>
```

**Fichier: `AcadSign.Desktop/Resources/Resources.ar.resx`**

```xml
<data name="Login_Title" xml:space="preserve">
    <value>تسجيل الدخول</value>
</data>
<data name="Login_Button" xml:space="preserve">
    <value>تسجيل الدخول</value>
</data>
<data name="Documents_Title" xml:space="preserve">
    <value>المستندات المراد توقيعها</value>
</data>
```

### Références Architecturales

**Source: Epics Document**
- Epic 4: Electronic Signature (Desktop App)
- Story 4.1: Configurer Desktop App UI avec MVVM
- Fichier: `_bmad-output/planning-artifacts/epics.md:1327-1390`

### Critères de Complétion

✅ Views créées (Login, Main, Signing, Settings)
✅ ViewModels créés avec CommunityToolkit.Mvvm
✅ Services interfaces créés
✅ NavigationService implémenté
✅ DI configuré dans App.xaml.cs
✅ Support multilingue FR/AR (NFR-U1)
✅ XAML Hot Reload fonctionnel
✅ Binding MVVM avec [ObservableProperty] et [RelayCommand]

## Dev Agent Record

### Agent Model Used

Cascade AI (Claude 3.7 Sonnet)

### Debug Log References

Aucun problème de compilation rencontré. L'implémentation s'est déroulée sans erreur.

### Completion Notes List

✅ **Views XAML Créées**

**LoginView.xaml:**
- Design moderne avec header bleu (#2196F3)
- Bouton OAuth 2.0 avec style arrondi
- ProgressBar pour indicateur de chargement
- Message de statut/erreur
- Footer avec copyright

**MainView.xaml:**
- Toolbar avec boutons Rafraîchir, Paramètres, Déconnexion
- DataGrid pour liste des documents
- Colonnes: Étudiant, Type, Date, Statut, Actions
- Bouton "Signer" par ligne
- StatusBar avec compteur de documents

**SigningView.xaml:**
- Header avec titre "Signature en cours"
- Affichage du nom du document
- ProgressBar avec pourcentage
- Boutons Annuler/Retour selon l'état
- Message d'avertissement

**SettingsView.xaml:**
- Configuration API (URL endpoint)
- Configuration Dongle (auto-détection, PIN)
- Sélection de langue (FR/AR)
- Informations (version, utilisateur)
- Boutons Enregistrer/Annuler

✅ **ViewModels avec CommunityToolkit.Mvvm**

**LoginViewModel:**
- `[ObservableProperty]` pour IsLoggingIn, StatusMessage
- `[RelayCommand]` pour LoginAsync
- Injection: IAuthenticationService, INavigationService, ITokenStorageService
- Gestion OAuth 2.0 flow
- Navigation vers MainView après succès

**MainViewModel:**
- `ObservableCollection<DocumentDto>` pour Documents
- `[RelayCommand]` pour LoadDocuments, SignDocument, NavigateToSettings, Logout
- Injection: IApiClientService, INavigationService
- Chargement automatique au démarrage
- Gestion des erreurs avec StatusText

**SigningViewModel:**
- Implémente `INavigationAware` pour recevoir DocumentDto
- Simulation du processus de signature (10% → 100%)
- Étapes: Détection dongle → Certificat → Download → Signature → Upload
- Progress tracking avec ProgressBar
- États: CanCancel, IsCompleted

**SettingsViewModel:**
- Binding avec Properties.Settings.Default
- ObservableCollection pour AvailableLanguages
- Sauvegarde/Chargement des paramètres
- Support multilingue (Français, العربية)

✅ **Services Interfaces et Implémentations**

**INavigationService + NavigationService:**
- Méthode: `NavigateTo<TViewModel>(object? parameter)`
- Mapping automatique ViewModel → View par convention
- Support INavigationAware pour paramètres
- Gestion du cycle de vie des fenêtres
- Fermeture automatique de la fenêtre précédente

**IAuthenticationService + AuthenticationService:**
- Mock implementation pour Story 4.1
- Méthodes: LoginAsync, LogoutAsync, RefreshTokenAsync
- Retourne AuthenticationResult (AccessToken, RefreshToken, ExpiresAt)
- À remplacer par OAuth 2.0 réel dans Story 4.5

**IApiClientService + ApiClientService:**
- Mock implementation avec données de test
- Méthodes: GetPendingDocumentsAsync, DownloadDocumentAsync, UploadSignedDocumentAsync
- Retourne 2 documents de test (Ahmed, Fatima)
- À remplacer par Refit dans Story 4.5

**ISignatureService + SignatureService:**
- Mock implementation
- Méthodes: SignDocumentAsync, ValidateSignatureAsync
- Simulation avec delay de 2 secondes
- À remplacer par iText 7 PAdES dans Story 4.3

**IDongleService + DongleService:**
- Mock implementation
- Méthodes: DetectDongleAsync, GetCertificateAsync, ValidatePinAsync
- À remplacer par PKCS#11 dans Story 4.2

**ITokenStorageService:**
- Déjà existant (Story 2.6)
- Utilise Windows Credential Manager

✅ **Models (DTOs)**

**DocumentDto:**
- Id (Guid)
- StudentName (string)
- DocumentType (string)
- CreatedAt (DateTime)
- Status (string)

✅ **Navigation System**

**Convention de Mapping:**
- LoginViewModel → LoginView
- MainViewModel → MainView
- SigningViewModel → SigningView
- SettingsViewModel → SettingsView

**Flow de Navigation:**
1. App démarre → LoginView
2. Login réussi → MainView
3. Clic "Signer" → SigningView (avec DocumentDto)
4. Signature terminée → MainView
5. Clic "Paramètres" → SettingsView

✅ **Dependency Injection**

**Services (Singleton):**
- INavigationService
- ITokenStorageService
- IAuthenticationService
- IApiClientService
- ISignatureService
- IDongleService

**ViewModels (Transient):**
- LoginViewModel
- MainViewModel
- SigningViewModel
- SettingsViewModel

**Configuration:**
- App.xaml.cs avec ConfigureServices
- Injection via constructeur dans ViewModels
- Résolution automatique par IServiceProvider

✅ **Support Multilingue (NFR-U1)**

**Fichiers de Ressources:**
- Resources/Strings.resx (Français)
- Resources/Strings.ar.resx (العربية)

**Chaînes Traduites:**
- Login_Title, Login_Button, Login_Welcome
- Documents_Title, Documents_Refresh, Documents_Settings, Documents_Logout
- Signing_Title
- Settings_Title

**Utilisation:**
```xml
<TextBlock Text="{x:Static properties:Strings.Login_Title}"/>
```

✅ **Configuration Application**

**Properties/Settings.settings:**
- ApiEndpoint (string) - Default: "http://localhost:5000"
- AutoDetectDongle (bool) - Default: true
- Language (string) - Default: "Français"

**Accès:**
```csharp
var apiUrl = Properties.Settings.Default.ApiEndpoint;
Properties.Settings.Default.Save();
```

✅ **Documentation Complète**

**docs/MVVM_ARCHITECTURE.md:**
- Vue d'ensemble du pattern MVVM
- Structure complète du projet
- CommunityToolkit.Mvvm (ObservableProperty, RelayCommand)
- Description détaillée de chaque ViewModel
- Description détaillée de chaque Service
- Configuration Dependency Injection
- Data Binding (propriétés, commandes, collections)
- Navigation (simple, avec paramètres)
- Support multilingue
- Configuration (Settings)
- Flux de données complets
- Best practices (ViewModels, Services, Views)
- Exemples de tests unitaires
- Évolutions futures

**Caractéristiques de l'Architecture:**

🎨 **UI Moderne:**
- Design Material-inspired (couleurs #2196F3, #4CAF50)
- Boutons avec coins arrondis
- ProgressBar pour feedback utilisateur
- StatusBar pour informations contextuelles

🏗️ **Architecture MVVM:**
- Séparation stricte View/ViewModel/Model
- CommunityToolkit.Mvvm pour code généré
- Pas de code-behind (sauf InitializeComponent)
- Data binding bidirectionnel

🔌 **Dependency Injection:**
- Microsoft.Extensions.DependencyInjection
- Services Singleton pour état partagé
- ViewModels Transient pour isolation
- Injection via constructeur

🧭 **Navigation:**
- Service centralisé
- Mapping par convention
- Support des paramètres
- Gestion automatique du cycle de vie

🌍 **Multilingue:**
- Fichiers .resx pour FR/AR
- Support NFR-U1
- Changement dynamique possible

⚙️ **Configuration:**
- Settings.settings pour persistance
- API endpoint configurable
- Préférences utilisateur

**Notes Importantes:**

📝 **Implémentations Mock:**
- AuthenticationService: Mock OAuth 2.0 (à remplacer Story 4.5)
- ApiClientService: Données de test (à remplacer Story 4.5 avec Refit)
- SignatureService: Mock signature (à remplacer Story 4.3 avec iText 7)
- DongleService: Mock dongle (à remplacer Story 4.2 avec PKCS#11)

📝 **XAML Hot Reload:**
- Fonctionne avec .NET 10 + WPF
- Modifications XAML visibles immédiatement
- Accélère le développement UI

📝 **Tests:**
- Architecture testable (DI + interfaces)
- ViewModels testables unitairement
- Services mockables facilement
- Tests non implémentés dans cette story

📝 **Évolutions Futures:**
- Story 4.2: Intégration PKCS#11 pour dongle USB
- Story 4.3: Signature PAdES avec iText 7
- Story 4.4: Validation certificat OCSP/CRL
- Story 4.5: Communication API avec Refit
- Story 4.6: Batch signing avec progress tracking

### File List

**Fichiers Créés:**

**Views:**
- `Views/LoginView.xaml` + `.xaml.cs` - Vue de connexion OAuth 2.0
- `Views/SigningView.xaml` + `.xaml.cs` - Vue de signature avec progress
- `Views/SettingsView.xaml` + `.xaml.cs` - Vue de configuration

**ViewModels:**
- `ViewModels/LoginViewModel.cs` - ViewModel connexion
- `ViewModels/SigningViewModel.cs` - ViewModel signature
- `ViewModels/SettingsViewModel.cs` - ViewModel paramètres

**Services - Navigation:**
- `Services/Navigation/INavigationService.cs` - Interface navigation
- `Services/Navigation/NavigationService.cs` - Implémentation navigation
- `Services/Navigation/INavigationAware.cs` - Interface pour paramètres

**Services - Authentication:**
- `Services/Authentication/IAuthenticationService.cs` - Interface auth
- `Services/Authentication/AuthenticationService.cs` - Mock auth

**Services - API:**
- `Services/Api/IApiClientService.cs` - Interface API client
- `Services/Api/ApiClientService.cs` - Mock API client

**Services - Signature:**
- `Services/Signature/ISignatureService.cs` - Interface signature
- `Services/Signature/SignatureService.cs` - Mock signature

**Services - Dongle:**
- `Services/Dongle/IDongleService.cs` - Interface dongle
- `Services/Dongle/DongleService.cs` - Mock dongle

**Models:**
- `Models/DocumentDto.cs` - DTO pour documents

**Resources:**
- `Resources/Strings.resx` - Ressources français
- `Resources/Strings.ar.resx` - Ressources arabe

**Configuration:**
- `Properties/Settings.settings` - Configuration application
- `Properties/Settings.Designer.cs` - Code généré settings

**Documentation:**
- `docs/MVVM_ARCHITECTURE.md` - Documentation complète MVVM

**Fichiers Modifiés:**
- `ViewModels/MainViewModel.cs` - Remplacé par implémentation complète
- `App.xaml.cs` - Configuration DI complète

**Fonctionnalités Implémentées:**
- ✅ Architecture MVVM complète avec CommunityToolkit.Mvvm
- ✅ 4 Views (Login, Main, Signing, Settings)
- ✅ 4 ViewModels avec [ObservableProperty] et [RelayCommand]
- ✅ NavigationService avec mapping ViewModel→View
- ✅ 6 Services avec interfaces (mock implementations)
- ✅ Dependency Injection (Microsoft.Extensions.DependencyInjection)
- ✅ Support multilingue FR/AR (NFR-U1)
- ✅ Configuration persistante (Settings.settings)
- ✅ Data binding bidirectionnel
- ✅ Navigation avec paramètres (INavigationAware)
- ✅ Progress tracking pour signature
- ✅ UI moderne et responsive
- ✅ Documentation complète

**Packages NuGet Utilisés:**
- CommunityToolkit.Mvvm 8.3.2
- Microsoft.Extensions.DependencyInjection 10.0.0
- CredentialManagement 1.0.2 (déjà présent)
- System.IdentityModel.Tokens.Jwt 8.16.0 (déjà présent)

**Conformité:**
- ✅ NFR-U1: Support multilingue (FR/AR)
- ✅ MVVM Pattern avec CommunityToolkit
- ✅ Dependency Injection
- ✅ Navigation service
- ✅ XAML Hot Reload compatible

**À Implémenter (Stories Futures):**
- Story 4.2: Remplacer DongleService par PKCS#11
- Story 4.3: Remplacer SignatureService par iText 7 PAdES
- Story 4.4: Validation certificat OCSP/CRL
- Story 4.5: Remplacer AuthenticationService et ApiClientService par implémentations réelles (OAuth 2.0 + Refit)
- Story 4.6: Batch signing avec progress tracking
- Tests unitaires pour ViewModels et Services
