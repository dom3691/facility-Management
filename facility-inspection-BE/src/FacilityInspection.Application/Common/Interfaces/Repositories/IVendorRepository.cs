using FacilityInspection.Domain.Entities;
using FacilityInspection.Domain.Enums;

namespace FacilityInspection.Application.Common.Interfaces.Repositories;

/// <summary>Repository for the <see cref="Vendor"/> aggregate.</summary>
public interface IVendorRepository : IRepository<Vendor>
{
    /// <summary>Returns a page of vendors (optionally filtered by category/active), with the total count.</summary>
    Task<(IReadOnlyList<Vendor> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        VendorCategory? category,
        bool? isActive,
        CancellationToken cancellationToken = default);
}
