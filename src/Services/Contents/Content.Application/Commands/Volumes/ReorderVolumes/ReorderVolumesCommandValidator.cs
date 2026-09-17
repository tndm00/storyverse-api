namespace Content.Application.Commands.Volumes.ReorderVolumes;

/// <summary>Validates <see cref="ReorderVolumesCommand"/> input before it reaches the handler.</summary>
public sealed class ReorderVolumesCommandValidator : AbstractValidator<ReorderVolumesCommand>
{
    /// <summary>Defines the field-level validation rules for reordering volumes.</summary>
    public ReorderVolumesCommandValidator()
    {
        // Target story must be identified.
        RuleFor(x => x.StoryId).NotEmpty();

        // The ordered list must be present and non-empty.
        RuleFor(x => x.OrderedVolumeIds)
            .NotEmpty().WithMessage(ApplicationErrorConstants.ReorderListEmpty);

        // Every entry in the list must be a valid (non-empty) id.
        RuleForEach(x => x.OrderedVolumeIds).NotEmpty();
    }
}
