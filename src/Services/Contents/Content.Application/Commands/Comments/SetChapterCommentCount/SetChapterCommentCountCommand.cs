namespace Content.Application.Commands.Comments.SetChapterCommentCount;

/// <summary>
/// Internal, service-to-service: applies the Community service's recomputed
/// visible-comment count to the denormalized
/// <see cref="Content.Domain.Entities.Chapter.CommentCount"/> field. Not an
/// author action — reached only through the <c>X-Service-Token</c>-protected
/// internal endpoint, called best-effort by Community after a comment write
/// commits. Returns <c>null</c> (no throw) when the chapter does not exist,
/// so the controller can answer 404 without the exception-handling middleware.
/// </summary>
public sealed class SetChapterCommentCountCommand : ICommand<ChapterCommentCountResponseDto>
{
    public Guid ChapterId { get; init; }

    public int Count { get; init; }
}
