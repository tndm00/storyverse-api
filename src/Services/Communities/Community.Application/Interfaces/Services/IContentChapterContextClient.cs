namespace Community.Application.Interfaces.Services;

/// <summary>
/// Resolves chapter ids to their story context (story id/slug/title, chapter
/// title) via the Content service's internal batch endpoint. Best-effort by
/// contract: a lookup failure (or a chapter id it could not resolve) is simply
/// absent from the returned dictionary — the caller renders the comment with
/// the story/chapter fields left null rather than failing the whole request.
/// </summary>
public interface IContentChapterContextClient
{
    Task<IReadOnlyDictionary<Guid, ChapterContextDto>> GetContextAsync(
        IEnumerable<Guid> chapterIds, CancellationToken cancellationToken);
}
