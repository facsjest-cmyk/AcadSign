using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AcadSign.Backend.Web.Endpoints;

public class Authorization : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapGet(Authorize, "/authorize").AllowAnonymous();
        group.MapPost(Authorize, "/authorize").AllowAnonymous();
        group.MapPost(Accept, "/authorize/accept").AllowAnonymous();
    }

    [AllowAnonymous]
    public async Task<IResult> Authorize(
        HttpContext httpContext,
        [FromServices] IOpenIddictApplicationManager applicationManager,
        [FromServices] IOpenIddictAuthorizationManager authorizationManager,
        [FromServices] IOpenIddictScopeManager scopeManager)
    {
        var request = httpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Retrieve the user principal stored in the authentication cookie
        var result = await httpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

        // If the user is not authenticated, redirect to the login page
        if (!result.Succeeded)
        {
            return Results.Challenge(
                properties: new AuthenticationProperties
                {
                    RedirectUri = httpContext.Request.PathBase + httpContext.Request.Path + QueryString.Create(
                        httpContext.Request.HasFormContentType ? httpContext.Request.Form.ToList() : httpContext.Request.Query.ToList())
                },
                authenticationSchemes: new[] { IdentityConstants.ApplicationScheme });
        }

        // Retrieve the profile of the logged in user
        var user = result.Principal;
        if (user == null)
        {
            return Results.Challenge(
                properties: new AuthenticationProperties
                {
                    RedirectUri = httpContext.Request.PathBase + httpContext.Request.Path + QueryString.Create(
                        httpContext.Request.HasFormContentType ? httpContext.Request.Form.ToList() : httpContext.Request.Query.ToList())
                },
                authenticationSchemes: new[] { IdentityConstants.ApplicationScheme });
        }

        // Retrieve the application details from the database
        var application = await applicationManager.FindByClientIdAsync(request.ClientId ?? string.Empty) ??
            throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

        // Retrieve the permanent authorizations associated with the user and the calling client application
        var authorizations = await authorizationManager.FindAsync(
            subject: user.GetClaim(Claims.Subject) ?? string.Empty,
            client: await applicationManager.GetIdAsync(application) ?? string.Empty,
            status: Statuses.Valid,
            type: AuthorizationTypes.Permanent,
            scopes: request.GetScopes()).ToListAsync();

        // Create a new ClaimsIdentity containing the claims that will be used to create an id_token and/or an access token
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        // Add the claims from the user principal
        identity.SetClaim(Claims.Subject, user.GetClaim(ClaimTypes.NameIdentifier))
                .SetClaim(Claims.Email, user.GetClaim(ClaimTypes.Email))
                .SetClaim(Claims.Name, user.GetClaim(ClaimTypes.Name));

        // Add all roles as claims
        var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        foreach (var role in roles)
        {
            identity.AddClaim(new Claim(Claims.Role, role));
        }

        // Add custom claims
        var institutionId = user.GetClaim("institutionId");
        if (!string.IsNullOrEmpty(institutionId))
        {
            identity.SetClaim("institutionId", institutionId);
        }

        // Set the list of scopes granted to the client application
        identity.SetScopes(request.GetScopes());
        identity.SetResources(await scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());

        // Automatically create a permanent authorization to avoid requiring explicit consent for future authorization requests
        var authorization = authorizations.LastOrDefault();
        authorization ??= await authorizationManager.CreateAsync(
            identity: identity,
            subject: user.GetClaim(Claims.Subject) ?? string.Empty,
            client: await applicationManager.GetIdAsync(application) ?? string.Empty,
            type: AuthorizationTypes.Permanent,
            scopes: identity.GetScopes());

        identity.SetAuthorizationId(await authorizationManager.GetIdAsync(authorization));
        identity.SetDestinations(GetDestinations);

        // Return a SignInResult to issue an authorization code
        return Results.SignIn(new ClaimsPrincipal(identity), properties: null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [AllowAnonymous]
    public async Task<IResult> Accept(
        HttpContext httpContext,
        [FromServices] IOpenIddictApplicationManager applicationManager,
        [FromServices] IOpenIddictAuthorizationManager authorizationManager,
        [FromServices] IOpenIddictScopeManager scopeManager)
    {
        var request = httpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        var result = await httpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (!result.Succeeded)
        {
            return Results.Forbid(
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.AccessDenied,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not authenticated."
                }),
                authenticationSchemes: new[] { OpenIddictServerAspNetCoreDefaults.AuthenticationScheme });
        }

        var user = result.Principal;
        if (user == null)
        {
            return Results.Forbid(
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.AccessDenied,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not authenticated."
                }),
                authenticationSchemes: new[] { OpenIddictServerAspNetCoreDefaults.AuthenticationScheme });
        }

        var application = await applicationManager.FindByClientIdAsync(request.ClientId ?? string.Empty);
        if (application == null)
        {
            return Results.Forbid(
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidClient,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The client application was not found."
                }),
                authenticationSchemes: new[] { OpenIddictServerAspNetCoreDefaults.AuthenticationScheme });
        }

        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        identity.SetClaim(Claims.Subject, user.GetClaim(ClaimTypes.NameIdentifier))
                .SetClaim(Claims.Email, user.GetClaim(ClaimTypes.Email))
                .SetClaim(Claims.Name, user.GetClaim(ClaimTypes.Name));

        // Add all roles as claims
        var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        foreach (var role in userRoles)
        {
            identity.AddClaim(new Claim(Claims.Role, role));
        }

        var institutionId = user.GetClaim("institutionId");
        if (!string.IsNullOrEmpty(institutionId))
        {
            identity.SetClaim("institutionId", institutionId);
        }

        identity.SetScopes(request.GetScopes());
        identity.SetResources(await scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());

        var authorization = await authorizationManager.CreateAsync(
            identity: identity,
            subject: user.GetClaim(Claims.Subject) ?? string.Empty,
            client: await applicationManager.GetIdAsync(application) ?? string.Empty,
            type: AuthorizationTypes.Permanent,
            scopes: identity.GetScopes());

        identity.SetAuthorizationId(await authorizationManager.GetIdAsync(authorization));
        identity.SetDestinations(GetDestinations);

        return Results.SignIn(new ClaimsPrincipal(identity), properties: null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        // Note: by default, claims are NOT automatically included in the access and identity tokens.
        // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
        // whether they should be included in access tokens, in identity tokens or in both.

        switch (claim.Type)
        {
            case Claims.Name:
            case Claims.Email:
                yield return Destinations.AccessToken;
                yield return Destinations.IdentityToken;
                break;

            case Claims.Role:
            case "institutionId":
                yield return Destinations.AccessToken;
                break;

            default:
                yield return Destinations.AccessToken;
                break;
        }
    }
}
