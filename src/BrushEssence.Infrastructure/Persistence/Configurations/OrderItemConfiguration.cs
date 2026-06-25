using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrushEssence.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Title).IsRequired().HasMaxLength(200);
        builder.Property(i => i.ImageUrl).HasMaxLength(2048);
        builder.Property(i => i.UnitPrice).HasColumnType("numeric(18,2)");
        builder.Property(i => i.LineTotal).HasColumnType("numeric(18,2)");
        builder.Property(i => i.Quantity).IsRequired();

        // Deleting a painting nulls the reference but preserves the purchase
        // snapshot (title/price/image), so order history stays intact.
        builder.HasOne(i => i.Painting)
            .WithMany()
            .HasForeignKey(i => i.PaintingId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(i => i.OrderId);
    }
}
