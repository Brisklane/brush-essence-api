using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrushEssence.Infrastructure.Persistence.Configurations;

public sealed class CustomRequestConfiguration : IEntityTypeConfiguration<CustomRequest>
{
    public void Configure(EntityTypeBuilder<CustomRequest> builder)
    {
        builder.ToTable("custom_requests");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.CustomerEmail).IsRequired().HasMaxLength(256);
        builder.Property(r => r.Title).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Description).IsRequired().HasMaxLength(4000);
        builder.Property(r => r.PreferredSize).HasMaxLength(200);
        builder.Property(r => r.BudgetAmount).HasColumnType("numeric(18,2)");
        builder.Property(r => r.Currency).IsRequired().HasMaxLength(3);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Ignore(r => r.IsEditable);

        // Keep requests if the user is removed (they're a work record); the FK is
        // restricted rather than cascading.
        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Supports the history query: a user's requests, newest first.
        builder.HasIndex(r => new { r.UserId, r.CreatedAt });

        builder.HasMany(r => r.Images)
            .WithOne(i => i.CustomRequest!)
            .HasForeignKey(i => i.CustomRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.StatusHistory)
            .WithOne(e => e.CustomRequest!)
            .HasForeignKey(e => e.CustomRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
