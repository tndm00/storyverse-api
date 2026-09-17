namespace Notification.Application.Dtos;

/// <summary>
/// Internal pagination envelope carried inside <c>ResponseDto&lt;T&gt;.Data</c> for
/// list endpoints, per code-standard.md section 16.
/// </summary>
public sealed class PagedResponseDto<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>Builds a page envelope from an already-fetched page of items and the total item count.</summary>
    public static PagedResponseDto<T> Create(IReadOnlyList<T> items, int pageNumber, int pageSize, int totalCount)
    {
        return new PagedResponseDto<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
