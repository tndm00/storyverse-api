namespace Community.Application.Queries.Votes.GetCurrentVotePeriod;

/// <summary>
/// The current weekly voting period boundaries, so the client can render a
/// "votes reset in …" countdown without guessing the server's week rule.
/// </summary>
public sealed class GetCurrentVotePeriodQuery : IQuery<VotePeriodResponseDto>
{
}
