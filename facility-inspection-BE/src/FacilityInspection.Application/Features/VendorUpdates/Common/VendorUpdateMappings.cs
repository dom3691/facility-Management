using FacilityInspection.Application.DTOs.Common;
using FacilityInspection.Application.DTOs.VendorUpdates;
using FacilityInspection.Domain.Entities;

namespace FacilityInspection.Application.Features.VendorUpdates.Common;

/// <summary>Manual entity → DTO mapping for vendor updates.</summary>
public static class VendorUpdateMappings
{
    public static VendorUpdateResponse ToResponse(this VendorUpdate update) => new()
    {
        Id = update.Id,
        WorkOrderId = update.WorkOrderId,
        ProgressComment = update.Notes,
        ProgressPercentage = update.PercentComplete,
        IsCompletionUpdate = update.IsCompletionUpdate,
        CompletionComment = update.CompletionComment,
        StatusAtUpdate = update.StatusAtUpdate.ToString(),
        UpdatedByUserId = update.UpdatedByUserId,
        UpdatedDate = update.UpdateDate,
        Attachments = update.Attachments
            .Select(a => new AttachmentResponse
            {
                Id = a.Id,
                FileName = a.FileName,
                ContentType = a.ContentType,
                FileSizeBytes = a.FileSizeBytes,
                StoragePath = a.StoragePath,
            })
            .ToList(),
    };
}
