namespace Content.Application.Queries.Chapters.GetStoryChapters;

public sealed class GetStoryChaptersQueryValidator : AbstractValidator<GetStoryChaptersQuery>
{
    /// <summary>Requires a non-empty story id.</summary>
    public GetStoryChaptersQueryValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();
    }
}
