using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrushEssence.Infrastructure.Persistence.Configurations;

public sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.CustomerEmail).IsRequired().HasMaxLength(256);
        builder.Property(r => r.Rating).IsRequired();
        builder.Property(r => r.Title).HasMaxLength(150);
        builder.Property(r => r.Comment).HasMaxLength(2000);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // One review per customer per painting.
        builder.HasIndex(r => new { r.PaintingId, r.UserId }).IsUnique();

        // Supports the storefront query: a painting's approved reviews.
        builder.HasIndex(r => new { r.PaintingId, r.Status });

        builder.HasOne(r => r.Painting)
            .WithMany()
            .HasForeignKey(r => r.PaintingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
