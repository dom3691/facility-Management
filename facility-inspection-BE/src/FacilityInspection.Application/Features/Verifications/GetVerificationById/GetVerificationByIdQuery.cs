using FacilityInspection.Application.DTOs.Verifications;
using MediatR;

namespace FacilityInspection.Application.Features.Verifications.GetVerificationById;

/// <summary>Returns a single verification, or 404 if it does not exist.</summary>
public record GetVerificationByIdQuery(Guid Id) : IRequest<VerificationResponse>;
