using System.Security.Cryptography;
using KeyMapper.Api.Configuration;
using KeyMapper.Api.Data;
using KeyMapper.Api.Data.Entities;
using KeyMapper.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KeyMapper.Api.Services;

public sealed class AuthService(
    KeyMapperDbContext dbContext,
    PasswordService passwordService,
    TokenService tokenService,
    AuditService auditService,
    IEmailSender emailSender,
    IOptions<AppOptions> appOptions)
{
    private readonly AppOptions _appOptions = appOptions.Value;

    public async Task<(bool Success, string? Error, User? User)> RegisterAsync(
        RegisterRequest request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        if (!request.AcceptTerms)
        {
            return (false, "You must accept the terms of service.", null);
        }

        if (string.IsNullOrWhiteSpace(request.Username)
            || string.IsNullOrWhiteSpace(request.Email)
            || string.IsNullOrWhiteSpace(request.Password))
        {
            return (false, "Username, email, and password are required.", null);
        }

        if (!PasswordValidator.IsValid(request.Password))
        {
            return (false, PasswordValidator.PolicyMessage, null);
        }

        var normalizedUsername = request.Username.Trim();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var usernameExists = await dbContext.Users
            .AnyAsync(u => u.Username == normalizedUsername && !u.IsDeleted, cancellationToken);

        if (usernameExists)
        {
            return (false, "Username is already taken.", null);
        }

        var emailExists = await dbContext.Users
            .AnyAsync(u => u.Email == normalizedEmail && !u.IsDeleted, cancellationToken);

        if (emailExists)
        {
            return (false, "Email is already registered.", null);
        }

        var verificationToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

        var user = new User
        {
            Username = normalizedUsername,
            Email = normalizedEmail,
            PasswordHash = passwordService.Hash(request.Password),
            IsEmailVerified = false,
            EmailVerificationToken = verificationToken,
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Preferences = new UserPreferences()
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        var verificationUrl =
            $"{_appOptions.DashboardBaseUrl.TrimEnd('/')}/verify-email?token={Uri.EscapeDataString(verificationToken)}";

        await emailSender.SendVerificationEmailAsync(user.Email, verificationUrl, cancellationToken);

        await auditService.LogAsync(
            user.UserId,
            "User",
            "USER_REGISTER",
            "User",
            user.Username,
            ipAddress,
            userAgent,
            cancellationToken: cancellationToken);

        return (true, null, user);
    }

    public async Task<(bool Success, string? Error)> VerifyEmailAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return (false, "Verification token is required.");
        }

        var user = await dbContext.Users
            .FirstOrDefaultAsync(
                u => u.EmailVerificationToken == token && !u.IsDeleted,
                cancellationToken);

        if (user is null)
        {
            return (false, "Invalid verification token.");
        }

        if (user.EmailVerificationTokenExpiry is null || user.EmailVerificationTokenExpiry < DateTime.UtcNow)
        {
            return (false, "Verification token has expired.");
        }

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return (true, null);
    }

    public async Task<(bool Success, string? Error, AuthResponse? Response)> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.UsernameOrEmail) || string.IsNullOrWhiteSpace(request.Password))
        {
            return (false, "Username or email and password are required.", null);
        }

        var loginValue = request.UsernameOrEmail.Trim();
        var normalizedEmail = loginValue.ToLowerInvariant();

        var user = await dbContext.Users
            .FirstOrDefaultAsync(
                u => (u.Username == loginValue || u.Email == normalizedEmail) && !u.IsDeleted,
                cancellationToken);

        if (user is null || !passwordService.Verify(request.Password, user.PasswordHash))
        {
            await auditService.LogAsync(
                user?.UserId,
                "User",
                "LOGIN_FAILURE",
                "User",
                loginValue,
                ipAddress,
                userAgent,
                cancellationToken: cancellationToken);

            return (false, "Invalid username/email or password.", null);
        }

        if (!user.IsEmailVerified)
        {
            return (false, "Please verify your email before signing in.", null);
        }

        if (user.IsSuspended)
        {
            return (false, "Your account has been suspended.", null);
        }

        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        var accessToken = tokenService.GenerateAccessToken(user);
        var (refreshToken, _) = await tokenService.CreateRefreshTokenAsync(user, cancellationToken: cancellationToken);

        await auditService.LogAsync(
            user.UserId,
            "User",
            "LOGIN_SUCCESS",
            "User",
            user.Username,
            ipAddress,
            userAgent,
            cancellationToken: cancellationToken);

        return (true, null, new AuthResponse(
            accessToken,
            refreshToken,
            tokenService.GetAccessTokenExpiry(),
            ToUserSummary(user)));
    }

    public async Task<(bool Success, string? Error, AuthResponse? Response)> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var storedToken = await tokenService.GetValidRefreshTokenAsync(refreshToken, cancellationToken);

        if (storedToken?.User is null || storedToken.User.IsDeleted || storedToken.User.IsSuspended)
        {
            return (false, "Invalid or expired refresh token.", null);
        }

        if (!storedToken.User.IsEmailVerified)
        {
            return (false, "Email verification required.", null);
        }

        await tokenService.RevokeRefreshTokenAsync(refreshToken, cancellationToken);

        var user = storedToken.User;
        var accessToken = tokenService.GenerateAccessToken(user);
        var (newRefreshToken, _) = await tokenService.CreateRefreshTokenAsync(user, storedToken.DeviceId, cancellationToken);

        return (true, null, new AuthResponse(
            accessToken,
            newRefreshToken,
            tokenService.GetAccessTokenExpiry(),
            ToUserSummary(user)));
    }

    public async Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail && !u.IsDeleted, cancellationToken);

        if (user is null)
        {
            return;
        }

        var resetToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        user.PasswordResetToken = resetToken;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        var resetUrl =
            $"{_appOptions.DashboardBaseUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(resetToken)}";

        await emailSender.SendPasswordResetEmailAsync(user.Email, resetUrl, cancellationToken);
    }

    public async Task<(bool Success, string? Error)> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return (false, "Token and new password are required.");
        }

        if (!PasswordValidator.IsValid(request.NewPassword))
        {
            return (false, PasswordValidator.PolicyMessage);
        }

        var user = await dbContext.Users
            .FirstOrDefaultAsync(
                u => u.PasswordResetToken == request.Token && !u.IsDeleted,
                cancellationToken);

        if (user is null)
        {
            return (false, "Invalid reset token.");
        }

        if (user.PasswordResetTokenExpiry is null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
        {
            return (false, "Reset token has expired.");
        }

        user.PasswordHash = passwordService.Hash(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        await tokenService.RevokeAllUserRefreshTokensAsync(user.UserId, cancellationToken);

        return (true, null);
    }

    public async Task<ProfileResponse?> GetProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted, cancellationToken);

        if (user is null)
        {
            return null;
        }

        return new ProfileResponse(
            user.UserId,
            user.Username,
            user.Email,
            user.IsEmailVerified,
            user.CreatedAt);
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(
        int userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return (false, "Current and new passwords are required.");
        }

        if (!PasswordValidator.IsValid(request.NewPassword))
        {
            return (false, PasswordValidator.PolicyMessage);
        }

        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted, cancellationToken);

        if (user is null)
        {
            return (false, "User not found.");
        }

        if (!passwordService.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return (false, "Current password is incorrect.");
        }

        user.PasswordHash = passwordService.Hash(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        await tokenService.RevokeAllUserRefreshTokensAsync(user.UserId, cancellationToken);

        return (true, null);
    }

    private static UserSummary ToUserSummary(User user) =>
        new(user.UserId, user.Username, user.Email, user.IsEmailVerified);
}
