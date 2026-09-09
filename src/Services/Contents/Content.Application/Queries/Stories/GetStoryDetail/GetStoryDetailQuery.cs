namespace Content.Application.Queries.Stories.GetStoryDetail;

/// <summary>
/// Story page header by slug. A caller who owns the story also sees it while it is
/// still a draft; everyone else only sees a story that has been published.
/// </summary>
public sealed class GetStoryDetailQuery : IQuery<StoryDetailResponseDto>
{
    public string Slug { get; init; } = string.Empty;
}
