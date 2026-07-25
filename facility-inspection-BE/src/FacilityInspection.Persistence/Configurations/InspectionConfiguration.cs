using FacilityInspection.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityInspection.Persistence.Configurations;

public class InspectionConfiguration : IEntityTypeConfiguration<Inspection>
{
    public void Configure(EntityTypeBuilder<Inspection> builder)
    {
        builder.ToTable("Inspections");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Classification)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.Findings)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.Recommendation)
            .HasMaxLength(1000);

        builder.Property(x => x.InspectorUserId)
            .IsRequired();

        builder.HasIndex(x => x.IncidentId);
        builder.HasIndex(x => x.InspectorUserId);

        // Inspection (1) ── (many) InspectionAttachment : cascade.
        builder.HasMany(x => x.Attachments)
            .WithOne(x => x.Inspection)
            .HasForeignKey(x => x.InspectionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Inspection (1) ── (many) VendorAssignment : restrict — assignments are part of the trail.
        builder.HasMany(x => x.VendorAssignments)
            .WithOne(x => x.Inspection)
            .HasForeignKey(x => x.InspectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
