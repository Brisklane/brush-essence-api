using BrushEssence.Application.Admin;
using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrushEssence.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public async Task<(IReadOnlyList<AdminUserDto> Items, int TotalCount)> GetPagedAsync(
        AdminUserQuery query,
        CancellationToken cancellationToken = default)
    {
        var queryable = context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            queryable = queryable.Where(u =>
                EF.Functions.ILike(u.Email, term) ||
                (u.FullName != null && EF.Functions.ILike(u.FullName, term)));
        }

        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            queryable = queryable.Where(u => u.UserRoles.Any(ur => ur.Role!.Name == query.Role));
        }

        if (query.IsActive is { } isActive)
        {
            queryable = queryable.Where(u => u.IsActive == isActive);
        }

        var totalCount = await queryable.CountAsync(cancellationToken);

        var items = await queryable
            .OrderByDescending(u => u.CreatedAt)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                IsActive = u.IsActive,
                IsEmailVerified = u.IsEmailVerified,
                Roles = u.UserRoles.Select(ur => ur.Role!.Name).ToList(),
                LastLoginAt = u.LastLoginAt,
                CreatedAt = u.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

    public Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        context.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        await context.Users.AddAsync(user, cancellationToken);
}
