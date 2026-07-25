using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Files;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.Features.Inspections.Common;
using FacilityInspection.Application.Features.VendorUpdates.Common;
using FacilityInspection.Domain.Entities;

namespace FacilityInspection.Application.Features.Files.Common;

/// <summary>
/// Resolves an attachment by id across the incident/inspection/vendor-update tables and
/// enforces that the current user may view the parent record (Admin sees all). Reuses the
/// existing module authorization rules.
/// </summary>
public static class FileAccessResolver
{
    public static async Task<FileAttachmentInfo> ResolveAsync(
        Guid fileId,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IIdentityService identityService,
        CancellationToken cancellationToken)
    {
        // Incident attachment.
        var incidentAttachment = await unitOfWork.Repository<IncidentAttachment>()
            .GetByIdAsync(fileId, cancellationToken);
        if (incidentAttachment is not null)
        {
            var incident = await unitOfWork.Incidents.GetByIdAsync(incidentAttachment.IncidentId, cancellationToken)
                ?? throw new NotFoundException(nameof(Incident), incidentAttachment.IncidentId);

            EnsureCanViewIncident(incident, currentUser);
            return ToInfo(incidentAttachment.Id, incidentAttachment.FileName, incidentAttachment.ContentType,
                incidentAttachment.FileSizeBytes, incidentAttachment.StoragePath, FileModules.Incidents);
        }

        // Inspection attachment.
        var inspectionAttachment = await unitOfWork.Repository<InspectionAttachment>()
            .GetByIdAsync(fileId, cancellationToken);
        if (inspectionAttachment is not null)
        {
            var inspection = await unitOfWork.Inspections
                .GetByIdWithDetailsAsync(inspectionAttachment.InspectionId, cancellationToken)
                ?? throw new NotFoundException(nameof(Inspection), inspectionAttachment.InspectionId);

            InspectionAuthorization.EnsureCanView(inspection, currentUser);
            return ToInfo(inspectionAttachment.Id, inspectionAttachment.FileName, inspectionAttachment.ContentType,
                inspectionAttachment.FileSizeBytes, inspectionAttachment.StoragePath, FileModules.Inspections);
        }

        // Vendor-update attachment.
        var vendorUpdateAttachment = await unitOfWork.Repository<VendorUpdateAttachment>()
            .GetByIdAsync(fileId, cancellationToken);
        if (vendorUpdateAttachment is not null)
        {
            var update = await unitOfWork.VendorUpdates.GetByIdAsync(vendorUpdateAttachment.VendorUpdateId, cancellationToken)
                ?? throw new NotFoundException(nameof(VendorUpdate), vendorUpdateAttachment.VendorUpdateId);
            var workOrder = await unitOfWork.WorkOrders.GetByIdAsync(update.WorkOrderId, cancellationToken)
                ?? throw new NotFoundException(nameof(WorkOrder), update.WorkOrderId);

            await VendorUpdateAuthorization.EnsureCanAccessWorkOrderAsync(
                workOrder, currentUser, identityService, cancellationToken);
            return ToInfo(vendorUpdateAttachment.Id, vendorUpdateAttachment.FileName, vendorUpdateAttachment.ContentType,
                vendorUpdateAttachment.FileSizeBytes, vendorUpdateAttachment.StoragePath, FileModules.VendorUpdates);
        }

        throw new NotFoundException("File", fileId);
    }

    private static void EnsureCanViewIncident(Incident incident, ICurrentUserService currentUser)
    {
        if (currentUser.Roles.Contains(AppRoles.Admin) || currentUser.Roles.Contains(AppRoles.Inspector))
        {
            return;
        }

        if (Guid.TryParse(currentUser.UserId, out var userId) && incident.ReportedByUserId == userId)
        {
            return;
        }

        throw new ForbiddenAccessException("You do not have permission to access this file.");
    }

    private static FileAttachmentInfo ToInfo(
        Guid id, string fileName, string contentType, long size, string storagePath, string module)
        => new(id, fileName, contentType, size, storagePath, module);
}
