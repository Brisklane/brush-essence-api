using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrushEssence.Infrastructure.Persistence.Configurations;

public sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("cart_items");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Quantity).IsRequired();

        // One line per painting in a given cart; "add again" bumps the quantity.
        builder.HasIndex(i => new { i.CartId, i.PaintingId }).IsUnique();

        // If a painting is removed from the catalogue, drop it from every cart.
        builder.HasOne(i => i.Painting)
            .WithMany()
            .HasForeignKey(i => i.PaintingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
