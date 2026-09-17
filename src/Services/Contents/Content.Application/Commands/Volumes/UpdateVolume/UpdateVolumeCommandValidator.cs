namespace Content.Application.Commands.Volumes.UpdateVolume;

/// <summary>Validates <see cref="UpdateVolumeCommand"/> input before it reaches the handler.</summary>
public sealed class UpdateVolumeCommandValidator : AbstractValidator<UpdateVolumeCommand>
{
    /// <summary>Defines the field-level validation rules for updating a volume.</summary>
    public UpdateVolumeCommandValidator()
    {
        // Target volume must be identified.
        RuleFor(x => x.VolumeId).NotEmpty();

        // Title is required and bounded in length.
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(ApplicationErrorConstants.TitleRequired)
            .MaximumLength(ApplicationConstants.MaxTitleLength);

        // Order index cannot be negative.
        RuleFor(x => x.OrderIndex).GreaterThanOrEqualTo(0);
    }
}
