using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration is missing.");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Dashboard", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:5173"];

        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Dashboard");
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapGet("/api/health", (IConfiguration configuration) =>
    {
        var connectionString = configuration.GetConnectionString("KeyMapperDb");

        return Results.Ok(new HealthResponse(
            Service: "KeyMapper API",
            Status: "Healthy",
            Environment: app.Environment.EnvironmentName,
            DatabaseConfigured: !string.IsNullOrWhiteSpace(connectionString),
            TimestampUtc: DateTimeOffset.UtcNow));
    })
    .AllowAnonymous()
    .WithName("GetApiHealth")
    .WithOpenApi();

app.MapGet("/api/auth/ping", [Authorize] () => Results.Ok(new
    {
        message = "JWT authentication is configured.",
        timestampUtc = DateTimeOffset.UtcNow
    }))
    .WithName("AuthenticatedPing")
    .WithOpenApi();

app.Run();

internal sealed record JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "KeyMapper.Api";

    public string Audience { get; init; } = "KeyMapper.Clients";

    public string SigningKey { get; init; } = "change-this-development-signing-key-before-production";
}

internal sealed record HealthResponse(
    string Service,
    string Status,
    string Environment,
    bool DatabaseConfigured,
    DateTimeOffset TimestampUtc);
