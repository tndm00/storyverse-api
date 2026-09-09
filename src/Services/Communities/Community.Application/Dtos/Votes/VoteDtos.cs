namespace Community.Application.Dtos;

/// <summary>Payload to cast a weekly ranking vote for a story.</summary>
public sealed class CastVoteRequestDto
{
    public Guid StoryId { get; init; }
}

/// <summary>Result of casting a vote.</summary>
public sealed class CastVoteResultDto
{
    public Guid StoryId { get; init; }

    public string WeekKey { get; init; }

    /// <summary>False when the caller had already voted for this story this week.</summary>
    public bool Recorded { get; init; }

    public int WeekVoteCount { get; init; }
}

/// <summary>This week's vote tally for a story.</summary>
public sealed class VoteCountResponseDto
{
    public Guid StoryId { get; init; }

    public string WeekKey { get; init; }

    public int WeekVoteCount { get; init; }
}
