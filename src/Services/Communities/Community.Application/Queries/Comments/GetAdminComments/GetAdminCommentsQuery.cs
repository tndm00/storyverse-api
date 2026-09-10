namespace Community.Application.Queries.Comments.GetAdminComments;

/// <summary>
/// Cross-chapter comment moderation listing. Requires <c>community.moderate</c>.
/// </summary>
public sealed class GetAdminCommentsQuery : IQuery<PagedResponseDto<CommentResponseDto>>
{
    /// <summary><c>Visible</c> / <c>Hidden</c> / <c>Deleted</c>; <c>all</c> or null returns every status.</summary>
    public string Status { get; init; }

    /// <summary>ILIKE match against the comment content.</summary>
    public string Keyword { get; init; }

    public Guid? ChapterId { get; init; }

    public long? AuthorUserId { get; init; }

    /// <summary>Only <c>CreatedAt</c> is supported; other values fall back to it.</summary>
    public string SortBy { get; init; }

    /// <summary><c>asc</c> or <c>desc</c> (default).</summary>
    public string SortDirection { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = ApplicationConstants.DefaultPageSize;
}
