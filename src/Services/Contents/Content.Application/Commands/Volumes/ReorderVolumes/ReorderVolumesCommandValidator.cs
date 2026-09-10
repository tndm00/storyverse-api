namespace Content.Application.Commands.Volumes.ReorderVolumes;

public sealed class ReorderVolumesCommandValidator : AbstractValidator<ReorderVolumesCommand>
{
    public ReorderVolumesCommandValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();

        RuleFor(x => x.OrderedVolumeIds)
            .NotEmpty().WithMessage(ApplicationErrorConstants.ReorderListEmpty);

        RuleForEach(x => x.OrderedVolumeIds).NotEmpty();
    }
}
