namespace Library.Application.Queries.LibraryEntries.GetMyLibrary;

/// <summary>Paged listing of the caller's library, optionally filtered by shelf.</summary>
public sealed class GetMyLibraryQuery : IQuery<PagedResponseDto<LibraryEntryResponseDto>>
{
    public string ShelfStatus { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = ApplicationConstants.DefaultPageSize;
}
