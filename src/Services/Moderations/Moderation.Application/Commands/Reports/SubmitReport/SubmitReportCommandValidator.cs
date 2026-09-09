namespace Moderation.Application.Commands.Reports.SubmitReport;

public sealed class SubmitReportCommandValidator : AbstractValidator<SubmitReportCommand>
{
    public SubmitReportCommandValidator()
    {
        RuleFor(x => x.TargetType).IsInEnum();

        RuleFor(x => x.TargetId)
            .NotEmpty().WithMessage(ApplicationErrorConstants.TargetIdRequired);

        RuleFor(x => x.Reason).IsInEnum();

        RuleFor(x => x.Description)
            .MaximumLength(ApplicationConstants.MaxDescriptionLength);
    }
}
