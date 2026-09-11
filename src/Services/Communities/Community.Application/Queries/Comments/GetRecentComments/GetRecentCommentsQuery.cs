namespace Community.Application.Queries.Comments.GetRecentComments;

/// <summary>
/// The N most recent visible comments across the whole platform, newest first,
/// enriched with story/chapter context — feed for "recently commented stories".
/// </summary>
public sealed class GetRecentCommentsQuery : IQuery<IReadOnlyList<RecentCommentEntryDto>>
{
    public int Limit { get; init; } = 15;
}
