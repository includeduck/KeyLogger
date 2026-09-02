namespace KeyMapper.Api.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "KeyMapper.Api";

    public string Audience { get; init; } = "KeyMapper.Clients";

    public string SigningKey { get; init; } = "change-this-development-signing-key-before-production";

    public int AccessTokenExpirationMinutes { get; init; } = 720;

    public int RefreshTokenExpirationDays { get; init; } = 30;
}

public sealed class AppOptions
{
    public const string SectionName = "App";

    public string DashboardBaseUrl { get; init; } = "http://localhost:5173";
}
