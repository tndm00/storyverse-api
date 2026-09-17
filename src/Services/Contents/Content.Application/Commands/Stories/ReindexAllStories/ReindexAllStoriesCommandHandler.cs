namespace Content.Application.Commands.Stories.ReindexAllStories;

public sealed class ReindexAllStoriesCommandHandler
    : ICommandHandler<ReindexAllStoriesCommand, ReindexAllStoriesResultDto>
{
    private const int PageSize = 100;

    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IStorySearchService _storySearchService;
    private readonly ILogger<ReindexAllStoriesCommandHandler> _logger;

    public ReindexAllStoriesCommandHandler(
        IStoryRepository storyRepository,
        IChapterRepository chapterRepository,
        IStorySearchService storySearchService,
        ILogger<ReindexAllStoriesCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;
        _storySearchService = storySearchService;
        _logger = logger;
    }

    /// <summary>Pages through every non-draft story and (re)indexes it into Elasticsearch along with its published chapter content.</summary>
    public async Task<ReindexAllStoriesResultDto> Handle(ReindexAllStoriesCommand request, CancellationToken cancellationToken)
    {
        var storyCount = 0;
        var afterId = 0L;

        // Keyset-paginate through all public stories until no more pages remain.
        while (true)
        {
            var page = await _storyRepository.GetAllPublicPagedAsync(afterId, PageSize, cancellationToken);
            if (page.Count == 0)
            {
                break;
            }

            // Index each story together with its currently published chapter content.
            foreach (var story in page)
            {
                var publishedContent = await _chapterRepository.GetPublishedContentByStoryIdAsync(story.Id, cancellationToken);
                await _storySearchService.IndexAsync(story, publishedContent, cancellationToken);
                storyCount++;
            }

            afterId = page[^1].Id;
        }

        _logger.LogInformation(ApplicationLogConstants.StoryReindexCompleted, storyCount);

        return new ReindexAllStoriesResultDto { StoryCount = storyCount };
    }
}
