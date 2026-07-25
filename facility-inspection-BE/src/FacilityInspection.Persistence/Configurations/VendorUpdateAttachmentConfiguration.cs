using FacilityInspection.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityInspection.Persistence.Configurations;

public class VendorUpdateAttachmentConfiguration : IEntityTypeConfiguration<VendorUpdateAttachment>
{
    public void Configure(EntityTypeBuilder<VendorUpdateAttachment> builder)
    {
        builder.ToTable("VendorUpdateAttachments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(x => x.StoragePath)
            .IsRequired()
            .HasMaxLength(1024);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.VendorUpdateId);

        // Relationship is configured from VendorUpdateConfiguration (principal side).
    }
}
