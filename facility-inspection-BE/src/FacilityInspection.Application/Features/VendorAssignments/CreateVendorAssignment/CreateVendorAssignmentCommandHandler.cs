using FacilityInspection.Application.Common.Audit;
using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.VendorAssignments;
using FacilityInspection.Application.Features.VendorAssignments.Common;
using FacilityInspection.Domain.Entities;
using FacilityInspection.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FacilityInspection.Application.Features.VendorAssignments.CreateVendorAssignment;

/// <summary>
/// Assigns a vendor to an inspected incident, enforcing the workflow rules, transitioning
/// the incident to <see cref="IncidentStatus.VendorAssigned"/>, writing an audit-log entry
/// and queuing a notification — all in one transaction.
/// </summary>
public class CreateVendorAssignmentCommandHandler
    : IRequestHandler<CreateVendorAssignmentCommand, VendorAssignmentResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<CreateVendorAssignmentCommandHandler> _logger;

    public CreateVendorAssignmentCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IAuditService auditService,
        ICurrentUserService currentUser,
        IDateTimeProvider dateTime,
        ILogger<CreateVendorAssignmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _auditService = auditService;
        _currentUser = currentUser;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task<VendorAssignmentResponse> Handle(
        CreateVendorAssignmentCommand request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUser.UserId, out var assignedByUserId))
        {
            throw new ForbiddenAccessException("The current user could not be resolved.");
        }

        // Rule 1: incident must exist.
        var incident = await _unitOfWork.Incidents.GetByIdAsync(request.IncidentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Incident), request.IncidentId);

        // Rule 2: inspection must exist.
        var inspection = await _unitOfWork.Inspections.GetByIdAsync(request.InspectionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Inspection), request.InspectionId);

        if (inspection.IncidentId != incident.Id)
        {
            throw new BadRequestException("The inspection does not belong to the specified incident.");
        }

        // Rule 3: classification must not be Good.
        if (inspection.Classification == InspectionClassification.Good)
        {
            throw new BadRequestException("A vendor cannot be assigned to an incident inspected as Good.");
        }

        // Rule 4: incident must be inspection-completed (awaiting vendor assignment).
        if (incident.Status != IncidentStatus.AwaitingVendorAssignment)
        {
            throw new BadRequestException(
                $"Incident {incident.IncidentNumber} is not awaiting vendor assignment (status: {incident.Status}).");
        }

        // Rule 5: vendor must exist and be active.
        var vendor = await _unitOfWork.Vendors.GetByIdAsync(request.VendorId, cancellationToken)
            ?? throw new NotFoundException(nameof(Vendor), request.VendorId);

        if (!vendor.IsActive)
        {
            throw new BadRequestException($"Vendor '{vendor.Name}' is inactive and cannot be assigned.");
        }

        // Rule 6: vendor category must match the selected category.
        if (vendor.Category != request.VendorCategory)
        {
            throw new BadRequestException(
                $"Vendor category mismatch: '{vendor.Name}' is {vendor.Category}, not {request.VendorCategory}.");
        }

        var assignment = new VendorAssignment
        {
            IncidentId = incident.Id,
            InspectionId = inspection.Id,
            VendorId = vendor.Id,
            VendorCategory = request.VendorCategory,
            AssignedByUserId = assignedByUserId,
            AssignedDate = _dateTime.UtcNow,
            Notes = request.Notes,
        };

        await _unitOfWork.VendorAssignments.AddAsync(assignment, cancellationToken);

        // Rule 7: incident becomes VendorAssigned.
        incident.Status = IncidentStatus.VendorAssigned;

        // Rule 8: audit log.
        await _auditService.LogWorkflowActionAsync(
            nameof(VendorAssignment),
            assignment.Id.ToString(),
            AuditActions.VendorAssigned,
            newValues: new
            {
                assignment.IncidentId,
                incident.IncidentNumber,
                assignment.VendorId,
                VendorName = vendor.Name,
                VendorCategory = request.VendorCategory.ToString(),
                IncidentStatus = incident.Status.ToString(),
            },
            cancellationToken: cancellationToken);

        // Rule 9: notify the incident initiator.
        await _notificationService.QueueNotificationAsync(new NotificationRequest
        {
            RecipientUserId = incident.ReportedByUserId,
            Type = NotificationType.VendorAssigned,
            Title = "Vendor assigned",
            Message = $"Incident {incident.IncidentNumber} has been assigned to vendor '{vendor.Name}' ({request.VendorCategory}).",
            RelatedEntityName = nameof(Incident),
            RelatedEntityId = incident.Id,
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Vendor {VendorId} assigned to incident {IncidentNumber} by {UserId}; incident → {Status}",
            vendor.Id, incident.IncidentNumber, assignedByUserId, incident.Status);

        var created = await _unitOfWork.VendorAssignments.GetByIdWithDetailsAsync(assignment.Id, cancellationToken);
        return created!.ToResponse();
    }
}
