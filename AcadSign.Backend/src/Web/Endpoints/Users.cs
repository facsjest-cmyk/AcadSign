using AcadSign.Backend.Domain.Constants;
using AcadSign.Backend.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AcadSign.Backend.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapIdentityApi<ApplicationUser>();
        
        group.MapPost(AssignRole, "{userId}/roles")
            .RequireAuthorization("RequireAdminRole");
        
        group.MapDelete(RemoveRole, "{userId}/roles/{roleName}")
            .RequireAuthorization("RequireAdminRole");
        
        group.MapGet(GetUserRoles, "{userId}/roles")
            .RequireAuthorization("RequireAdminRole");
    }

    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IResult> AssignRole(
        string userId,
        [FromBody] AssignRoleRequest request,
        [FromServices] UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Results.NotFound(new { error = "User not found" });
        }

        var result = await userManager.AddToRoleAsync(user, request.RoleName);
        if (!result.Succeeded)
        {
            return Results.BadRequest(new { errors = result.Errors });
        }

        return Results.Ok(new { message = $"Role '{request.RoleName}' assigned to user '{user.UserName}'" });
    }

    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IResult> RemoveRole(
        string userId,
        string roleName,
        [FromServices] UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Results.NotFound(new { error = "User not found" });
        }

        var result = await userManager.RemoveFromRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            return Results.BadRequest(new { errors = result.Errors });
        }

        return Results.Ok(new { message = $"Role '{roleName}' removed from user '{user.UserName}'" });
    }

    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IResult> GetUserRoles(
        string userId,
        [FromServices] UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Results.NotFound(new { error = "User not found" });
        }

        var roles = await userManager.GetRolesAsync(user);
        return Results.Ok(new { userId = user.Id, userName = user.UserName, roles });
    }
}

public record AssignRoleRequest(string RoleName);
