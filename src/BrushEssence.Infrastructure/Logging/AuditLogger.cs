using BrushEssence.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace BrushEssence.Infrastructure.Logging;

/// <summary>
/// Writes audit entries to the structured log, attributed to the current caller.
/// With the Serilog JSON sink these become queryable events (Action, EntityType,
/// EntityId, ActorId, …) for traceability and compliance.
/// </summary>
public sealed class AuditLogger(ILogger<AuditLogger> logger, ICurrentUser currentUser) : IAuditLogger
{
    public void LogAction(string action, string entityType, object entityId, object? details = null)
    {
        logger.LogInformation(
            "AUDIT {AuditAction} on {EntityType} {EntityId} by {ActorId} ({ActorEmail}) {@Details}",
            action,
            entityType,
            entityId,
            currentUser.UserId,
            currentUser.Email ?? "anonymous",
            details);
    }
}
