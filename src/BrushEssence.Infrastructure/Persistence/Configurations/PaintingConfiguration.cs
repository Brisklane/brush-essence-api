using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrushEssence.Infrastructure.Persistence.Configurations;

public class PaintingConfiguration : IEntityTypeConfiguration<Painting>
{
    public void Configure(EntityTypeBuilder<Painting> builder)
    {
        builder.ToTable("paintings");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(4000);

        builder.Property(p => p.Price)
            .HasColumnType("numeric(18,2)");

        builder.Property(p => p.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(p => p.Medium)
            .HasMaxLength(100);

        builder.Property(p => p.ImageUrl)
            .HasMaxLength(2048);

        builder.HasIndex(p => p.IsPublished);
    }
}
