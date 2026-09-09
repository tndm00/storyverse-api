namespace Community.Application.Commands.Comments.AddComment;

/// <summary>Adds a top-level comment on a chapter for the authenticated caller.</summary>
public sealed class AddCommentCommand : ICommand<CommentResponseDto>
{
    public Guid ChapterId { get; init; }

    public string Content { get; init; } = string.Empty;
}
