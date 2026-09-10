namespace Content.Application.Queries.Moderation.GetChapterTitles;

/// <summary>Internal batch lookup: chapter public id -&gt; title. X-Service-Token protected.</summary>
public sealed class GetChapterTitlesQuery : IQuery<IReadOnlyList<ContentTitleEntryDto>>
{
    public IReadOnlyCollection<Guid> Ids { get; init; } = Array.Empty<Guid>();
}
