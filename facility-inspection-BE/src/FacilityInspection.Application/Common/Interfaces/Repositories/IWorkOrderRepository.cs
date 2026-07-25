using FacilityInspection.Domain.Entities;
using FacilityInspection.Domain.Enums;

namespace FacilityInspection.Application.Common.Interfaces.Repositories;

/// <summary>Repository for the <see cref="WorkOrder"/> aggregate.</summary>
public interface IWorkOrderRepository : IRepository<WorkOrder>
{
    /// <summary>Counts work orders whose number starts with the prefix (ignores soft-delete filter).</summary>
    Task<int> CountByNumberPrefixAsync(string prefix, CancellationToken cancellationToken = default);

    /// <summary>True if a work order already exists for the given vendor assignment.</summary>
    Task<bool> ExistsForVendorAssignmentAsync(Guid vendorAssignmentId, CancellationToken cancellationToken = default);

    /// <summary>Loads a work order with incident and vendor for a detail view.</summary>
    Task<WorkOrder?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Returns all work orders for an incident (with vendor), newest first.</summary>
    Task<IReadOnlyList<WorkOrder>> GetByIncidentIdAsync(Guid incidentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a page of work orders (with incident/vendor), optionally filtered by status
    /// and/or vendor, newest first, with the total count.
    /// </summary>
    Task<(IReadOnlyList<WorkOrder> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        WorkOrderStatus? status,
        Guid? vendorId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a page of Completed work orders awaiting verification, optionally scoped to the
    /// incidents reported by a given initiator, with the total count.
    /// </summary>
    Task<(IReadOnlyList<WorkOrder> Items, int TotalCount)> GetPendingVerificationPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? initiatorUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns work-order counts grouped by status for a dashboard, optionally scoped to a
    /// <paramref name="vendorId"/> and/or the incidents reported by <paramref name="initiatorUserId"/>.
    /// Nulls return all.
    /// </summary>
    Task<IReadOnlyDictionary<WorkOrderStatus, int>> GetStatusCountsAsync(
        Guid? vendorId,
        Guid? initiatorUserId,
        CancellationToken cancellationToken = default);
}
