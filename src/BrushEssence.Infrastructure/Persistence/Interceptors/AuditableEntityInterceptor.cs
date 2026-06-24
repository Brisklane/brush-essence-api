using BrushEssence.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BrushEssence.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Populates <see cref="IAuditableEntity"/> timestamps on save so callers never
/// have to set them by hand. Uses <see cref="TimeProvider"/> for testability.
/// </summary>
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly TimeProvider _timeProvider;

    public AuditableEntityInterceptor(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateTimestamps(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateTimestamps(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateTimestamps(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = _timeProvider.GetUtcNow();

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }
    }
}
