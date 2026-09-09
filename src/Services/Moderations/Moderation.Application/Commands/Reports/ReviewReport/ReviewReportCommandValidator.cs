namespace Moderation.Application.Commands.Reports.ReviewReport;

public sealed class ReviewReportCommandValidator : AbstractValidator<ReviewReportCommand>
{
    public ReviewReportCommandValidator()
    {
        RuleFor(x => x.ReportId).NotEmpty();
    }
}
