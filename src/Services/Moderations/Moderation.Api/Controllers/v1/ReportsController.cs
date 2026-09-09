namespace Moderation.Api.Controllers.v1;

/// <summary>
/// Content-violation reports and their moderation lifecycle. Submitting a report
/// is open to any authenticated user; every queue and decision action requires a
/// moderation permission (held by <c>Moderator</c> and <c>PlatformAdmin</c>),
/// enforced by the shared permission policy. Stays thin: binds the request,
/// sends a command/query through MediatR, wraps the result in
/// <see cref="ResponseDto{T}"/>.
/// </summary>
[ApiController]
[Route(ControllerRouteConstants.ReportsBase)]
public sealed class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Any authenticated user files a report. It enters the queue as <c>Pending</c>.</summary>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ResponseDto<ReportDetailResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> SubmitReport(
        [FromBody] SubmitReportRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new SubmitReportCommand
        {
            TargetType = request.TargetType,
            TargetId = request.TargetId,
            Reason = request.Reason,
            Description = request.Description
        };

        var result = await _mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, ResponseDto<ReportDetailResponseDto>.Ok(result));
    }

    /// <summary>Moderation queue, newest first, paged, filtered by <c>status</c> / <c>reason</c>.</summary>
    [HasPermission(StoryVersePermissions.Reports.Review)]
    [HttpGet]
    [ProducesResponseType(typeof(ResponseDto<PagedResponseDto<ReportSummaryResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReports(
        [FromQuery(Name = "status")] string status,
        [FromQuery(Name = "reason")] string reason,
        [FromQuery(Name = "page-number")] int pageNumber = 1,
        [FromQuery(Name = "page-size")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetReportsQuery
        {
            Status = status,
            Reason = reason,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(ResponseDto<PagedResponseDto<ReportSummaryResponseDto>>.Ok(result));
    }

    [HasPermission(StoryVersePermissions.Reports.Review)]
    [HttpGet(ControllerRouteConstants.ReportByIdSegment)]
    [ProducesResponseType(typeof(ResponseDto<ReportDetailResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReport(Guid reportId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetReportDetailQuery { ReportId = reportId }, cancellationToken);

        return Ok(ResponseDto<ReportDetailResponseDto>.Ok(result));
    }

    /// <summary>Picks a report up off the queue: <c>Pending → Reviewing</c>.</summary>
    [HasPermission(StoryVersePermissions.Reports.Review)]
    [HttpPost(ControllerRouteConstants.ReportReviewSegment)]
    [ProducesResponseType(typeof(ResponseDto<ReportDetailResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Review(Guid reportId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ReviewReportCommand { ReportId = reportId }, cancellationToken);

        return Ok(ResponseDto<ReportDetailResponseDto>.Ok(result));
    }

    /// <summary>
    /// Records a moderation action (<c>Warn</c> / <c>Hide</c> / <c>Remove</c>) and
    /// closes the report: <c>Status = Resolved</c>, <c>ResolvedAt</c> stamped.
    /// </summary>
    [HasPermission(StoryVersePermissions.Reports.Resolve)]
    [HttpPost(ControllerRouteConstants.ReportResolveSegment)]
    [ProducesResponseType(typeof(ResponseDto<ReportDetailResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Resolve(
        Guid reportId,
        [FromBody] ResolveReportRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new ResolveReportCommand
        {
            ReportId = reportId,
            Action = request.Action,
            Note = request.Note
        };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(ResponseDto<ReportDetailResponseDto>.Ok(result));
    }

    /// <summary>Closes a report with no action against the content: <c>Status = Dismissed</c>.</summary>
    [HasPermission(StoryVersePermissions.Reports.Resolve)]
    [HttpPost(ControllerRouteConstants.ReportDismissSegment)]
    [ProducesResponseType(typeof(ResponseDto<ReportDetailResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Dismiss(
        Guid reportId,
        [FromBody] DismissReportRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new DismissReportCommand { ReportId = reportId, Note = request.Note };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(ResponseDto<ReportDetailResponseDto>.Ok(result));
    }
}
