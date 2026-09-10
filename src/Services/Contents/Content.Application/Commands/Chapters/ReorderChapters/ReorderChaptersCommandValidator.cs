namespace Content.Application.Commands.Chapters.ReorderChapters;

public sealed class ReorderChaptersCommandValidator : AbstractValidator<ReorderChaptersCommand>
{
    public ReorderChaptersCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => x.VolumeId.HasValue ^ x.StoryId.HasValue)
            .WithMessage("Exactly one of volumeId or storyId must be provided.");

        RuleFor(x => x.OrderedChapterIds)
            .NotEmpty().WithMessage(ApplicationErrorConstants.ReorderListEmpty);

        RuleForEach(x => x.OrderedChapterIds).NotEmpty();
    }
}
