namespace Community.Application.Dtos;

/// <summary>Payload to add a top-level comment on a chapter.</summary>
public sealed class AddCommentRequestDto
{
    public Guid ChapterId { get; init; }

    public string Content { get; init; } = string.Empty;
}

/// <summary>Payload to reply to an existing comment.</summary>
public sealed class ReplyCommentRequestDto
{
    public string Content { get; init; } = string.Empty;
}

/// <summary>Payload to edit the caller's own comment.</summary>
public sealed class EditCommentRequestDto
{
    public string Content { get; init; } = string.Empty;
}

/// <summary>A comment as returned to clients.</summary>
public sealed class CommentResponseDto
{
    public Guid Id { get; init; }

    public Guid ChapterId { get; init; }

    public Guid? ParentCommentId { get; init; }

    public long AuthorUserId { get; init; }

    /// <summary>
    /// Author's public display name, resolved from the Authentication service.
    /// Null when the lookup was unavailable — clients fall back to
    /// <see cref="AuthorUserId"/>.
    /// </summary>
    public string AuthorDisplayName { get; init; }

    public string Content { get; init; }

    public string Status { get; init; }

    public int LikeCount { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }
}
