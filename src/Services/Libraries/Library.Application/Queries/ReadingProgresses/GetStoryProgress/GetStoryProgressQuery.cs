namespace Library.Application.Queries.ReadingProgresses.GetStoryProgress;

/// <summary>The caller's last reading position in a single story.</summary>
public sealed class GetStoryProgressQuery : IQuery<ReadingProgressResponseDto>
{
    public Guid StoryId { get; init; }
}
