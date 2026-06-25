using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrushEssence.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(rt => rt.Id);

        // SHA-256 hex is 64 chars; index for fast lookup by hash.
        builder.Property(rt => rt.TokenHash).IsRequired().HasMaxLength(128);
        builder.HasIndex(rt => rt.TokenHash).IsUnique();

        builder.Property(rt => rt.ReplacedByTokenHash).HasMaxLength(128);
        builder.Property(rt => rt.CreatedByIp).HasMaxLength(64);

        builder.HasIndex(rt => rt.UserId);
    }
}
