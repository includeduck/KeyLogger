namespace KeyMapper.Api.Models;

public sealed record RegisterRequest(
    string Username,
    string Email,
    string Password,
    bool AcceptTerms);

public sealed record LoginRequest(
    string UsernameOrEmail,
    string Password);

public sealed record RefreshRequest(
    string RefreshToken);

public sealed record LogoutRequest(
    string RefreshToken);

public sealed record ForgotPasswordRequest(
    string Email);

public sealed record ResetPasswordRequest(
    string Token,
    string NewPassword);

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    UserSummary User);

public sealed record UserSummary(
    int UserId,
    string Username,
    string Email,
    bool IsEmailVerified);

public sealed record ProfileResponse(
    int UserId,
    string Username,
    string Email,
    bool IsEmailVerified,
    DateTimeOffset CreatedAt);

public sealed record MessageResponse(
    string Message);

public sealed record ErrorResponse(
    string Error);
