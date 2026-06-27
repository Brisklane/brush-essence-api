using BrushEssence.Application.Admin;
using BrushEssence.Application.Common.Models;

namespace BrushEssence.Application.CustomRequests;

/// <summary>
/// Custom painting request use cases. Create/read/edit act on the authenticated
/// caller's own requests; status changes are an administrative operation.
/// </summary>
public interface ICustomRequestService
{
    /// <summary>Admin: a paged, filterable list of every customer's requests.</summary>
    Task<PagedResult<AdminCustomRequestListItemDto>> GetAllAsync(AdminCustomRequestQuery query, CancellationToken cancellationToken = default);

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

    /// <summary>Admin: sends the customer a price quote and moves the request to Quoted.</summary>
    Task<CustomRequestDto> SetQuoteAsync(Guid id, SetCustomRequestQuoteRequest request, CancellationToken cancellationToken = default);

    /// <summary>Customer: approves the pending quote (moves to In progress).</summary>
    Task<CustomRequestDto> ApproveQuoteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Customer: declines the pending quote (moves to Declined).</summary>
    Task<CustomRequestDto> DeclineQuoteAsync(Guid id, CancellationToken cancellationToken = default);
}
