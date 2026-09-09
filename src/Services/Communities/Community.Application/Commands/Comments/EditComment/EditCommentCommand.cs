namespace Community.Application.Commands.Comments.EditComment;

/// <summary>Edits the body of the caller's own visible comment.</summary>
public sealed class EditCommentCommand : ICommand<CommentResponseDto>
{
    public Guid CommentId { get; init; }

    public string Content { get; init; } = string.Empty;
}
