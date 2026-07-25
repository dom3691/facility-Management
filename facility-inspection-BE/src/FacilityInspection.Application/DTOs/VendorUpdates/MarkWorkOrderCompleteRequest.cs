namespace FacilityInspection.Application.DTOs.VendorUpdates;

/// <summary>
/// Form payload for <c>POST /api/vendor-updates/{workOrderId}/mark-complete</c>
/// (completion evidence files are sent as separate multipart parts).
/// </summary>
public class MarkWorkOrderCompleteRequest
{
    public string CompletionComment { get; set; } = string.Empty;
}
