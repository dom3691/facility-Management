using System.Linq.Expressions;
using System.Reflection;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Domain.Common;
using FacilityInspection.Domain.Entities;
using FacilityInspection.Persistence.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FacilityInspection.Persistence.Context;

/// <summary>
/// The application's EF Core unit of work. Inherits <see cref="IdentityDbContext{TUser,TRole,TKey}"/>
/// so ASP.NET Core Identity tables live alongside the domain aggregates, and exposes the
/// <see cref="IApplicationDbContext"/> abstraction consumed by the Application layer.
/// </summary>
public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Reference data
    public DbSet<Facility> Facilities => Set<Facility>();
    public DbSet<Location> Locations => Set<Location>();

    // Workflow aggregates
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<IncidentAttachment> IncidentAttachments => Set<IncidentAttachment>();
    public DbSet<Inspection> Inspections => Set<Inspection>();
    public DbSet<InspectionAttachment> InspectionAttachments => Set<InspectionAttachment>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<VendorAssignment> VendorAssignments => Set<VendorAssignment>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<VendorUpdate> VendorUpdates => Set<VendorUpdate>();
    public DbSet<VendorUpdateAttachment> VendorUpdateAttachments => Set<VendorUpdateAttachment>();
    public DbSet<Verification> Verifications => Set<Verification>();

    // Cross-cutting
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Identity schema first, then our entity-specific configurations.
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        ApplyGlobalConventions(builder);
    }

    /// <summary>
    /// Cross-entity conventions applied after per-entity configuration:
    /// consistent audit-column lengths and a global soft-delete query filter.
    /// </summary>
    private static void ApplyGlobalConventions(ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (typeof(IAuditableEntity).IsAssignableFrom(clrType))
            {
                builder.Entity(clrType).Property(nameof(IAuditableEntity.CreatedBy)).HasMaxLength(256);
                builder.Entity(clrType).Property(nameof(IAuditableEntity.ModifiedBy)).HasMaxLength(256);
            }

            if (typeof(ISoftDelete).IsAssignableFrom(clrType))
            {
                builder.Entity(clrType).Property(nameof(ISoftDelete.DeletedBy)).HasMaxLength(256);
                builder.Entity(clrType).HasIndex(nameof(ISoftDelete.IsDeleted));

                // Global filter: exclude soft-deleted rows from all queries.
                var parameter = Expression.Parameter(clrType, "e");
                var body = Expression.Not(
                    Expression.Property(parameter, nameof(ISoftDelete.IsDeleted)));
                entityType.SetQueryFilter(Expression.Lambda(body, parameter));
            }
        }
    }
}
