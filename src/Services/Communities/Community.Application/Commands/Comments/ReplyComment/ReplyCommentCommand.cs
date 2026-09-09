namespace Community.Application.Commands.Comments.ReplyComment;

/// <summary>Adds a reply under an existing top-level comment.</summary>
public sealed class ReplyCommentCommand : ICommand<CommentResponseDto>
{
    public Guid ParentCommentId { get; init; }

    public string Content { get; init; } = string.Empty;
}
