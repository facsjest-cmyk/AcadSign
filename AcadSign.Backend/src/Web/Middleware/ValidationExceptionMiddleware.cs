using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace AcadSign.Backend.Web.Middleware;

public class ValidationExceptionMiddleware
{
    private readonly RequestDelegate _next;
    
    public ValidationExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            
            var errorResponse = new ValidationErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Les données fournies sont invalides",
                    Details = ex.Errors.Select(e => new ValidationError
                    {
                        Field = ToCamelCase(e.PropertyName),
                        Message = e.ErrorMessage
                    }).ToList(),
                    Timestamp = DateTime.UtcNow,
                    RequestId = context.TraceIdentifier
                }
            };
            
            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }
    
    private string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return char.ToLowerInvariant(str[0]) + str.Substring(1);
    }
}

public class ValidationErrorResponse
{
    public ErrorDetail Error { get; set; } = new();
}

public class ErrorDetail
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<ValidationError> Details { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public string RequestId { get; set; } = string.Empty;
}

public class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
