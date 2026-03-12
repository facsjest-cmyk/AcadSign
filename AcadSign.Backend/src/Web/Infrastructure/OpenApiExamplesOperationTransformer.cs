using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

namespace AcadSign.Backend.Web.Infrastructure;

internal sealed class OpenApiExamplesOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var method = context.Description.HttpMethod?.ToUpperInvariant();
        var relativePath = "/" + (context.Description.RelativePath?.TrimStart('/') ?? string.Empty);

        if (string.Equals(method, "POST", StringComparison.OrdinalIgnoreCase)
            && relativePath.EndsWith("/api/v1/auth/login", StringComparison.OrdinalIgnoreCase))
        {
            AddLoginExamples(operation);
        }

        if (string.Equals(method, "POST", StringComparison.OrdinalIgnoreCase)
            && relativePath.EndsWith("/api/v1/documents/batch", StringComparison.OrdinalIgnoreCase))
        {
            AddBatchExamples(operation);
        }

        return Task.CompletedTask;
    }

    private static void AddLoginExamples(OpenApiOperation operation)
    {
        if (operation.RequestBody?.Content != null
            && operation.RequestBody.Content.TryGetValue("application/json", out var requestMediaType))
        {
            requestMediaType.Example = new JsonObject
            {
                ["username"] = JsonValue.Create("api-client"),
                ["password"] = JsonValue.Create("ApiClient123!")
            };
        }

        if (operation.Responses != null
            && operation.Responses.TryGetValue("200", out var okResponse)
            && okResponse.Content != null
            && okResponse.Content.TryGetValue("application/json", out var okMediaType))
        {
            okMediaType.Example = new JsonObject
            {
                ["success"] = JsonValue.Create(true),
                ["accessToken"] = JsonValue.Create("<jwt-access-token>"),
                ["refreshToken"] = JsonValue.Create("<refresh-token>"),
                ["expiresAt"] = JsonValue.Create(DateTime.UtcNow.AddHours(1).ToString("O"))
            };
        }
    }

    private static void AddBatchExamples(OpenApiOperation operation)
    {
        if (operation.RequestBody?.Content != null
            && operation.RequestBody.Content.TryGetValue("application/json", out var requestMediaType))
        {
            requestMediaType.Example = new JsonObject
            {
                ["documents"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["studentId"] = JsonValue.Create("STU-001"),
                        ["firstName"] = JsonValue.Create("Amina"),
                        ["lastName"] = JsonValue.Create("El Idrissi"),
                        ["cin"] = JsonValue.Create("AA123456"),
                        ["cne"] = JsonValue.Create("CNE123456"),
                        ["documentType"] = JsonValue.Create(0),
                        ["programName"] = JsonValue.Create("Informatique"),
                        ["academicYear"] = JsonValue.Create("2025-2026")
                    }
                }
            };
        }

        if (operation.Responses != null
            && operation.Responses.TryGetValue("202", out var acceptedResponse)
            && acceptedResponse.Content != null
            && acceptedResponse.Content.TryGetValue("application/json", out var acceptedMediaType))
        {
            acceptedMediaType.Example = new JsonObject
            {
                ["batchId"] = JsonValue.Create("00000000-0000-0000-0000-000000000000"),
                ["totalDocuments"] = JsonValue.Create(1),
                ["status"] = JsonValue.Create("PROCESSING"),
                ["createdAt"] = JsonValue.Create(DateTime.UtcNow.ToString("O")),
                ["statusUrl"] = JsonValue.Create("/api/v1/documents/batch/00000000-0000-0000-0000-000000000000/status")
            };
        }
    }
}
