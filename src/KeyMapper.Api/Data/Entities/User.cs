namespace KeyMapper.Api.Data.Entities;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiry { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
    public bool IsSuspended { get; set; }
    public string? SuspensionReason { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletionRequestDate { get; set; }
    public DateTime? PermanentDeletionDate { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string? TwoFactorSecret { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public UserPreferences? Preferences { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
