using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrushEssence.Infrastructure.Persistence.Configurations;

public sealed class MediumConfiguration : IEntityTypeConfiguration<Medium>
{
    public void Configure(EntityTypeBuilder<Medium> builder)
    {
        builder.ToTable("mediums");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(m => m.Name).IsUnique();
    }
}
