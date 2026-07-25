using FacilityInspection.Application.Common.Audit;
using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Files;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.VendorUpdates;
using FacilityInspection.Application.Features.VendorUpdates.Common;
using FacilityInspection.Domain.Entities;
using FacilityInspection.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FacilityInspection.Application.Features.VendorUpdates.CreateVendorUpdate;

/// <summary>
/// Records a vendor progress update. If the work order is still <c>Assigned</c>, the first
/// update moves it to <c>InProgress</c>. Stored in one transaction with any evidence files.
/// </summary>
public class CreateVendorUpdateCommandHandler
    : IRequestHandler<CreateVendorUpdateCommand, VendorUpdateResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly IIdentityService _identityService;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<CreateVendorUpdateCommandHandler> _logger;

    public CreateVendorUpdateCommandHandler(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorage,
        IIdentityService identityService,
        IAuditService auditService,
        ICurrentUserService currentUser,
        IDateTimeProvider dateTime,
        ILogger<CreateVendorUpdateCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _identityService = identityService;
        _auditService = auditService;
        _currentUser = currentUser;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task<VendorUpdateResponse> Handle(CreateVendorUpdateCommand request, CancellationToken cancellationToken)
    {
        Guid.TryParse(_currentUser.UserId, out var updatedByUserId);

        // Rule 1: work order must exist.
        var workOrder = await _unitOfWork.WorkOrders.GetByIdAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        // Rule 2: vendor may only update their own work orders.
        await VendorUpdateAuthorization.EnsureCanAccessWorkOrderAsync(
            workOrder, _currentUser, _identityService, cancellationToken);

        // Rule 3: work order must be Assigned or InProgress.
        if (workOrder.Status is not (WorkOrderStatus.Assigned or WorkOrderStatus.InProgress))
        {
            throw new BadRequestException(
                $"Work order {workOrder.WorkOrderNumber} cannot be updated (status: {workOrder.Status}).");
        }

        // Rule 4: first update on an Assigned work order moves it to InProgress.
        if (workOrder.Status == WorkOrderStatus.Assigned)
        {
            workOrder.Status = WorkOrderStatus.InProgress;
        }

        var update = new VendorUpdate
        {
            WorkOrderId = workOrder.Id,
            UpdatedByUserId = updatedByUserId,
            UpdateDate = _dateTime.UtcNow,
            StatusAtUpdate = workOrder.Status,
            Notes = request.ProgressComment,
            PercentComplete = request.ProgressPercentage,
            IsCompletionUpdate = false,
        };

        await StoreAttachmentsAsync(update, request.Attachments, cancellationToken);
        await _unitOfWork.VendorUpdates.AddAsync(update, cancellationToken);

        await _auditService.LogWorkflowActionAsync(
            nameof(VendorUpdate),
            update.Id.ToString(),
            AuditActions.VendorProgressUpdate,
            newValues: new
            {
                update.WorkOrderId,
                workOrder.WorkOrderNumber,
                ProgressComment = update.Notes,
                ProgressPercentage = update.PercentComplete,
                WorkOrderStatus = workOrder.Status.ToString(),
            },
            cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Vendor update {UpdateId} recorded for work order {WorkOrderNumber} by {UserId}",
            update.Id, workOrder.WorkOrderNumber, updatedByUserId);

        var created = await _unitOfWork.VendorUpdates.GetByIdWithDetailsAsync(update.Id, cancellationToken);
        return created!.ToResponse();
    }

    private async Task StoreAttachmentsAsync(
        VendorUpdate update,
        IReadOnlyList<FileUpload> attachments,
        CancellationToken cancellationToken)
    {
        foreach (var file in attachments)
        {
            _fileStorage.ValidateFile(file.FileName, file.SizeBytes);
        }

        foreach (var file in attachments)
        {
            using var stream = new MemoryStream(file.Content);
            var storagePath = await _fileStorage.SaveAsync(
                stream, file.FileName, file.ContentType, FileModules.VendorUpdates, cancellationToken);

            update.Attachments.Add(new VendorUpdateAttachment
            {
                FileName = file.FileName,
                StoragePath = storagePath,
                ContentType = file.ContentType,
                FileSizeBytes = file.SizeBytes,
            });
        }
    }
}
