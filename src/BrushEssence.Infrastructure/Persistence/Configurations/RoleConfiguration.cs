using BrushEssence.Domain.Common;
using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrushEssence.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    // Fixed ids so the seed is stable across migrations/environments.
    public static readonly Guid AdminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid CustomerRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    // Fixed timestamp keeps the seed migration deterministic.
    private static readonly DateTimeOffset SeedTimestamp =
        new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name).IsRequired().HasMaxLength(64);
        builder.HasIndex(r => r.Name).IsUnique();

        builder.HasData(
            new Role { Id = AdminRoleId, Name = Roles.Admin, CreatedAt = SeedTimestamp },
            new Role { Id = CustomerRoleId, Name = Roles.Customer, CreatedAt = SeedTimestamp });
    }
}
