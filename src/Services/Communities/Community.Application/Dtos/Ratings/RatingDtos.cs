namespace Community.Application.Dtos;

/// <summary>Payload to create or replace the caller's rating for a story.</summary>
public sealed class UpsertRatingRequestDto
{
    public Guid StoryId { get; init; }

    public int Score { get; init; }

    public string ReviewText { get; init; }
}

/// <summary>A rating as returned to clients.</summary>
public sealed class RatingResponseDto
{
    public Guid Id { get; init; }

    public Guid StoryId { get; init; }

    public long UserId { get; init; }

    public int Score { get; init; }

    public string ReviewText { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }
}
