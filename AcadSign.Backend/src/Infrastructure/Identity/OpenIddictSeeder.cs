using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;

namespace AcadSign.Backend.Infrastructure.Identity;

public static class OpenIddictSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        await SeedScopesAsync(serviceProvider);
        await SeedClientsAsync(serviceProvider);
    }

    private static async Task SeedScopesAsync(IServiceProvider serviceProvider)
    {
        var scopeManager = serviceProvider.GetRequiredService<IOpenIddictScopeManager>();

        // api.documents.generate
        if (await scopeManager.FindByNameAsync("api.documents.generate") == null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "api.documents.generate",
                DisplayName = "Generate Documents",
                Description = "Permission to generate academic documents",
                Resources = { "acadsign-api" }
            });
        }

        // api.documents.read
        if (await scopeManager.FindByNameAsync("api.documents.read") == null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "api.documents.read",
                DisplayName = "Read Documents",
                Description = "Permission to read document metadata",
                Resources = { "acadsign-api" }
            });
        }

        // api.documents.sign
        if (await scopeManager.FindByNameAsync("api.documents.sign") == null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "api.documents.sign",
                DisplayName = "Sign Documents",
                Description = "Permission to sign academic documents",
                Resources = { "acadsign-api" }
            });
        }
    }

    private static async Task SeedClientsAsync(IServiceProvider serviceProvider)
    {
        var applicationManager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        // SIS Laravel Client
        if (await applicationManager.FindByClientIdAsync("sis-laravel-client") == null)
        {
            var clientSecret = GenerateSecureSecret();

            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "sis-laravel-client",
                ClientSecret = clientSecret,
                DisplayName = "SIS Laravel Application",
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.Endpoints.Introspection,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "api.documents.generate",
                    OpenIddictConstants.Permissions.Prefixes.Scope + "api.documents.read"
                }
            });

            // Log the secret for SIS configuration (only once during seeding)
            Console.WriteLine("===========================================");
            Console.WriteLine("SIS Laravel Client Created");
            Console.WriteLine($"Client ID: sis-laravel-client");
            Console.WriteLine($"Client Secret: {clientSecret}");
            Console.WriteLine("IMPORTANT: Save this secret securely!");
            Console.WriteLine("===========================================");
        }

        // Desktop App Client
        if (await applicationManager.FindByClientIdAsync("acadsign-desktop") == null)
        {
            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "acadsign-desktop",
                DisplayName = "AcadSign Desktop Application",
                ClientType = OpenIddictConstants.ClientTypes.Public, // Public client (no client secret)
                RedirectUris = { new Uri("http://localhost:7890/callback") },
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    "scp:openid",
                    "scp:profile",
                    "scp:email",
                    OpenIddictConstants.Permissions.Prefixes.Scope + "api.documents.sign"
                },
                Requirements =
                {
                    OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange // PKCE required
                }
            });

            Console.WriteLine("===========================================");
            Console.WriteLine("Desktop App Client Created");
            Console.WriteLine($"Client ID: acadsign-desktop");
            Console.WriteLine("Type: Public Client (no secret)");
            Console.WriteLine("PKCE: Required");
            Console.WriteLine("===========================================");
        }
    }

    private static string GenerateSecureSecret()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public static async Task RotateClientSecretAsync(string clientId, IServiceProvider serviceProvider)
    {
        var applicationManager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        var application = await applicationManager.FindByClientIdAsync(clientId);
        if (application == null)
        {
            throw new InvalidOperationException($"Client '{clientId}' not found");
        }

        var newSecret = GenerateSecureSecret();

        await applicationManager.UpdateAsync(application, new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientSecret = newSecret,
            DisplayName = await applicationManager.GetDisplayNameAsync(application),
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.Endpoints.Introspection,
                OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                OpenIddictConstants.Permissions.Prefixes.Scope + "api.documents.generate",
                OpenIddictConstants.Permissions.Prefixes.Scope + "api.documents.read"
            }
        });

        Console.WriteLine("===========================================");
        Console.WriteLine($"Client Secret Rotated for: {clientId}");
        Console.WriteLine($"New Secret: {newSecret}");
        Console.WriteLine("Update the SIS Laravel configuration!");
        Console.WriteLine("===========================================");
    }
}
