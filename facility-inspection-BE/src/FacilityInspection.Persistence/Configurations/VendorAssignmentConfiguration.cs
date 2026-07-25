using FacilityInspection.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityInspection.Persistence.Configurations;

public class VendorAssignmentConfiguration : IEntityTypeConfiguration<VendorAssignment>
{
    public void Configure(EntityTypeBuilder<VendorAssignment> builder)
    {
        builder.ToTable("VendorAssignments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.VendorCategory)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.AssignedByUserId)
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(x => x.IncidentId);
        builder.HasIndex(x => x.InspectionId);
        builder.HasIndex(x => x.VendorId);
        builder.HasIndex(x => x.AssignedByUserId);

        // VendorAssignment (many) ── (1) Incident : restrict.
        builder.HasOne(x => x.Incident)
            .WithMany()
            .HasForeignKey(x => x.IncidentId)
            .OnDelete(DeleteBehavior.Restrict);

        // VendorAssignment (1) ── (0..1) WorkOrder : one work order per assignment.
        // FK + unique index live on WorkOrder; restrict so a work order is not lost with its assignment.
        builder.HasOne(x => x.WorkOrder)
            .WithOne(x => x.VendorAssignment)
            .HasForeignKey<WorkOrder>(x => x.VendorAssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationships to Inspection and Vendor are configured from their principal configurations.
    }
}
