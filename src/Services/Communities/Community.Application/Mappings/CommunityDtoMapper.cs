namespace Community.Application.Mappings;

/// <summary>
/// Explicit entity-to-response-DTO projection for the Community service. Entities
/// are never returned directly as API contracts (code-standard.md section 19);
/// this is the single place that translates them.
/// </summary>
public static class CommunityDtoMapper
{
    public static CommentResponseDto ToDto(Comment comment)
    {
        return new CommentResponseDto
        {
            Id = comment.PublicId,
            ChapterId = comment.ChapterId,
            ParentCommentId = comment.ParentCommentId,
            AuthorUserId = comment.AuthorUserId,
            Content = comment.Content,
            Status = comment.Status.ToString(),
            LikeCount = comment.LikeCount,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }

    public static RatingResponseDto ToDto(Rating rating)
    {
        return new RatingResponseDto
        {
            Id = rating.PublicId,
            StoryId = rating.StoryId,
            UserId = rating.UserId,
            Score = rating.Score,
            ReviewText = rating.ReviewText,
            CreatedAt = rating.CreatedAt,
            UpdatedAt = rating.UpdatedAt
        };
    }
}
