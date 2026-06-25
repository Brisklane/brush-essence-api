using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrushEssence.Infrastructure.Persistence.Configurations;

public sealed class CustomRequestStatusEventConfiguration : IEntityTypeConfiguration<CustomRequestStatusEvent>
{
    public void Configure(EntityTypeBuilder<CustomRequestStatusEvent> builder)
    {
        builder.ToTable("custom_request_status_events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Note).HasMaxLength(1000);

        builder.HasIndex(e => e.CustomRequestId);
    }
}
