using FacilityInspection.Domain.Entities;

namespace FacilityInspection.Application.Common.Interfaces.Repositories;

/// <summary>Repository for the <see cref="Verification"/> aggregate.</summary>
public interface IVerificationRepository : IRepository<Verification>
{
    /// <summary>Loads a verification with its work order and incident.</summary>
    Task<Verification?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Returns all verifications for a work order (newest first).</summary>
    Task<IReadOnlyList<Verification>> GetByWorkOrderIdAsync(
        Guid workOrderId,
        CancellationToken cancellationToken = default);
}
