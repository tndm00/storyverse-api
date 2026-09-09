namespace Moderation.Application.Commands.Reports.ResolveReport;

public sealed class ResolveReportCommandValidator : AbstractValidator<ResolveReportCommand>
{
    public ResolveReportCommandValidator()
    {
        RuleFor(x => x.ReportId).NotEmpty();

        RuleFor(x => x.Action)
            .IsInEnum()
            .Must(action => action != ModerationActionType.Dismiss)
            .WithMessage(ApplicationErrorConstants.ResolveActionInvalid);

        RuleFor(x => x.Note)
            .MaximumLength(ApplicationConstants.MaxNoteLength);
    }
}
