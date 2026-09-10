namespace Content.Application.Queries.Stories.GetMyStories;

/// <summary>
/// The signed-in author's own stories, every status (Draft included). Ownership
/// is resolved from the <c>author_id</c> claim, never from a parameter.
/// </summary>
public sealed class GetMyStoriesQuery : IQuery<PagedResponseDto<StorySummaryResponseDto>>
{
    public string Status { get; init; }

    public string SortBy { get; init; }

    public string SortDirection { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = ApplicationConstants.DefaultPageSize;
}
