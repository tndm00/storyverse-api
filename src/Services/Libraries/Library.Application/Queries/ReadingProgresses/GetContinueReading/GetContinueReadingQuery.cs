namespace Library.Application.Queries.ReadingProgresses.GetContinueReading;

/// <summary>
/// "Đọc tiếp" — a paged list of the caller's most recently read stories, newest
/// first, so the reader can jump straight back in.
/// </summary>
public sealed class GetContinueReadingQuery : IQuery<PagedResponseDto<ReadingProgressResponseDto>>
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = ApplicationConstants.DefaultPageSize;
}
