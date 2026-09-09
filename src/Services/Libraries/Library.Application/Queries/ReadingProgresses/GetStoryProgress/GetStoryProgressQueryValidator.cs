namespace Library.Application.Queries.ReadingProgresses.GetStoryProgress;

public sealed class GetStoryProgressQueryValidator : AbstractValidator<GetStoryProgressQuery>
{
    public GetStoryProgressQueryValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty().WithMessage(ApplicationErrorConstants.StoryIdRequired);
    }
}
