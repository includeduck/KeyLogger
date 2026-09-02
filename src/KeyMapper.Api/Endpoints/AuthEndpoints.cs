using System.Security.Claims;
using KeyMapper.Api.Models;
using KeyMapper.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace KeyMapper.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", RegisterAsync);
        group.MapGet("/verify-email", VerifyEmailAsync);
        group.MapPost("/login", LoginAsync);
        group.MapPost("/refresh", RefreshAsync);
        group.MapPost("/logout", LogoutAsync).RequireAuthorization();
        group.MapPost("/forgot-password", ForgotPasswordAsync);
        group.MapPost("/reset-password", ResetPasswordAsync);

        return group;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        AuthService authService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var (success, error, user) = await authService.RegisterAsync(
            request,
            httpContext.Connection.RemoteIpAddress?.ToString(),
            httpContext.Request.Headers.UserAgent.ToString(),
            cancellationToken);

        if (!success)
        {
            var statusCode = error is "Username is already taken." or "Email is already registered."
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest;

            return Results.Json(new ErrorResponse(error!), statusCode: statusCode);
        }

        return Results.Created(
            $"/api/profile/{user!.UserId}",
            new MessageResponse("Registration successful. Check the API console for the verification link."));
    }

    private static async Task<IResult> VerifyEmailAsync(
        [FromQuery] string token,
        AuthService authService,
        CancellationToken cancellationToken)
    {
        var (success, error) = await authService.VerifyEmailAsync(token, cancellationToken);

        if (!success)
        {
            return Results.Json(new ErrorResponse(error!), statusCode: StatusCodes.Status400BadRequest);
        }

        return Results.Ok(new MessageResponse("Email verified successfully. You can now sign in."));
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        AuthService authService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var (success, error, response) = await authService.LoginAsync(
            request,
            httpContext.Connection.RemoteIpAddress?.ToString(),
            httpContext.Request.Headers.UserAgent.ToString(),
            cancellationToken);

        if (!success)
        {
            return Results.Json(new ErrorResponse(error!), statusCode: StatusCodes.Status401Unauthorized);
        }

        return Results.Ok(response);
    }

    private static async Task<IResult> RefreshAsync(
        RefreshRequest request,
        AuthService authService,
        CancellationToken cancellationToken)
    {
        var (success, error, response) = await authService.RefreshAsync(request.RefreshToken, cancellationToken);

        if (!success)
        {
            return Results.Json(new ErrorResponse(error!), statusCode: StatusCodes.Status401Unauthorized);
        }

        return Results.Ok(response);
    }

    private static async Task<IResult> LogoutAsync(
        LogoutRequest request,
        TokenService tokenService,
        CancellationToken cancellationToken)
    {
        await tokenService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);
        return Results.Ok(new MessageResponse("Logged out successfully."));
    }

    private static async Task<IResult> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        AuthService authService,
        CancellationToken cancellationToken)
    {
        await authService.ForgotPasswordAsync(request.Email, cancellationToken);
        return Results.Ok(new MessageResponse(
            "If an account exists for that email, a password reset link has been sent."));
    }

    private static async Task<IResult> ResetPasswordAsync(
        ResetPasswordRequest request,
        AuthService authService,
        CancellationToken cancellationToken)
    {
        var (success, error) = await authService.ResetPasswordAsync(request, cancellationToken);

        if (!success)
        {
            return Results.Json(new ErrorResponse(error!), statusCode: StatusCodes.Status400BadRequest);
        }

        return Results.Ok(new MessageResponse("Password reset successfully. You can now sign in."));
    }
}
