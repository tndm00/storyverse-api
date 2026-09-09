namespace Community.Application.Commands.Comments.SetCommentVisibility;

/// <summary>
/// Moderator action: hides (<c>Hidden</c>) or restores (<c>Visible</c>) a comment.
/// Gated by the <c>community.moderate</c> permission at the API layer. A comment
/// the author has already soft-deleted stays Deleted.
/// </summary>
public sealed class SetCommentVisibilityCommand : ICommand<CommentResponseDto>
{
    public Guid CommentId { get; init; }

    /// <summary>True to hide the comment; false to restore it to visible.</summary>
    public bool Hide { get; init; }
}
