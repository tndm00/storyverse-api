namespace Content.Application.Commands.Volumes.CreateVolume;

/// <summary>Validates <see cref="CreateVolumeCommand"/> input before it reaches the handler.</summary>
public sealed class CreateVolumeCommandValidator : AbstractValidator<CreateVolumeCommand>
{
    /// <summary>Defines the field-level validation rules for creating a volume.</summary>
    public CreateVolumeCommandValidator()
    {
        // Target story must be identified.
        RuleFor(x => x.StoryId).NotEmpty();

        // Title is required and bounded in length.
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(ApplicationErrorConstants.TitleRequired)
            .MaximumLength(ApplicationConstants.MaxTitleLength);

        // Order index cannot be negative.
        RuleFor(x => x.OrderIndex).GreaterThanOrEqualTo(0);
    }
}
