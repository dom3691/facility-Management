namespace FacilityInspection.Application.Common.Models;

/// <summary>
/// A single page of results plus the paging metadata a client needs to navigate.
/// </summary>
public class PaginatedResult<T>
{
    public PaginatedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public IReadOnlyList<T> Items { get; }

    public int PageNumber { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public static PaginatedResult<T> Create(
        IReadOnlyList<T> items,
        int totalCount,
        int pageNumber,
        int pageSize)
        => new(items, totalCount, pageNumber, pageSize);
}
