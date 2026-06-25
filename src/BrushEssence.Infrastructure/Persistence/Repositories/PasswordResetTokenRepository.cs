using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrushEssence.Infrastructure.Persistence.Repositories;

public sealed class PasswordResetTokenRepository(ApplicationDbContext context)
    : IPasswordResetTokenRepository
{
    public Task<PasswordResetToken?> GetByHashWithUserAsync(
        string tokenHash,
        CancellationToken cancellationToken = default) =>
        context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

    public async Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default) =>
        await context.PasswordResetTokens.AddAsync(token, cancellationToken);
}
