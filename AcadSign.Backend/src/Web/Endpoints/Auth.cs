using AcadSign.Backend.Application.Auth.Commands;
using AcadSign.Backend.Application.Auth.Services;
using AcadSign.Backend.Application.Services;
using AcadSign.Backend.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AcadSign.Backend.Web.Endpoints;

public class Auth : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapPost(Login, "/login")
            .AllowAnonymous();
    }

    public async Task<IResult> Login(
        [FromBody] LoginCommand command,
        [FromServices] IAuthService authService,
        [FromServices] IAuditLogService auditService)
    {
        if (string.IsNullOrWhiteSpace(command.Username) || string.IsNullOrWhiteSpace(command.Password))
        {
            return Results.BadRequest(new LoginResponse
            {
                Success = false,
                ErrorMessage = "Nom d'utilisateur et mot de passe requis."
            });
        }

        var result = await authService.LoginAsync(command.Username, command.Password);

        await auditService.LogEventAsync(AuditEventType.USER_LOGIN, null, new
        {
            username = command.Username,
            success = result.Success
        }, userIdOverride: result.Success ? result.User?.Id : null);

        if (!result.Success)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(result);
    }
}
