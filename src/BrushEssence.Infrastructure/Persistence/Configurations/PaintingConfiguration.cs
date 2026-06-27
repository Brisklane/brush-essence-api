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

        builder.Property(p => p.ImageUrl)
            .HasMaxLength(2048);

        // Cached rating aggregates, refreshed by the review service.
        builder.Property(p => p.AverageRating)
            .HasColumnType("numeric(3,2)");

        // Supports the storefront's default query: published paintings ordered
        // newest-first. Also covers filtering by IsPublished alone.
        builder.HasIndex(p => new { p.IsPublished, p.CreatedAt });

        // Supports price-range filtering and price sorting.
        builder.HasIndex(p => p.Price);

        // A painting optionally belongs to one category; deleting a category
        // leaves its paintings in place but uncategorized (FK set to null).
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Paintings)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(p => p.CategoryId);

        // A painting optionally has one medium; deleting a medium leaves its
        // paintings in place but without a medium (FK set to null).
        builder.HasOne(p => p.Medium)
            .WithMany(m => m.Paintings)
            .HasForeignKey(p => p.MediumId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(p => p.MediumId);
    }
}
