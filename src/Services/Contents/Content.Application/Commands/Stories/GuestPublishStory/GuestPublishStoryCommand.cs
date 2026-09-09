namespace Content.Application.Commands.Stories.GuestPublishStory;

/// <summary>
/// Anonymous quick-publish. No authenticated caller — the guest types a pen name.
/// Creates a story with <c>AuthorProfileId = 0</c> and
/// <see cref="Content.Domain.Entities.Story.GuestAuthorName"/> set, writes and
/// publishes the first chapter, and marks the story Ongoing. The guest cannot edit
/// it afterwards.
/// </summary>
public sealed class GuestPublishStoryCommand : ICommand<StoryDetailResponseDto>
{
    public string GuestPenName { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public IReadOnlyList<StoryGenreSelection> Genres { get; init; } = Array.Empty<StoryGenreSelection>();

    public string ChapterContent { get; init; } = string.Empty;
}
