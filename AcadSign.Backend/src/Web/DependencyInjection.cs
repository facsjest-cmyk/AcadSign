using Azure.Identity;
using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Domain.Constants;
using AcadSign.Backend.Infrastructure.Data;
using AcadSign.Backend.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddScoped<IUser, CurrentUser>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        builder.Services.AddExceptionHandler<CustomExceptionHandler>();

        // Configure OpenIddict for OAuth 2.0 / OpenID Connect
        builder.AddOpenIddictServices();

        // Configure authorization policies for scopes and roles
        builder.Services.AddAuthorization(options =>
        {
            // Scope-based policies
            options.AddPolicy("RequireDocumentGenerateScope", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", "api.documents.generate");
            });

            options.AddPolicy("RequireDocumentReadScope", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", "api.documents.read");
            });

            options.AddPolicy("RequireDocumentSignScope", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", "api.documents.sign");
            });

            // Role-based policies
            options.AddPolicy("RequireAdminRole", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(Roles.Administrator);
            });

            options.AddPolicy("RequireRegistrarRole", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(Roles.Registrar, Roles.Administrator); // Admin has Registrar access
            });

            options.AddPolicy("RequireAuditorRole", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(Roles.Auditor, Roles.Administrator); // Admin has Auditor access
            });

            options.AddPolicy("RequireApiClientRole", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(Roles.ApiClient);
            });
        });

        // Customise default API behaviour
        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddOpenApi(options =>
        {
            options.AddOperationTransformer<ApiExceptionOperationTransformer>();
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        });
    }

    public static void AddKeyVaultIfConfigured(this IHostApplicationBuilder builder)
    {
        var keyVaultUri = builder.Configuration["AZURE_KEY_VAULT_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(keyVaultUri))
        {
            builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUri),
                new DefaultAzureCredential());
        }
    }
}
