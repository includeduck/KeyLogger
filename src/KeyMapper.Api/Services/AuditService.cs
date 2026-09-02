using KeyMapper.Api.Data;
using KeyMapper.Api.Data.Entities;

namespace KeyMapper.Api.Services;

public sealed class AuditService(KeyMapperDbContext dbContext)
{
    public async Task LogAsync(
        int? userId,
        string actorType,
        string action,
        string? targetType = null,
        string? targetId = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? details = null,
        CancellationToken cancellationToken = default)
    {
        dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            ActorType = actorType,
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            IPAddress = ipAddress,
            UserAgent = userAgent,
            Details = details,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
