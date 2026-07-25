namespace FacilityInspection.Application.DTOs.VendorUpdates;

/// <summary>
/// Form payload for <c>POST /api/vendor-updates</c> (evidence files are sent as separate
/// multipart parts).
/// </summary>
public class CreateVendorUpdateRequest
{
    public Guid WorkOrderId { get; set; }

    public string ProgressComment { get; set; } = string.Empty;

    public int? ProgressPercentage { get; set; }
}
