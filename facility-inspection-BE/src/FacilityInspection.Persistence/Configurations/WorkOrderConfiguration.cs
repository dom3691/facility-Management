using FacilityInspection.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityInspection.Persistence.Configurations;

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ToTable("WorkOrders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.WorkOrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.HasIndex(x => x.WorkOrderNumber).IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.IncidentId);
        builder.HasIndex(x => x.VendorId);

        // WorkOrder (many) ── (1) Incident : restrict.
        builder.HasOne(x => x.Incident)
            .WithMany()
            .HasForeignKey(x => x.IncidentId)
            .OnDelete(DeleteBehavior.Restrict);

        // WorkOrder (many) ── (1) Vendor : restrict.
        builder.HasOne(x => x.Vendor)
            .WithMany()
            .HasForeignKey(x => x.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        // WorkOrder (1) ── (many) VendorUpdate : cascade — progress log belongs to the work order.
        builder.HasMany(x => x.Updates)
            .WithOne(x => x.WorkOrder)
            .HasForeignKey(x => x.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // WorkOrder (1) ── (many) Verification : restrict — verifications are retained sign-offs
        // (a NotFixed decision can be followed by re-verification after rework).
        builder.HasMany(x => x.Verifications)
            .WithOne(x => x.WorkOrder)
            .HasForeignKey(x => x.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // The VendorAssignment (1)──(0..1) WorkOrder relationship (with a unique index on
        // VendorAssignmentId that also prevents duplicate work orders) is configured from
        // VendorAssignmentConfiguration.
    }
}
