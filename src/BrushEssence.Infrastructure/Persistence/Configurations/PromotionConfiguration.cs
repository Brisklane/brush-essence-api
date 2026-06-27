using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrushEssence.Infrastructure.Persistence.Configurations;

public sealed class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.ToTable("promotions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(150);
        builder.Property(p => p.Value).HasColumnType("numeric(18,2)");

        builder.Property(p => p.DiscountType)
            .HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.Scope)
            .HasConversion<string>().HasMaxLength(20).IsRequired();

        // Deleting a category removes its category-scoped promotions.
        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.IsActive);
    }
}

public sealed class PromotionPaintingConfiguration : IEntityTypeConfiguration<PromotionPainting>
{
    public void Configure(EntityTypeBuilder<PromotionPainting> builder)
    {
        builder.ToTable("promotion_paintings");

        builder.HasKey(pp => new { pp.PromotionId, pp.PaintingId });

        builder.HasOne(pp => pp.Promotion)
            .WithMany(p => p.PromotionPaintings)
            .HasForeignKey(pp => pp.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pp => pp.Painting)
            .WithMany()
            .HasForeignKey(pp => pp.PaintingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pp => pp.PaintingId);
    }
}
