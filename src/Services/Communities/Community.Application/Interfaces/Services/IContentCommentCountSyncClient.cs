namespace Community.Application.Interfaces.Services;

/// <summary>
/// Pushes the recomputed visible-comment count for one chapter to the Content
/// service, which stores it denormalized on <c>Chapter.CommentCount</c> for
/// story listings ("most commented" sort) and displays. Called best-effort
/// after a comment write (add/reply/delete/visibility change) commits.
/// Best-effort by contract: a sync failure is logged and swallowed, never
/// thrown, so it never fails the caller's comment write.
/// </summary>
public interface IContentCommentCountSyncClient
{
    Task SyncCommentCountAsync(Guid chapterId, int commentCount, CancellationToken cancellationToken);
}
