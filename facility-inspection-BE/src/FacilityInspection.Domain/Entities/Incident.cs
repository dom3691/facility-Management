using FacilityInspection.Domain.Common;
using FacilityInspection.Domain.Enums;

namespace FacilityInspection.Domain.Entities;

/// <summary>
/// A reported facility problem — the entry point of the workflow. An incident is
/// inspected, and if remediation is required it flows to vendor assignment,
/// a work order, completion and finally verification.
/// </summary>
public class Incident : AuditableEntity, ISoftDelete
{
    /// <summary>Human-friendly unique reference, format <c>INC-yyyy-000001</c>.</summary>
    public string IncidentNumber { get; set; } = string.Empty;

    public string BusinessUnit { get; set; } = string.Empty;

    /// <summary>SAP identifier captured for the incident.</summary>
    public string SAPId { get; set; } = string.Empty;

    public Guid FacilityId { get; set; }

    public Guid LocationId { get; set; }

    /// <summary>Business date/time the incident occurred (distinct from when it was logged).</summary>
    public DateTimeOffset IncidentDate { get; set; }

    public string Description { get; set; } = string.Empty;

    public IncidentStatus Status { get; set; } = IncidentStatus.PendingInspection;

    /// <summary>Logical reference to the reporting user (AspNetUsers.Id).</summary>
    public Guid ReportedByUserId { get; set; }

    // Navigation
    public Facility Facility { get; set; } = null!;

    public Location Location { get; set; } = null!;

    public ICollection<IncidentAttachment> Attachments { get; set; } = new List<IncidentAttachment>();

    public ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTimeOffset? DeletedDate { get; set; }
}
