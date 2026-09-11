namespace Content.Api.Controllers.v1;

/// <summary>
/// Internal, service-to-service endpoints. Not for end users. Every action is
/// authorized by a valid JWT OR the <c>X-Service-Token</c> header
/// (<c>ServiceAuth:Token</c>). The Moderation service calls these to apply a
/// Hide/Remove report decision to the real content and to resolve target titles
/// for the reports queue.
/// </summary>
[ApiController]
[Route(ControllerRouteConstants.InternalBase)]
public sealed class InternalContentController : ControllerBase
{
    private readonly IMediator _mediator;

    public InternalContentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Hide (or restore) a story as the result of a moderation decision.</summary>
    [AllowAnonymous]
    [ServiceOrUserAuthorize]
    [HttpPost(ControllerRouteConstants.InternalStoryModerationVisibilitySegment)]
    [ProducesResponseType(typeof(ResponseDto<ModerationVisibilityResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetStoryModerationVisibility(
        Guid storyId,
        [FromBody] ModerationVisibilityRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SetStoryModerationVisibilityCommand
            {
                StoryId = storyId,
                Hidden = request.Hidden,
                Reason = request.Reason
            },
            cancellationToken);

        return Ok(ResponseDto<ModerationVisibilityResponseDto>.Ok(result));
    }

    /// <summary>Hide (or restore) a chapter as the result of a moderation decision.</summary>
    [AllowAnonymous]
    [ServiceOrUserAuthorize]
    [HttpPost(ControllerRouteConstants.InternalChapterModerationVisibilitySegment)]
    [ProducesResponseType(typeof(ResponseDto<ModerationVisibilityResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetChapterModerationVisibility(
        Guid chapterId,
        [FromBody] ModerationVisibilityRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SetChapterModerationVisibilityCommand
            {
                ChapterId = chapterId,
                Hidden = request.Hidden,
                Reason = request.Reason
            },
            cancellationToken);

        return Ok(ResponseDto<ModerationVisibilityResponseDto>.Ok(result));
    }

    /// <summary>Batch story public id -&gt; title (<c>?ids=guid,guid</c>). Unknown ids are omitted.</summary>
    [AllowAnonymous]
    [ServiceOrUserAuthorize]
    [HttpGet(ControllerRouteConstants.InternalStoryTitlesSegment)]
    [ProducesResponseType(typeof(ResponseDto<IReadOnlyList<ContentTitleEntryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStoryTitles(
        [FromQuery(Name = "ids")] string ids,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetStoryTitlesQuery { Ids = ParseGuidCsv(ids) }, cancellationToken);

        return Ok(ResponseDto<IReadOnlyList<ContentTitleEntryDto>>.Ok(result));
    }

    /// <summary>Batch chapter public id -&gt; title (<c>?ids=guid,guid</c>). Unknown ids are omitted.</summary>
    [AllowAnonymous]
    [ServiceOrUserAuthorize]
    [HttpGet(ControllerRouteConstants.InternalChapterTitlesSegment)]
    [ProducesResponseType(typeof(ResponseDto<IReadOnlyList<ContentTitleEntryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChapterTitles(
        [FromQuery(Name = "ids")] string ids,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetChapterTitlesQuery { Ids = ParseGuidCsv(ids) }, cancellationToken);

        return Ok(ResponseDto<IReadOnlyList<ContentTitleEntryDto>>.Ok(result));
    }

    /// <summary>
    /// Applies the Community service's recomputed rating aggregate (avg + count
    /// over all real ratings) to the story. Called best-effort after a rating
    /// write commits; a missing story here must not fail the caller's request.
    /// </summary>
    [AllowAnonymous]
    [ServiceOrUserAuthorize]
    [HttpPost(ControllerRouteConstants.InternalStoryRatingSummarySegment)]
    [ProducesResponseType(typeof(ResponseDto<RatingSummaryResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetStoryRatingSummary(
        Guid storyId,
        [FromBody] RatingSummaryRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SetStoryRatingSummaryCommand
            {
                StoryId = storyId,
                RatingAvg = request.RatingAvg,
                RatingCount = request.RatingCount
            },
            cancellationToken);

        return Ok(ResponseDto<RatingSummaryResponseDto>.Ok(result));
    }

    /// <summary>
    /// Applies the Community service's recomputed visible-comment count to the
    /// chapter. Called best-effort after a comment add/reply/delete/visibility
    /// change commits; a missing chapter here must not fail the caller's request.
    /// </summary>
    [AllowAnonymous]
    [ServiceOrUserAuthorize]
    [HttpPost(ControllerRouteConstants.InternalChapterCommentCountSegment)]
    [ProducesResponseType(typeof(ResponseDto<ChapterCommentCountResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetChapterCommentCount(
        Guid chapterId,
        [FromBody] ChapterCommentCountRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SetChapterCommentCountCommand { ChapterId = chapterId, Count = request.Count },
            cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(ResponseDto<ChapterCommentCountResponseDto>.Ok(result));
    }

    /// <summary>
    /// Batch chapter public id -&gt; chapter title + parent story id/slug/title
    /// (<c>?ids=guid,guid</c>). Unknown ids are omitted. Used by the Community
    /// service to enrich its cross-story recent-comments feed.
    /// </summary>
    [AllowAnonymous]
    [ServiceOrUserAuthorize]
    [HttpGet(ControllerRouteConstants.InternalChapterContextSegment)]
    [ProducesResponseType(typeof(ResponseDto<IReadOnlyList<ChapterContextEntryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChapterContext(
        [FromQuery(Name = "ids")] string ids,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetChapterContextQuery { Ids = ParseGuidCsv(ids) }, cancellationToken);

        return Ok(ResponseDto<IReadOnlyList<ChapterContextEntryDto>>.Ok(result));
    }

    private static IReadOnlyCollection<Guid> ParseGuidCsv(string ids)
    {
        return (ids ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => Guid.TryParse(part, out var id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();
    }
}
