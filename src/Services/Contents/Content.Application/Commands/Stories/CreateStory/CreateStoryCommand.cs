namespace Content.Application.Commands.Stories.CreateStory;

/// <summary>Creates a new story owned by the calling author. The story starts as a draft.</summary>
public sealed class CreateStoryCommand : ICommand<StoryDetailResponseDto>
{
    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string CoverImageUrl { get; init; }

    public StoryContentType ContentType { get; init; } = StoryContentType.Original;

    public string OriginalSource { get; init; }

    public string Language { get; init; } = ApplicationConstants.DefaultLanguage;

    public AgeRating AgeRating { get; init; } = AgeRating.General;
}
