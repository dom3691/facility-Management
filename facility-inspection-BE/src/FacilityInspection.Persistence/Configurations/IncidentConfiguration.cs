using FacilityInspection.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityInspection.Persistence.Configurations;

public class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("Incidents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.IncidentNumber)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.BusinessUnit)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.SAPId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.ReportedByUserId)
            .IsRequired();

        builder.HasIndex(x => x.IncidentNumber).IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ReportedByUserId);
        builder.HasIndex(x => x.FacilityId);
        builder.HasIndex(x => x.LocationId);

        // Incident (many) ── (1) Facility : restrict — reference data is retained.
        builder.HasOne(x => x.Facility)
            .WithMany()
            .HasForeignKey(x => x.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);

        // Incident (many) ── (1) Location : restrict, and avoids multiple cascade paths.
        builder.HasOne(x => x.Location)
            .WithMany()
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Incident (1) ── (many) IncidentAttachment : cascade — attachments are owned by the incident.
        builder.HasMany(x => x.Attachments)
            .WithOne(x => x.Incident)
            .HasForeignKey(x => x.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Incident (1) ── (many) Inspection : restrict — inspections are part of the audit trail.
        builder.HasMany(x => x.Inspections)
            .WithOne(x => x.Incident)
            .HasForeignKey(x => x.IncidentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
