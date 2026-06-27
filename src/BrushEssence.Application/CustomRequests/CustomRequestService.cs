using BrushEssence.Application.Admin;
using BrushEssence.Application.Common.Exceptions;
using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Application.Common.Models;
using BrushEssence.Domain.Common;
using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.CustomRequests;

public sealed class CustomRequestService(
    ICustomRequestRepository requests,
    ICurrentUser currentUser,
    IFileStorageService fileStorage,
    IAuditLogger auditLogger,
    IUnitOfWork unitOfWork) : ICustomRequestService
{
    public async Task<CustomRequestDto> CreateAsync(
        CreateCustomRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireUserId();

        var entity = new CustomRequest
        {
            UserId = userId,
            CustomerEmail = currentUser.Email ?? string.Empty,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            PreferredSize = Clean(request.PreferredSize),
            Currency = StoreDefaults.Currency,
            Status = CustomRequestStatus.Submitted,
        };

        foreach (var image in request.Images)
        {
            entity.Images.Add(image.ToEntity());
        }

        entity.StatusHistory.Add(new CustomRequestStatusEvent
        {
            Status = CustomRequestStatus.Submitted,
            Note = "Request submitted.",
        });

        await requests.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToDto();
    }

    public async Task<PagedResult<AdminCustomRequestListItemDto>> GetAllAsync(
        AdminCustomRequestQuery query,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await requests.GetPagedForAdminAsync(query, cancellationToken);

        return new PagedResult<AdminCustomRequestListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<IReadOnlyList<CustomRequestSummaryDto>> GetMyRequestsAsync(
        CancellationToken cancellationToken = default)
        => await requests.GetSummariesByUserAsync(RequireUserId(), cancellationToken);

    public async Task<CustomRequestDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => (await LoadOwnedRequestAsync(id, cancellationToken)).ToDto();

    public async Task<CustomRequestDto> UpdateAsync(
        Guid id,
        UpdateCustomRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadOwnedRequestAsync(id, cancellationToken);

        if (!entity.IsEditable)
        {
            throw new BadRequestException(
                "This request can no longer be edited because it is already being handled.");
        }

        entity.Title = request.Title.Trim();
        entity.Description = request.Description.Trim();
        entity.PreferredSize = Clean(request.PreferredSize);
        entity.Currency = StoreDefaults.Currency;

        var orphanedUrls = ReconcileImages(entity, request.Images);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Best-effort cleanup of files no longer referenced by any image.
        foreach (var url in orphanedUrls)
        {
            await fileStorage.DeleteAsync(url, cancellationToken);
        }

        return entity.ToDto();
    }

    public async Task<CustomRequestDto> UpdateStatusAsync(
        Guid id,
        UpdateCustomRequestStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await requests.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Custom request not found.");

        if (!CustomRequestStatusWorkflow.CanTransition(entity.Status, request.Status))
        {
            throw new BadRequestException(
                $"A request cannot move from {entity.Status} to {request.Status}.");
        }

        var previousStatus = entity.Status;
        entity.TransitionTo(request.Status, Clean(request.Note));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        auditLogger.LogAction("CustomRequestStatusChanged", "CustomRequest", entity.Id,
            new { From = previousStatus.ToString(), To = request.Status.ToString() });

        return entity.ToDto();
    }

    public async Task<CustomRequestDto> SetQuoteAsync(
        Guid id,
        SetCustomRequestQuoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await requests.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Custom request not found.");

        if (!CustomRequestStatusWorkflow.CanTransition(entity.Status, CustomRequestStatus.Quoted))
        {
            throw new BadRequestException(
                $"A quote can't be sent for a request that is {entity.Status}.");
        }

        entity.QuoteAmount = request.Amount;
        var note = Clean(request.Note) is { } n
            ? $"Quote sent: {request.Amount:0.##} {entity.Currency}. {n}"
            : $"Quote sent: {request.Amount:0.##} {entity.Currency}.";
        entity.TransitionTo(CustomRequestStatus.Quoted, note);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        auditLogger.LogAction("CustomRequestQuoted", "CustomRequest", entity.Id,
            new { request.Amount });

        return entity.ToDto();
    }

    public async Task<CustomRequestDto> ApproveQuoteAsync(Guid id, CancellationToken cancellationToken = default)
        => await RespondToQuoteAsync(id, CustomRequestStatus.InProgress, "Quote approved by customer.", cancellationToken);

    public async Task<CustomRequestDto> DeclineQuoteAsync(Guid id, CancellationToken cancellationToken = default)
        => await RespondToQuoteAsync(id, CustomRequestStatus.Declined, "Quote declined by customer.", cancellationToken);

    private async Task<CustomRequestDto> RespondToQuoteAsync(
        Guid id,
        CustomRequestStatus target,
        string note,
        CancellationToken cancellationToken)
    {
        var entity = await LoadOwnedRequestAsync(id, cancellationToken);

        if (entity.Status != CustomRequestStatus.Quoted)
        {
            throw new BadRequestException("There is no pending quote to respond to.");
        }

        entity.TransitionTo(target, note);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        auditLogger.LogAction("CustomRequestQuoteResponse", "CustomRequest", entity.Id,
            new { Response = target.ToString() });

        return entity.ToDto();
    }

    // ----- Helpers -----

    private Guid RequireUserId()
        => currentUser.UserId
            ?? throw new AuthenticationException("You must be signed in to manage custom requests.");

    private bool IsAdmin => currentUser.Roles.Contains(Roles.Admin);

    private async Task<CustomRequest> LoadOwnedRequestAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await requests.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Custom request not found.");

        if (entity.UserId != RequireUserId() && !IsAdmin)
        {
            // Don't reveal that someone else's request exists.
            throw new NotFoundException("Custom request not found.");
        }

        return entity;
    }

    /// <summary>
    /// Syncs the request's images to the desired set: removes dropped ones and
    /// adds new ones, preserving images that stay. Returns the URLs of files that
    /// are no longer referenced and can be deleted from storage.
    /// </summary>
    private static IReadOnlyList<string> ReconcileImages(
        CustomRequest entity,
        IReadOnlyList<CustomRequestImageInput> desired)
    {
        var desiredUrls = desired.Select(i => i.Url.Trim()).ToHashSet();
        var existingUrls = entity.Images.Select(i => i.Url).ToHashSet();

        var removed = entity.Images.Where(i => !desiredUrls.Contains(i.Url)).ToList();
        foreach (var image in removed)
        {
            entity.Images.Remove(image);
        }

        foreach (var input in desired.Where(i => !existingUrls.Contains(i.Url.Trim())))
        {
            entity.Images.Add(input.ToEntity());
        }

        return removed.Select(i => i.Url).ToList();
    }

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
