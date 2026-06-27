using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.CustomRequests;

/// <summary>Explicit, dependency-free mapping for custom requests.</summary>
public static class CustomRequestMappings
{
    public static CustomRequestDto ToDto(this CustomRequest request) => new()
    {
        Id = request.Id,
        CustomerEmail = request.CustomerEmail,
        Title = request.Title,
        Description = request.Description,
        PreferredSize = request.PreferredSize,
        QuoteAmount = request.QuoteAmount,
        Currency = request.Currency,
        Status = request.Status,
        IsEditable = request.IsEditable,
        Images = request.Images
            .OrderBy(image => image.CreatedAt)
            .Select(image => new CustomRequestImageDto
            {
                Id = image.Id,
                Url = image.Url,
                FileName = image.FileName,
            })
            .ToList(),
        History = request.StatusHistory
            .OrderBy(e => e.CreatedAt)
            .Select(e => new CustomRequestStatusEventDto
            {
                Status = e.Status,
                Note = e.Note,
                OccurredAt = e.CreatedAt,
            })
            .ToList(),
        CreatedAt = request.CreatedAt,
    };

    public static CustomRequestImage ToEntity(this CustomRequestImageInput input) => new()
    {
        Url = input.Url.Trim(),
        FileName = string.IsNullOrWhiteSpace(input.FileName) ? null : input.FileName.Trim(),
    };
}
