using FacilityInspection.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityInspection.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RecipientUserId)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(40);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.RecipientEmail)
            .HasMaxLength(256);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.RelatedEntityName)
            .HasMaxLength(100);

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(1000);

        // Common access patterns: a user's (unread) notifications, and the pending send queue.
        builder.HasIndex(x => new { x.RecipientUserId, x.IsRead });
        builder.HasIndex(x => x.Status);
    }
}
