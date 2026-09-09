namespace Content.Application.Commands.Volumes.CreateVolume;

public sealed class CreateVolumeCommandValidator : AbstractValidator<CreateVolumeCommand>
{
    public CreateVolumeCommandValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(ApplicationErrorConstants.TitleRequired)
            .MaximumLength(ApplicationConstants.MaxTitleLength);

        RuleFor(x => x.OrderIndex).GreaterThanOrEqualTo(0);
    }
}
