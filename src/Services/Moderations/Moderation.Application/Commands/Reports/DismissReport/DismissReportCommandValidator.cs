namespace Moderation.Application.Commands.Reports.DismissReport;

/// <summary>Validation rules for <see cref="DismissReportCommand"/>.</summary>
public sealed class DismissReportCommandValidator : AbstractValidator<DismissReportCommand>
{
    /// <summary>Requires a non-empty report id and caps the optional note length.</summary>
    public DismissReportCommandValidator()
    {
        RuleFor(x => x.ReportId).NotEmpty();

        RuleFor(x => x.Note)
            .MaximumLength(ApplicationConstants.MaxNoteLength);
    }
}
