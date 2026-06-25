using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrushEssence.Infrastructure.Persistence.Configurations;

public sealed class CustomRequestImageConfiguration : IEntityTypeConfiguration<CustomRequestImage>
{
    public void Configure(EntityTypeBuilder<CustomRequestImage> builder)
    {
        builder.ToTable("custom_request_images");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Url).IsRequired().HasMaxLength(2048);
        builder.Property(i => i.FileName).HasMaxLength(260);

        builder.HasIndex(i => i.CustomRequestId);
    }
}
