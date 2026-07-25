using FacilityInspection.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityInspection.Persistence.Configurations;

public class VendorUpdateConfiguration : IEntityTypeConfiguration<VendorUpdate>
{
    public void Configure(EntityTypeBuilder<VendorUpdate> builder)
    {
        builder.ToTable("VendorUpdates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UpdatedByUserId)
            .IsRequired();

        builder.Property(x => x.StatusAtUpdate)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.Property(x => x.CompletionComment)
            .HasMaxLength(2000);

        builder.HasIndex(x => x.WorkOrderId);

        // VendorUpdate (1) ── (many) VendorUpdateAttachment : cascade.
        builder.HasMany(x => x.Attachments)
            .WithOne(x => x.VendorUpdate)
            .HasForeignKey(x => x.VendorUpdateId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship to WorkOrder is configured from WorkOrderConfiguration (principal side).
    }
}
