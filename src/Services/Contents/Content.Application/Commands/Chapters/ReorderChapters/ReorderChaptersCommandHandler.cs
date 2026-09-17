namespace Content.Application.Commands.Chapters.ReorderChapters;

public sealed class ReorderChaptersCommandHandler
    : ICommandHandler<ReorderChaptersCommand, IReadOnlyList<ChapterSummaryResponseDto>>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IVolumeRepository _volumeRepository;
    private readonly ICurrentAuthorContext _authorContext;
    private readonly ILogger<ReorderChaptersCommandHandler> _logger;

    public ReorderChaptersCommandHandler(
        IStoryRepository storyRepository,
        IChapterRepository chapterRepository,
        IVolumeRepository volumeRepository,
        ICurrentAuthorContext authorContext,
        ILogger<ReorderChaptersCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;
        _volumeRepository = volumeRepository;
        _authorContext = authorContext;
        _logger = logger;
    }

    /// <summary>
    /// Reorders chapters within a single scope (a volume, or a story's
    /// volume-less chapters) to match the given sequence.
    /// </summary>
    public async Task<IReadOnlyList<ChapterSummaryResponseDto>> Handle(
        ReorderChaptersCommand request,
        CancellationToken cancellationToken)
    {
        Volume volume = null;
        Story story;
        IReadOnlyList<Chapter> chapters;

        // Resolve the scope (volume or volume-less story chapters) and ensure
        // the caller may mutate the parent story.
        if (request.VolumeId is { } volumePublicId)
        {
            volume = await _volumeRepository.GetByPublicIdAsync(volumePublicId, cancellationToken)
                ?? throw new NotFoundException(ApplicationErrorConstants.VolumeNotFound);

            story = ReorderPolicy.EnsureStoryMutable(
                await _storyRepository.GetByIdAsync(volume.StoryId, cancellationToken),
                _authorContext,
                _logger);

            chapters = await _chapterRepository.GetByVolumeTrackedAsync(volume.Id, cancellationToken);
        }
        else
        {
            story = ReorderPolicy.EnsureStoryMutable(
                await _storyRepository.GetByPublicIdAsync(request.StoryId!.Value, cancellationToken),
                _authorContext,
                _logger);

            chapters = await _chapterRepository.GetStoryChaptersWithoutVolumeTrackedAsync(
                story.Id, cancellationToken);
        }

        // The requested sequence must contain exactly the chapters in scope.
        ReorderPolicy.EnsureExactMatch(
            request.OrderedChapterIds, chapters.Select(c => c.PublicId).ToArray());

        var byPublicId = chapters.ToDictionary(c => c.PublicId);
        var now = DateTime.UtcNow;

        // Assign order index by position in the requested sequence.
        for (var position = 0; position < request.OrderedChapterIds.Count; position++)
        {
            var chapter = byPublicId[request.OrderedChapterIds[position]];
            chapter.OrderIndex = position + 1;
            chapter.UpdatedAt = now;
            _chapterRepository.Update(chapter);
        }

        await _chapterRepository.SaveChangesAsync(cancellationToken);

        var volumeResultId = volume?.PublicId;

        return request.OrderedChapterIds
            .Select(id => ContentDtoMapper.ToSummary(byPublicId[id], volumeResultId))
            .ToArray();
    }
}
