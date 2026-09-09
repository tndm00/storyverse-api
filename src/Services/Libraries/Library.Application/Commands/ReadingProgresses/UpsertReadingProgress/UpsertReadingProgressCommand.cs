namespace Library.Application.Commands.ReadingProgresses.UpsertReadingProgress;

/// <summary>
/// Records where the caller is in a story. Called automatically whenever a
/// chapter is opened, so it creates the row on first read and advances it
/// afterwards. A free read produces no revenue but is still tracked (spec rule 09).
/// </summary>
public sealed class UpsertReadingProgressCommand : ICommand<ReadingProgressResponseDto>
{
    public Guid StoryId { get; init; }

    public Guid LastChapterId { get; init; }

    public decimal? ScrollPercent { get; init; }
}
