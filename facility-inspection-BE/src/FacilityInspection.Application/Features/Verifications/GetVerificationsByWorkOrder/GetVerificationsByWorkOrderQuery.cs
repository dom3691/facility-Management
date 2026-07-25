using FacilityInspection.Application.DTOs.Verifications;
using MediatR;

namespace FacilityInspection.Application.Features.Verifications.GetVerificationsByWorkOrder;

/// <summary>Returns all verifications for a work order.</summary>
public record GetVerificationsByWorkOrderQuery(Guid WorkOrderId)
    : IRequest<IReadOnlyList<VerificationResponse>>;
