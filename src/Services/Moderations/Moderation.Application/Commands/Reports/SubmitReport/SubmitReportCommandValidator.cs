namespace Moderation.Application.Commands.Reports.SubmitReport;

/// <summary>Validation rules for <see cref="SubmitReportCommand"/>.</summary>
public sealed class SubmitReportCommandValidator : AbstractValidator<SubmitReportCommand>
{
    /// <summary>Requires valid target type/reason enums, a non-empty target id, and caps the description length.</summary>
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
