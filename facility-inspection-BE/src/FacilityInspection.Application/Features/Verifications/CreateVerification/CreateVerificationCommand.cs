using FacilityInspection.Application.DTOs.Verifications;
using FacilityInspection.Domain.Enums;
using MediatR;

namespace FacilityInspection.Application.Features.Verifications.CreateVerification;

/// <summary>Records a verification decision on a completed work order.</summary>
public record CreateVerificationCommand : IRequest<VerificationResponse>
{
    public Guid WorkOrderId { get; init; }

    public VerificationDecision Decision { get; init; }

    public string? Comments { get; init; }
}
