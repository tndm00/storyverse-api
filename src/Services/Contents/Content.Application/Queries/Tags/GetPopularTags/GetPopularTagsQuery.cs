namespace Content.Application.Queries.Tags.GetPopularTags;

/// <summary>Most-used free-form tags, for tag-cloud / suggestion UIs.</summary>
public sealed class GetPopularTagsQuery : IQuery<IReadOnlyList<TagResponseDto>>
{
    public int Count { get; init; } = ApplicationConstants.PopularTagsCount;
}
