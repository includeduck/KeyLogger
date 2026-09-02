using System.Security.Claims;
using KeyMapper.Api.Models;
using KeyMapper.Api.Services;

namespace KeyMapper.Api.Endpoints;

public static class ProfileEndpoints
{
    public static RouteGroupBuilder MapProfileEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/profile")
            .WithTags("Profile")
            .RequireAuthorization();

        group.MapGet("/", GetProfileAsync);
        group.MapPut("/password", ChangePasswordAsync);

        return group;
    }

    private static async Task<IResult> GetProfileAsync(
        ClaimsPrincipal user,
        AuthService authService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var profile = await authService.GetProfileAsync(userId.Value, cancellationToken);

        if (profile is null)
        {
            return Results.NotFound(new ErrorResponse("User not found."));
        }

        return Results.Ok(profile);
    }

    private static async Task<IResult> ChangePasswordAsync(
        ChangePasswordRequest request,
        ClaimsPrincipal user,
        AuthService authService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var (success, error) = await authService.ChangePasswordAsync(userId.Value, request, cancellationToken);

        if (!success)
        {
            return Results.Json(new ErrorResponse(error!), statusCode: StatusCodes.Status400BadRequest);
        }

        return Results.Ok(new MessageResponse("Password changed successfully. Please sign in again."));
    }

    private static int? GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");

        return int.TryParse(sub, out var userId) ? userId : null;
    }
}
