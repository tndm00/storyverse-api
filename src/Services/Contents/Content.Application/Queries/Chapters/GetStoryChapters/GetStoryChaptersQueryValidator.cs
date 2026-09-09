namespace Content.Application.Queries.Chapters.GetStoryChapters;

public sealed class GetStoryChaptersQueryValidator : AbstractValidator<GetStoryChaptersQuery>
{
    public GetStoryChaptersQueryValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();
    }
}
