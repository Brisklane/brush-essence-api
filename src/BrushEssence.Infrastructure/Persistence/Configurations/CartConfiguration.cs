using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrushEssence.Infrastructure.Persistence.Configurations;

public sealed class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("carts");

        builder.HasKey(c => c.Id);

        // At most one cart per signed-in user, and one per guest token. Partial
        // unique indexes enforce this while still allowing the "other" key to be
        // null on any given row.
        builder.HasIndex(c => c.UserId)
            .IsUnique()
            .HasFilter("\"UserId\" IS NOT NULL");

        builder.HasIndex(c => c.AnonymousId)
            .IsUnique()
            .HasFilter("\"AnonymousId\" IS NOT NULL");

        // Deleting a user removes their cart (and its items, via the cascade below).
        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Items)
            .WithOne(i => i.Cart!)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
