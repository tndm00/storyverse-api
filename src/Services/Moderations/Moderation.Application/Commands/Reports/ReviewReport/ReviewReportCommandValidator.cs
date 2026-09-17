namespace Moderation.Application.Commands.Reports.ReviewReport;

/// <summary>Validation rules for <see cref="ReviewReportCommand"/>.</summary>
public sealed class ReviewReportCommandValidator : AbstractValidator<ReviewReportCommand>
{
    /// <summary>Requires a non-empty report id.</summary>
    public ReviewReportCommandValidator()
    {
        RuleFor(x => x.ReportId).NotEmpty();
    }
}
