namespace Community.Application.Queries.Comments.GetChapterComments;

/// <summary>Paged list of visible comments for a chapter, newest first.</summary>
public sealed class GetChapterCommentsQuery : IQuery<PagedResponseDto<CommentResponseDto>>
{
    public Guid ChapterId { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = ApplicationConstants.DefaultPageSize;
}
