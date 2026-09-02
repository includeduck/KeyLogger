using System.Text.RegularExpressions;

namespace KeyMapper.Api.Services;

public static class PasswordValidator
{
    private static readonly Regex PasswordPolicy = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$",
        RegexOptions.Compiled);

    public static bool IsValid(string password) => PasswordPolicy.IsMatch(password);

    public const string PolicyMessage =
        "Password must be at least 8 characters and include uppercase, lowercase, digit, and special character.";
}

public sealed class PasswordService
{
    private const int WorkFactor = 12;

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string passwordHash) =>
        BCrypt.Net.BCrypt.Verify(password, passwordHash);
}
