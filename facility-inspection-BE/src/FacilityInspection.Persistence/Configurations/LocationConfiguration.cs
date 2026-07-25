using FacilityInspection.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityInspection.Persistence.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("Locations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Code)
            .HasMaxLength(50);

        builder.Property(x => x.FloorOrArea)
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.HasIndex(x => x.FacilityId);
        builder.HasIndex(x => new { x.FacilityId, x.Name });

        // Relationship to Facility is configured from FacilityConfiguration (principal side).
    }
}
