namespace BrushEssence.Application.Common.Interfaces;

/// <summary>
/// Records security/compliance-relevant actions (order changes, admin actions)
/// to the structured log, automatically attributed to the current caller. Kept
/// as an abstraction so the sink can change (log, table, SIEM) without touching
/// callers.
/// </summary>
public interface IAuditLogger
{
    /// <summary>
    /// Logs an audited action against an entity, with optional structured detail.
    /// </summary>
    /// <param name="action">Verb-ish action name, e.g. "OrderStatusChanged".</param>
    /// <param name="entityType">The entity affected, e.g. "Order".</param>
    /// <param name="entityId">Identifier of the affected entity.</param>
    /// <param name="details">Optional structured detail (logged destructured).</param>
    void LogAction(string action, string entityType, object entityId, object? details = null);
}
