using FacilityInspection.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityInspection.Persistence.Configurations;

public class InspectionAttachmentConfiguration : IEntityTypeConfiguration<InspectionAttachment>
{
    public void Configure(EntityTypeBuilder<InspectionAttachment> builder)
    {
        builder.ToTable("InspectionAttachments");

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

        builder.HasIndex(x => x.InspectionId);

        // Relationship is configured from InspectionConfiguration (principal side).
    }
}
