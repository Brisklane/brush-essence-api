using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrushEssence.Infrastructure.Persistence.Repositories;

public sealed class RoleRepository(ApplicationDbContext context) : IRoleRepository
{
    public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        context.Roles.FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
}
