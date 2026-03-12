using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace AcadSign.Backend.Web.Infrastructure;

internal sealed class AuthorizeOperationSecurityTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;

        var requiresAuth = metadata.Any(m => m is IAuthorizeData);
        var allowAnonymous = metadata.Any(m => m is IAllowAnonymous);

        if (!requiresAuth || allowAnonymous)
        {
            return Task.CompletedTask;
        }

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        var bearerRef = new OpenApiSecuritySchemeReference("Bearer", null, null);
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [bearerRef] = new List<string>()
        });

        return Task.CompletedTask;
    }
}
