namespace Content.Application.Commands.Marketing.PostFacebookDigest;

public sealed class PostFacebookDigestCommandHandler
    : ICommandHandler<PostFacebookDigestCommand, PostFacebookDigestResultDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IFacebookPageClient _facebookPageClient;
    private readonly ILogger<PostFacebookDigestCommandHandler> _logger;

    public PostFacebookDigestCommandHandler(
        IStoryRepository storyRepository,
        IFacebookPageClient facebookPageClient,
        ILogger<PostFacebookDigestCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _facebookPageClient = facebookPageClient;
        _logger = logger;
    }

    public async Task<PostFacebookDigestResultDto> Handle(
        PostFacebookDigestCommand request, CancellationToken cancellationToken)
    {
        var criteria = new StorySearchCriteria
        {
            SortBy = StorySortField.ViewCount,
            Descending = true,
            PageNumber = 1,
            PageSize = request.TopCount
        };

        var (stories, _) = await _storyRepository.SearchPublishedAsync(criteria, cancellationToken);

        if (stories.Count == 0)
        {
            return new PostFacebookDigestResultDto { StoryCount = 0 };
        }

        var message = BuildMessage(stories);

        try
        {
            await _facebookPageClient.PostAsync(message, $"{ApplicationConstants.SiteBaseUrl}/truyen-hay", cancellationToken);
            _logger.LogInformation(ApplicationLogConstants.FacebookDigestPosted, stories.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, ApplicationLogConstants.FacebookDigestFailed);
        }

        return new PostFacebookDigestResultDto { StoryCount = stories.Count };
    }

    private static string BuildMessage(IReadOnlyList<Story> stories)
    {
        var builder = new StringBuilder("👻 Truyện ma hot hôm nay:\n\n");

        for (var i = 0; i < stories.Count; i++)
        {
            var story = stories[i];
            builder.Append($"{i + 1}. {story.Title} — {ApplicationConstants.SiteBaseUrl}/truyen/{story.Slug}\n");
        }

        return builder.ToString();
    }
}
