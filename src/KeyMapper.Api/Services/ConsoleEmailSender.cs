namespace KeyMapper.Api.Services;

public interface IEmailSender
{
    Task SendVerificationEmailAsync(string email, string verificationUrl, CancellationToken cancellationToken = default);

    Task SendPasswordResetEmailAsync(string email, string resetUrl, CancellationToken cancellationToken = default);
}

public sealed class ConsoleEmailSender(ILogger<ConsoleEmailSender> logger) : IEmailSender
{
    public Task SendVerificationEmailAsync(string email, string verificationUrl, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "[Email Simulation] Verification email for {Email}. Open this link to verify: {VerificationUrl}",
            email,
            verificationUrl);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmailAsync(string email, string resetUrl, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "[Email Simulation] Password reset email for {Email}. Open this link to reset: {ResetUrl}",
            email,
            resetUrl);

        return Task.CompletedTask;
    }
}
