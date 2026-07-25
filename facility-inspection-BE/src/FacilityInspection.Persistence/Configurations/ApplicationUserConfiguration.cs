using FacilityInspection.Persistence.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityInspection.Persistence.Configurations;

/// <summary>
/// Configures the custom fields added to the Identity user. Identity's own schema
/// (table names, base columns) is applied by <c>base.OnModelCreating</c>.
/// </summary>
public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.SAPId)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.SAPId).IsUnique();
    }
}
