using FacilityInspection.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityInspection.Persistence.Configurations;

public class FacilityConfiguration : IEntityTypeConfiguration<Facility>
{
    public void Configure(EntityTypeBuilder<Facility> builder)
    {
        builder.ToTable("Facilities");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.HasIndex(x => x.Code).IsUnique();

        // Facility (1) ── (many) Location : restrict — locations are retained reference data.
        builder.HasMany(x => x.Locations)
            .WithOne(x => x.Facility)
            .HasForeignKey(x => x.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
