using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.Incidents;
using MediatR;

namespace FacilityInspection.Application.Features.Incidents.CreateIncident;

/// <summary>
/// Command to log a new incident. The reporting user is resolved from the current
/// authenticated context in the handler; attachments are optional.
/// </summary>
public record CreateIncidentCommand : IRequest<IncidentResponse>
{
    public string BusinessUnit { get; init; } = string.Empty;

    public string SAPId { get; init; } = string.Empty;

    public Guid FacilityId { get; init; }

    public Guid LocationId { get; init; }

    public DateTimeOffset IncidentDate { get; init; }

    public string Description { get; init; } = string.Empty;

    public IReadOnlyList<FileUpload> Attachments { get; init; } = Array.Empty<FileUpload>();
}
