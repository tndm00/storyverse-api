namespace Content.Application.Commands.Stories.ChangeStoryStatus;

/// <summary>Requests a story lifecycle transition. Owner only.</summary>
public sealed class ChangeStoryStatusCommand : ICommand<StoryDetailResponseDto>
{
    public Guid StoryId { get; init; }

    public StoryStatus TargetStatus { get; init; }
}
