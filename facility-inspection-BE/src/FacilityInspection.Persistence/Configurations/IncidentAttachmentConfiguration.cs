using FacilityInspection.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityInspection.Persistence.Configurations;

public class IncidentAttachmentConfiguration : IEntityTypeConfiguration<IncidentAttachment>
{
    public void Configure(EntityTypeBuilder<IncidentAttachment> builder)
    {
        builder.ToTable("IncidentAttachments");

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

        builder.HasIndex(x => x.IncidentId);

        // Relationship is configured from IncidentConfiguration (principal side).
    }
}
