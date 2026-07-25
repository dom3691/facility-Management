namespace FacilityInspection.Application.DTOs.Verifications;

/// <summary>Verification detail returned by the verification endpoints.</summary>
public record VerificationResponse
{
    public Guid Id { get; init; }

    public Guid WorkOrderId { get; init; }

    public string? WorkOrderNumber { get; init; }

    public Guid IncidentId { get; init; }

    public string? IncidentNumber { get; init; }

    public Guid VerifiedByUserId { get; init; }

    public DateTimeOffset VerificationDate { get; init; }

    public string Decision { get; init; } = string.Empty;

    public string? Comments { get; init; }

    /// <summary>The work order's status after this verification.</summary>
    public string? WorkOrderStatus { get; init; }

    /// <summary>The incident's status after this verification.</summary>
    public string? IncidentStatus { get; init; }

    public DateTimeOffset CreatedDate { get; init; }
}
