namespace Moderation.Application.Commands.Reports.ResolveReport;

/// <summary>Validation rules for <see cref="ResolveReportCommand"/>.</summary>
public sealed class ResolveReportCommandValidator : AbstractValidator<ResolveReportCommand>
{
    /// <summary>Requires a non-empty report id, a valid action other than <c>Dismiss</c>, and caps the note length.</summary>
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
