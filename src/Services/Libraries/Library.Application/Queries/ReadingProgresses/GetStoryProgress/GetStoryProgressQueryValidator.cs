namespace Library.Application.Queries.ReadingProgresses.GetStoryProgress;

/// <summary>Validates <see cref="GetStoryProgressQuery"/> input before it reaches the handler.</summary>
public sealed class GetStoryProgressQueryValidator : AbstractValidator<GetStoryProgressQuery>
{
    public GetStoryProgressQueryValidator()
    {
        // Story must be specified.
        RuleFor(x => x.StoryId).NotEmpty().WithMessage(ApplicationErrorConstants.StoryIdRequired);
    }
}
