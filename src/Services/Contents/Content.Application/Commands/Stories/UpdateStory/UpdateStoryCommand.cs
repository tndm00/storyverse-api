namespace Content.Application.Commands.Stories.UpdateStory;

/// <summary>Updates editable story metadata. Owner only. Does not change status or classification.</summary>
public sealed class UpdateStoryCommand : ICommand<StoryDetailResponseDto>
{
    public Guid StoryId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string CoverImageUrl { get; init; }

    public StoryContentType ContentType { get; init; } = StoryContentType.Original;

    public string OriginalSource { get; init; }

    public string Language { get; init; } = ApplicationConstants.DefaultLanguage;

    public AgeRating AgeRating { get; init; } = AgeRating.General;
}
