using FacilityInspection.Domain.Enums;

namespace FacilityInspection.Application.DTOs.Verifications;

/// <summary>Payload for <c>POST /api/verifications</c>.</summary>
public class CreateVerificationRequest
{
    public Guid WorkOrderId { get; set; }

    public VerificationDecision Decision { get; set; }

    /// <summary>Verification remarks. Required when the decision is <c>NotFixed</c>.</summary>
    public string? Comments { get; set; }
}
