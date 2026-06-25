using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrushEssence.Infrastructure.Persistence.Configurations;

public sealed class OrderStatusEventConfiguration : IEntityTypeConfiguration<OrderStatusEvent>
{
    public void Configure(EntityTypeBuilder<OrderStatusEvent> builder)
    {
        builder.ToTable("order_status_events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Note).HasMaxLength(500);

        builder.HasIndex(e => e.OrderId);
    }
}
