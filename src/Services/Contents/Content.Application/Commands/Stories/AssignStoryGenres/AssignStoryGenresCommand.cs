namespace Content.Application.Commands.Stories.AssignStoryGenres;

/// <summary>Replaces a story's entire genre set. Exactly one genre must be primary.</summary>
public sealed class AssignStoryGenresCommand : ICommand<StoryDetailResponseDto>
{
    public Guid StoryId { get; init; }

    public IReadOnlyList<StoryGenreSelection> Genres { get; init; } = Array.Empty<StoryGenreSelection>();
}
