using MediatR;

namespace FacilityInspection.Application.Features.Vendors.DeleteVendor;

/// <summary>Soft-deletes a vendor.</summary>
public record DeleteVendorCommand(Guid Id) : IRequest;
