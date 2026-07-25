using FacilityInspection.Domain.Entities;

namespace FacilityInspection.Application.Common.Interfaces.Repositories;

/// <summary>Repository for the <see cref="VendorUpdate"/> progress log.</summary>
public interface IVendorUpdateRepository : IRepository<VendorUpdate>
{
    /// <summary>Loads a vendor update with its attachments.</summary>
    Task<VendorUpdate?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Returns all updates for a work order (with attachments), newest first.</summary>
    Task<IReadOnlyList<VendorUpdate>> GetByWorkOrderIdAsync(
        Guid workOrderId,
        CancellationToken cancellationToken = default);
}
