using FacilityInspection.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityInspection.Persistence.Configurations;

public class VerificationConfiguration : IEntityTypeConfiguration<Verification>
{
    public void Configure(EntityTypeBuilder<Verification> builder)
    {
        builder.ToTable("Verifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Decision)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.Remarks)
            .HasMaxLength(2000);

        builder.Property(x => x.VerifiedByUserId)
            .IsRequired();

        builder.HasIndex(x => x.VerifiedByUserId);
        builder.HasIndex(x => x.WorkOrderId);

        // The WorkOrder (1)──(many) Verification relationship is configured from WorkOrderConfiguration.
    }
}
