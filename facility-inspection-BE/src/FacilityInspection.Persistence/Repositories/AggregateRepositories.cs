using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Domain.Entities;
using FacilityInspection.Persistence.Context;

namespace FacilityInspection.Persistence.Repositories;

// Concrete repositories for the main aggregates. They inherit all behaviour from
// Repository<TEntity>; entity-specific queries are added to each as features are built.

public class FacilityRepository : Repository<Facility>, IFacilityRepository
{
    public FacilityRepository(ApplicationDbContext context) : base(context) { }
}

// IncidentRepository has entity-specific queries and lives in its own file.

// InspectionRepository has entity-specific queries and lives in its own file.

// VendorRepository has entity-specific queries and lives in its own file.

// VendorAssignmentRepository has entity-specific queries and lives in its own file.

// WorkOrderRepository has entity-specific queries and lives in its own file.

// VerificationRepository has entity-specific queries and lives in its own file.
