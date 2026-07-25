using FacilityInspection.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityInspection.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EntityName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.EntityId)
            .HasMaxLength(128);

        builder.Property(x => x.Action)
            .IsRequired()
            .HasMaxLength(100);

        // Serialized change sets — can be large (nvarchar(max)).
        builder.Property(x => x.OldValues);
        builder.Property(x => x.NewValues);

        builder.Property(x => x.PerformedByName)
            .HasMaxLength(256);

        builder.Property(x => x.IpAddress)
            .HasMaxLength(64);

        builder.Property(x => x.UserAgent)
            .HasMaxLength(512);

        builder.HasIndex(x => new { x.EntityName, x.EntityId });
        builder.HasIndex(x => x.PerformedDate);
        builder.HasIndex(x => x.PerformedByUserId);
    }
}
