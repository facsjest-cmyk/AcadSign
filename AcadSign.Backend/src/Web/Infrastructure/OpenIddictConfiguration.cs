using AcadSign.Backend.Infrastructure.Data;

namespace Microsoft.Extensions.DependencyInjection;

public static class OpenIddictConfiguration
{
    public static void AddOpenIddictServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOpenIddict()
            // Register EF Core stores
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<ApplicationDbContext>();
            })
            
            // Register ASP.NET Core components
            .AddServer(options =>
            {
                // Enable OAuth 2.0 flows
                options.AllowClientCredentialsFlow();
                options.AllowAuthorizationCodeFlow()
                    .RequireProofKeyForCodeExchange();
                options.AllowRefreshTokenFlow();
                
                // Set token lifetimes
                options.SetAccessTokenLifetime(TimeSpan.FromHours(1));
                options.SetRefreshTokenLifetime(TimeSpan.FromDays(7));
                
                // Register endpoints
                options.SetTokenEndpointUris("/connect/token");
                options.SetAuthorizationEndpointUris("/connect/authorize");
                options.SetIntrospectionEndpointUris("/connect/introspect");
                
                // Enable endpoint passthrough for custom handling
                options.UseAspNetCore()
                    .EnableTokenEndpointPassthrough()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableUserinfoEndpointPassthrough();
                
                // Register signing and encryption credentials
                if (builder.Environment.IsDevelopment())
                {
                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();
                }
                else
                {
                    // Production: Use certificates from Azure Key Vault
                    // TODO: Story 2.1 - Configure production certificates from Azure Key Vault
                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();
                }
            })
            
            // Register validation components
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });
    }
}
