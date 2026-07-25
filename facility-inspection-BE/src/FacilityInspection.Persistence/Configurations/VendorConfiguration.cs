using FacilityInspection.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityInspection.Persistence.Configurations;

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("Vendors");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.ContactPerson)
            .HasMaxLength(150);

        builder.Property(x => x.ContactEmail)
            .HasMaxLength(256);

        builder.Property(x => x.ContactPhone)
            .HasMaxLength(50);

        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.Category);

        // Vendor (1) ── (many) VendorAssignment : restrict — never cascade-delete history via master data.
        builder.HasMany(x => x.VendorAssignments)
            .WithOne(x => x.Vendor)
            .HasForeignKey(x => x.VendorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
