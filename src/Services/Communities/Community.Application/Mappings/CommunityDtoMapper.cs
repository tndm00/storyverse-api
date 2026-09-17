namespace Community.Application.Mappings;

/// <summary>
/// Explicit entity-to-response-DTO projection for the Community service. Entities
/// are never returned directly as API contracts (code-standard.md section 19);
/// this is the single place that translates them.
/// </summary>
public static class CommunityDtoMapper
{
    /// <summary>Projects a <see cref="Comment"/> entity into its response DTO, optionally attaching the author's resolved display name.</summary>
    public static CommentResponseDto ToDto(Comment comment, string authorDisplayName = null)
    {
        return new CommentResponseDto
        {
            Id = comment.PublicId,
            ChapterId = comment.ChapterId,
            ParentCommentId = comment.ParentCommentId,
            AuthorUserId = comment.AuthorUserId,
            AuthorDisplayName = authorDisplayName,
            Content = comment.Content,
            Status = comment.Status.ToString(),
            LikeCount = comment.LikeCount,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }

    /// <summary>Projects a <see cref="Rating"/> entity into its response DTO, optionally attaching the rating user's resolved display name.</summary>
    public static RatingResponseDto ToDto(Rating rating, string userDisplayName = null)
    {
        return new RatingResponseDto
        {
            Id = rating.PublicId,
            StoryId = rating.StoryId,
            UserId = rating.UserId,
            UserDisplayName = userDisplayName,
            Score = rating.Score,
            ReviewText = rating.ReviewText,
            CreatedAt = rating.CreatedAt,
            UpdatedAt = rating.UpdatedAt
        };
    }
}
