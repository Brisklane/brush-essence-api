using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Common.Interfaces;

/// <summary>
/// Data access for <see cref="User"/> aggregates. Read methods eager-load roles
/// where the caller needs them (login, profile). Persistence is committed via
/// <see cref="IUnitOfWork"/>.
/// </summary>
public interface IUserRepository
{
    /// <summary>Finds a user by normalized email, including roles.</summary>
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    /// <summary>Finds a user by id, including roles.</summary>
    Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);
}
