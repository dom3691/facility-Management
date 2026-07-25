using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.Inspections;
using MediatR;

namespace FacilityInspection.Application.Features.Inspections.GetMyInspections;

/// <summary>Returns a page of inspections performed by the current user.</summary>
public record GetMyInspectionsQuery : IRequest<PaginatedResult<InspectionResponse>>
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
