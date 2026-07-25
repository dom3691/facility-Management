namespace FacilityInspection.Application.Common.Models;

/// <summary>Clamps client-supplied paging parameters to safe bounds.</summary>
public static class PaginationHelper
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static (int PageNumber, int PageSize) Normalize(int pageNumber, int pageSize)
    {
        var normalizedPageNumber = pageNumber < 1 ? 1 : pageNumber;
        var normalizedPageSize = pageSize switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => pageSize,
        };
        return (normalizedPageNumber, normalizedPageSize);
    }
}
