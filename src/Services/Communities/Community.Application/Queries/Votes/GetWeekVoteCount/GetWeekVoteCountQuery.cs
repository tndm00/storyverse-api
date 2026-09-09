namespace Community.Application.Queries.Votes.GetWeekVoteCount;

/// <summary>Returns the current ISO week's vote count for a story.</summary>
public sealed class GetWeekVoteCountQuery : IQuery<VoteCountResponseDto>
{
    public Guid StoryId { get; init; }
}
