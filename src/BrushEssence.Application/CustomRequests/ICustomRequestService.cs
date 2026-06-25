namespace BrushEssence.Application.CustomRequests;

/// <summary>
/// Custom painting request use cases. Create/read/edit act on the authenticated
/// caller's own requests; status changes are an administrative operation.
/// </summary>
public interface ICustomRequestService
{
    /// <summary>Submits a new custom request for the current user.</summary>
    Task<CustomRequestDto> CreateAsync(CreateCustomRequestRequest request, CancellationToken cancellationToken = default);

    /// <summary>The current user's requests, newest first (history list).</summary>
    Task<IReadOnlyList<CustomRequestSummaryDto>> GetMyRequestsAsync(CancellationToken cancellationToken = default);

    /// <summary>A single request's full details. Restricted to its owner (or an admin).</summary>
    Task<CustomRequestDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Edits a request the caller owns. Only allowed while the request is still
    /// editable (status Submitted); orphaned reference images are cleaned up.
    /// </summary>
    Task<CustomRequestDto> UpdateAsync(Guid id, UpdateCustomRequestRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Advances a request along its lifecycle (admin only). Enforces the allowed
    /// status transitions and records a timeline entry.
    /// </summary>
    Task<CustomRequestDto> UpdateStatusAsync(Guid id, UpdateCustomRequestStatusRequest request, CancellationToken cancellationToken = default);
}
