using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.Vendors;
using FacilityInspection.Domain.Enums;
using MediatR;

namespace FacilityInspection.Application.Features.Vendors.GetVendors;

/// <summary>Returns a page of vendors, optionally filtered by category and/or active flag.</summary>
public record GetVendorsQuery : IRequest<PaginatedResult<VendorResponse>>
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public VendorCategory? Category { get; init; }

    public bool? IsActive { get; init; }
}
