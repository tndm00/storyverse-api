namespace Content.Api.Controllers.v1;

/// <summary>
/// Story discovery (public) and authoring (author only). Stays thin: binds the
/// request, sends a command/query through MediatR, wraps the result in
/// <see cref="ResponseDto{T}"/>.
/// </summary>
[ApiController]
[Route(ControllerRouteConstants.StoriesBase)]
public sealed class StoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public StoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Public discovery listing. Draft stories are never returned.</summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(ResponseDto<PagedResponseDto<StorySummaryResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStories(
        [FromQuery(Name = "genre-slug")] string genreSlug,
        [FromQuery(Name = "tag-slug")] string tagSlug,
        [FromQuery(Name = "q")] string q,
        [FromQuery(Name = "author-profile-id")] long? authorProfileId,
        [FromQuery(Name = "status")] string status,
        [FromQuery(Name = "length")] string length,
        [FromQuery(Name = "sort-by")] string sortBy,
        [FromQuery(Name = "sort-direction")] string sortDirection,
        [FromQuery(Name = "page-number")] int pageNumber = 1,
        [FromQuery(Name = "page-size")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStoriesQuery
        {
            GenreSlug = genreSlug,
            TagSlug = tagSlug,
            Keyword = q,
            AuthorProfileId = authorProfileId,
            Status = status,
            Length = length,
            SortBy = sortBy,
            SortDirection = sortDirection,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(ResponseDto<PagedResponseDto<StorySummaryResponseDto>>.Ok(result));
    }

    /// <summary>Admin catalog listing: stories in every status (Draft included).</summary>
    [HasPermission(StoryVersePermissions.Content.Moderate)]
    [HttpGet(ControllerRouteConstants.StoryAdminSegment)]
    [ProducesResponseType(typeof(ResponseDto<PagedResponseDto<StorySummaryResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdminStories(
        [FromQuery(Name = "status")] string status,
        [FromQuery(Name = "genre-slug")] string genreSlug,
        [FromQuery(Name = "q")] string q,
        [FromQuery(Name = "author-profile-id")] long? authorProfileId,
        [FromQuery(Name = "sort-by")] string sortBy,
        [FromQuery(Name = "sort-direction")] string sortDirection,
        [FromQuery(Name = "page-number")] int pageNumber = 1,
        [FromQuery(Name = "page-size")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAdminStoriesQuery
        {
            Status = status,
            GenreSlug = genreSlug,
            Keyword = q,
            AuthorProfileId = authorProfileId,
            SortBy = sortBy,
            SortDirection = sortDirection,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(ResponseDto<PagedResponseDto<StorySummaryResponseDto>>.Ok(result));
    }

    /// <summary>Admin dashboard: story counts per lifecycle status.</summary>
    [HasPermission(StoryVersePermissions.Content.Moderate)]
    [HttpGet(ControllerRouteConstants.StoryAdminCountsSegment)]
    [ProducesResponseType(typeof(ResponseDto<StoryStatusCountsResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdminStoryCounts(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStoryStatusCountsQuery(), cancellationToken);

        return Ok(ResponseDto<StoryStatusCountsResponseDto>.Ok(result));
    }

    /// <summary>
    /// One-time/backfill: indexes every non-Draft story into Elasticsearch. Run
    /// once after the elasticsearch container is confirmed healthy, before
    /// flipping <c>Elasticsearch:SearchReadEnabled</c> on — see the
    /// Elasticsearch/Kibana rollout plan.
    /// </summary>
    [HasPermission(StoryVersePermissions.Content.Moderate)]
    [HttpPost(ControllerRouteConstants.StoryAdminReindexSearchSegment)]
    [ProducesResponseType(typeof(ResponseDto<ReindexAllStoriesResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReindexSearch(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ReindexAllStoriesCommand(), cancellationToken);

        return Ok(ResponseDto<ReindexAllStoriesResultDto>.Ok(result));
    }

    /// <summary>The signed-in author's own stories, Draft included.</summary>
    [Authorize]
    [HttpGet(ControllerRouteConstants.StoryMineSegment)]
    [ProducesResponseType(typeof(ResponseDto<PagedResponseDto<StorySummaryResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyStories(
        [FromQuery(Name = "status")] string status,
        [FromQuery(Name = "sort-by")] string sortBy,
        [FromQuery(Name = "sort-direction")] string sortDirection,
        [FromQuery(Name = "page-number")] int pageNumber = 1,
        [FromQuery(Name = "page-size")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMyStoriesQuery
        {
            Status = status,
            SortBy = sortBy,
            SortDirection = sortDirection,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(ResponseDto<PagedResponseDto<StorySummaryResponseDto>>.Ok(result));
    }

    [AllowAnonymous]
    [HttpGet(ControllerRouteConstants.StoryBySlugSegment)]
    [ProducesResponseType(typeof(ResponseDto<StoryDetailResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStoryBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStoryDetailQuery { Slug = slug }, cancellationToken);

        return Ok(ResponseDto<StoryDetailResponseDto>.Ok(result));
    }

    [AllowAnonymous]
    [HttpGet(ControllerRouteConstants.StoryByIdSegment)]
    [ProducesResponseType(typeof(ResponseDto<StoryDetailResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStoryById(Guid storyId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStoryByIdQuery { StoryId = storyId }, cancellationToken);

        return Ok(ResponseDto<StoryDetailResponseDto>.Ok(result));
    }

    /// <summary>
    /// One call: creates the story, assigns genres/tags, writes and publishes the
    /// first chapter. Set <c>completeImmediately</c> for a one-shot.
    /// </summary>
    [Authorize]
    [HttpPost(ControllerRouteConstants.StoryQuickPublishSegment)]
    [ProducesResponseType(typeof(ResponseDto<QuickPublishStoryResultDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> QuickPublish(
        [FromBody] QuickPublishStoryRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new QuickPublishStoryCommand
        {
            Title = request.Title,
            Description = request.Description,
            CoverImageUrl = request.CoverImageUrl,
            ContentType = request.ContentType,
            OriginalSource = request.OriginalSource,
            Language = request.Language,
            AgeRating = request.AgeRating,
            Genres = request.Genres
                .Select(g => new StoryGenreSelection(g.GenreSlug, g.IsPrimary))
                .ToArray(),
            Tags = request.Tags,
            ChapterTitle = request.ChapterTitle,
            ChapterContent = request.ChapterContent,
            CompleteImmediately = request.CompleteImmediately
        };

        var result = await _mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, ResponseDto<QuickPublishStoryResultDto>.Ok(result));
    }

    /// <summary>
    /// Anonymous one-call publish: no account, the guest types a pen name. Creates
    /// the story + first published chapter. The guest cannot edit it afterwards.
    /// </summary>
    [AllowAnonymous]
    [HttpPost(ControllerRouteConstants.StoryGuestPublishSegment)]
    [ProducesResponseType(typeof(ResponseDto<StoryDetailResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> GuestPublish(
        [FromBody] GuestPublishStoryRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new GuestPublishStoryCommand
        {
            GuestPenName = request.GuestPenName,
            Title = request.Title,
            Description = request.Description,
            Genres = request.Genres
                .Select(g => new StoryGenreSelection(g.GenreSlug, g.IsPrimary))
                .ToArray(),
            ChapterContent = request.ChapterContent
        };

        var result = await _mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, ResponseDto<StoryDetailResponseDto>.Ok(result));
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ResponseDto<StoryDetailResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateStory([FromBody] CreateStoryRequestDto request, CancellationToken cancellationToken)
    {
        var command = new CreateStoryCommand
        {
            Title = request.Title,
            Description = request.Description,
            CoverImageUrl = request.CoverImageUrl,
            ContentType = request.ContentType,
            OriginalSource = request.OriginalSource,
            Language = request.Language,
            AgeRating = request.AgeRating
        };

        var result = await _mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, ResponseDto<StoryDetailResponseDto>.Ok(result));
    }

    [Authorize]
    [HttpPut(ControllerRouteConstants.StoryByIdSegment)]
    [ProducesResponseType(typeof(ResponseDto<StoryDetailResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateStory(
        Guid storyId,
        [FromBody] UpdateStoryRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateStoryCommand
        {
            StoryId = storyId,
            Title = request.Title,
            Description = request.Description,
            CoverImageUrl = request.CoverImageUrl,
            ContentType = request.ContentType,
            OriginalSource = request.OriginalSource,
            Language = request.Language,
            AgeRating = request.AgeRating
        };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(ResponseDto<StoryDetailResponseDto>.Ok(result));
    }

    /// <summary>
    /// Hard-deletes a story. Only allowed while it is still Draft (never
    /// published, never seen publicly). Every other status must go through
    /// <c>POST {storyId}/status</c> instead. Owner only, or staff with
    /// <c>content.moderate</c>.
    /// </summary>
    [Authorize]
    [HttpDelete(ControllerRouteConstants.StoryByIdSegment)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteStory(Guid storyId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteStoryCommand { StoryId = storyId }, cancellationToken);

        return NoContent();
    }

    [Authorize]
    [HttpPut(ControllerRouteConstants.StoryGenresSegment)]
    [ProducesResponseType(typeof(ResponseDto<StoryDetailResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignGenres(
        Guid storyId,
        [FromBody] AssignStoryGenresRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new AssignStoryGenresCommand
        {
            StoryId = storyId,
            Genres = request.Genres
                .Select(g => new StoryGenreSelection(g.GenreSlug, g.IsPrimary))
                .ToArray()
        };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(ResponseDto<StoryDetailResponseDto>.Ok(result));
    }

    [Authorize]
    [HttpPut(ControllerRouteConstants.StoryTagsSegment)]
    [ProducesResponseType(typeof(ResponseDto<StoryDetailResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignTags(
        Guid storyId,
        [FromBody] AssignStoryTagsRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new AssignStoryTagsCommand { StoryId = storyId, Tags = request.Tags };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(ResponseDto<StoryDetailResponseDto>.Ok(result));
    }

    [Authorize]
    [HttpPost(ControllerRouteConstants.StoryStatusSegment)]
    [ProducesResponseType(typeof(ResponseDto<StoryDetailResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangeStatus(
        Guid storyId,
        [FromBody] ChangeStoryStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeStoryStatusCommand { StoryId = storyId, TargetStatus = request.TargetStatus };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(ResponseDto<StoryDetailResponseDto>.Ok(result));
    }

    [AllowAnonymous]
    [HttpGet(ControllerRouteConstants.StoryVolumesSegment)]
    [ProducesResponseType(typeof(ResponseDto<IReadOnlyList<VolumeResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVolumes(Guid storyId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStoryVolumesQuery { StoryId = storyId }, cancellationToken);

        return Ok(ResponseDto<IReadOnlyList<VolumeResponseDto>>.Ok(result));
    }

    [Authorize]
    [HttpPost(ControllerRouteConstants.StoryVolumesSegment)]
    [ProducesResponseType(typeof(ResponseDto<VolumeResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateVolume(
        Guid storyId,
        [FromBody] CreateVolumeRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new CreateVolumeCommand
        {
            StoryId = storyId,
            Title = request.Title,
            OrderIndex = request.OrderIndex
        };

        var result = await _mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, ResponseDto<VolumeResponseDto>.Ok(result));
    }

    /// <summary>Reorder every volume in a story. Owner only, or staff with <c>content.moderate</c>.</summary>
    [Authorize]
    [HttpPut(ControllerRouteConstants.StoryVolumesOrderSegment)]
    [ProducesResponseType(typeof(ResponseDto<IReadOnlyList<VolumeResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReorderVolumes(
        Guid storyId,
        [FromBody] ReorderVolumesRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new ReorderVolumesCommand
        {
            StoryId = storyId,
            OrderedVolumeIds = request.OrderedVolumeIds
        };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(ResponseDto<IReadOnlyList<VolumeResponseDto>>.Ok(result));
    }

    /// <summary>
    /// Reorder a story's chapters that belong to no volume. Owner only, or staff
    /// with <c>content.moderate</c>.
    /// </summary>
    [Authorize]
    [HttpPut(ControllerRouteConstants.StoryChaptersOrderSegment)]
    [ProducesResponseType(typeof(ResponseDto<IReadOnlyList<ChapterSummaryResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReorderStoryChapters(
        Guid storyId,
        [FromBody] ReorderChaptersRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new ReorderChaptersCommand
        {
            StoryId = storyId,
            OrderedChapterIds = request.OrderedChapterIds
        };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(ResponseDto<IReadOnlyList<ChapterSummaryResponseDto>>.Ok(result));
    }

    [AllowAnonymous]
    [HttpGet(ControllerRouteConstants.StoryChaptersSegment)]
    [ProducesResponseType(typeof(ResponseDto<IReadOnlyList<ChapterSummaryResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChapters(Guid storyId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStoryChaptersQuery { StoryId = storyId }, cancellationToken);

        return Ok(ResponseDto<IReadOnlyList<ChapterSummaryResponseDto>>.Ok(result));
    }

    [Authorize]
    [HttpPost(ControllerRouteConstants.StoryChaptersSegment)]
    [ProducesResponseType(typeof(ResponseDto<ChapterDetailResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateChapter(
        Guid storyId,
        [FromBody] CreateChapterRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new CreateChapterCommand
        {
            StoryId = storyId,
            Title = request.Title,
            OrderIndex = request.OrderIndex,
            Content = request.Content,
            VolumeId = request.VolumeId,
            PublishImmediately = request.PublishImmediately
        };

        var result = await _mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, ResponseDto<ChapterDetailResponseDto>.Ok(result));
    }
}
