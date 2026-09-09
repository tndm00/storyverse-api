namespace Moderation.Application.Commands.Reports.DismissReport;

public sealed class DismissReportCommandValidator : AbstractValidator<DismissReportCommand>
{
    public DismissReportCommandValidator()
    {
        RuleFor(x => x.ReportId).NotEmpty();

        RuleFor(x => x.Note)
            .MaximumLength(ApplicationConstants.MaxNoteLength);
    }
}
