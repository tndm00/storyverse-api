namespace Content.Application.Commands.Chapters.ReorderChapters;

/// <summary>
/// Sets the order of chapters within a single scope from a client-provided
/// sequence. Exactly one of <see cref="VolumeId"/> (chapters in that volume) or
/// <see cref="StoryId"/> (a story's chapters that belong to no volume) is set.
/// Story owner only, or staff with <c>content.moderate</c>.
/// </summary>
public sealed class ReorderChaptersCommand : ICommand<IReadOnlyList<ChapterSummaryResponseDto>>
{
    public Guid? VolumeId { get; init; }

    public Guid? StoryId { get; init; }

    public IReadOnlyList<Guid> OrderedChapterIds { get; init; } = Array.Empty<Guid>();
}
