namespace Content.Application.Commands.Volumes.UpdateVolume;

public sealed class UpdateVolumeCommandValidator : AbstractValidator<UpdateVolumeCommand>
{
    public UpdateVolumeCommandValidator()
    {
        RuleFor(x => x.VolumeId).NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(ApplicationErrorConstants.TitleRequired)
            .MaximumLength(ApplicationConstants.MaxTitleLength);

        RuleFor(x => x.OrderIndex).GreaterThanOrEqualTo(0);
    }
}
