namespace Content.Application.Queries.Chapters.GetChapterContext;

/// <summary>
/// Internal batch lookup: chapter public id -&gt; chapter title plus parent
/// story id/slug/title. X-Service-Token protected. Used by the Community
/// service to enrich a cross-story comment feed without N follow-up calls.
/// </summary>
public sealed class GetChapterContextQuery : IQuery<IReadOnlyList<ChapterContextEntryDto>>
{
    public IReadOnlyCollection<Guid> Ids { get; init; } = Array.Empty<Guid>();
}
