namespace Content.Application.Commands.Stories.AssignStoryTags;

/// <summary>Replaces a story's entire free-form tag set. New tags are created on demand.</summary>
public sealed class AssignStoryTagsCommand : ICommand<StoryDetailResponseDto>
{
    public Guid StoryId { get; init; }

    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}
