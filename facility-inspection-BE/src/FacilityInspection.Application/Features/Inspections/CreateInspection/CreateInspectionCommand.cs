using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.Inspections;
using FacilityInspection.Domain.Enums;
using MediatR;

namespace FacilityInspection.Application.Features.Inspections.CreateInspection;

/// <summary>
/// Command to record an inspection against a pending incident. The inspector is resolved
/// from the current authenticated context; attachments (photo evidence) are optional.
/// </summary>
public record CreateInspectionCommand : IRequest<InspectionResponse>
{
    public Guid IncidentId { get; init; }

    public InspectionClassification Classification { get; init; }

    public string Comments { get; init; } = string.Empty;

    public IReadOnlyList<FileUpload> Attachments { get; init; } = Array.Empty<FileUpload>();
}
