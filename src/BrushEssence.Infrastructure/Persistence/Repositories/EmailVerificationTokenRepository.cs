using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrushEssence.Infrastructure.Persistence.Repositories;

public sealed class EmailVerificationTokenRepository(ApplicationDbContext context)
    : IEmailVerificationTokenRepository
{
    public Task<EmailVerificationToken?> GetByHashWithUserAsync(
        string tokenHash,
        CancellationToken cancellationToken = default) =>
        context.EmailVerificationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

    public async Task AddAsync(
        EmailVerificationToken token,
        CancellationToken cancellationToken = default) =>
        await context.EmailVerificationTokens.AddAsync(token, cancellationToken);

    public async Task InvalidateActiveForUserAsync(
        Guid userId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        await context.EmailVerificationTokens
            .Where(t => t.UserId == userId && !t.IsUsed && t.ExpiresAt > now)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(t => t.IsUsed, true)
                    .SetProperty(t => t.UsedAt, now),
                cancellationToken);
    }
}
