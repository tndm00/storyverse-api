namespace Content.Application.Queries.Stories.GetStoryStatusCounts;

/// <summary>
/// Story counts per lifecycle status for the admin dashboard. Requires
/// <c>content.moderate</c>.
/// </summary>
public sealed class GetStoryStatusCountsQuery : IQuery<StoryStatusCountsResponseDto>
{
}
