using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrushEssence.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(32);
        builder.HasIndex(o => o.OrderNumber).IsUnique();

        builder.Property(o => o.CustomerEmail).IsRequired().HasMaxLength(256);

        // Persist the status as its readable name, not a brittle integer.
        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.Currency).IsRequired().HasMaxLength(3);
        builder.Property(o => o.Subtotal).HasColumnType("numeric(18,2)");
        builder.Property(o => o.ShippingCost).HasColumnType("numeric(18,2)");
        builder.Property(o => o.Total).HasColumnType("numeric(18,2)");

        // Shipping address is an owned value object stored in the same table.
        builder.OwnsOne(o => o.ShippingAddress, address =>
        {
            address.Property(a => a.FullName).HasColumnName("ShipFullName").IsRequired().HasMaxLength(200);
            address.Property(a => a.Line1).HasColumnName("ShipLine1").IsRequired().HasMaxLength(200);
            address.Property(a => a.Line2).HasColumnName("ShipLine2").HasMaxLength(200);
            address.Property(a => a.City).HasColumnName("ShipCity").IsRequired().HasMaxLength(120);
            address.Property(a => a.Region).HasColumnName("ShipRegion").HasMaxLength(120);
            address.Property(a => a.PostalCode).HasColumnName("ShipPostalCode").IsRequired().HasMaxLength(20);
            address.Property(a => a.Country).HasColumnName("ShipCountry").IsRequired().HasMaxLength(100);
            address.Property(a => a.Phone).HasColumnName("ShipPhone").HasMaxLength(40);
        });
        builder.Navigation(o => o.ShippingAddress).IsRequired();

        // Orders are financial records: keep them even if the user is removed.
        builder.HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Supports the history query: a user's orders, newest first.
        builder.HasIndex(o => new { o.UserId, o.CreatedAt });

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order!)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.StatusHistory)
            .WithOne(e => e.Order!)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
