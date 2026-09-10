namespace Content.Application.Queries.Moderation.GetStoryTitles;

/// <summary>Internal batch lookup: story public id -&gt; title. X-Service-Token protected.</summary>
public sealed class GetStoryTitlesQuery : IQuery<IReadOnlyList<ContentTitleEntryDto>>
{
    public IReadOnlyCollection<Guid> Ids { get; init; } = Array.Empty<Guid>();
}
