using BrushEssence.Application.Admin;
using BrushEssence.Application.CustomRequests;
using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Common.Interfaces;

public interface ICustomRequestRepository
{
    Task AddAsync(CustomRequest request, CancellationToken cancellationToken = default);

    /// <summary>Paged, filtered admin listing across all users (no tracking).</summary>
    Task<(IReadOnlyList<AdminCustomRequestListItemDto> Items, int TotalCount)> GetPagedForAdminAsync(
        AdminCustomRequestQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a request with its images and status history (tracked) for reading
    /// or mutation.
    /// </summary>
    Task<CustomRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Compact, no-tracking summaries for a user's requests (newest first).</summary>
    Task<IReadOnlyList<CustomRequestSummaryDto>> GetSummariesByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
