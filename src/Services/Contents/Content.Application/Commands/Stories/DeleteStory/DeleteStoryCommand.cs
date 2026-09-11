namespace Content.Application.Commands.Stories.DeleteStory;

/// <summary>
/// Hard-deletes a story. Only allowed while the story is still
/// <see cref="StoryStatus.Draft"/> (never published, never seen publicly) so a
/// mis-created draft can be cleaned up. Any other status must go through the
/// status-change endpoint instead — this is not a substitute for that
/// lifecycle. Story owner only, or staff with <c>content.moderate</c>.
/// </summary>
public sealed class DeleteStoryCommand : ICommand<Unit>
{
    public Guid StoryId { get; init; }
}
