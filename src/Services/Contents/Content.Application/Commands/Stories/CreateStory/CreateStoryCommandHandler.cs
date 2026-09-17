namespace Content.Application.Commands.Stories.CreateStory;

public sealed class CreateStoryCommandHandler : ICommandHandler<CreateStoryCommand, StoryDetailResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly ICurrentAuthorContext _authorContext;
    private readonly ILogger<CreateStoryCommandHandler> _logger;

    public CreateStoryCommandHandler(
        IStoryRepository storyRepository,
        ICurrentAuthorContext authorContext,
        ILogger<CreateStoryCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _authorContext = authorContext;
        _logger = logger;
    }

    /// <summary>Creates a new draft story for the calling author, generating a unique slug from the title.</summary>
    public async Task<StoryDetailResponseDto> Handle(CreateStoryCommand request, CancellationToken cancellationToken)
    {
        var authorProfileId = _authorContext.GetAuthorProfileId();

        // Slug is derived from the title and must be unique across all stories.
        var slug = await BuildUniqueSlugAsync(request.Title, cancellationToken);

        // New stories always start as drafts, regardless of requested content type.
        var story = new Story
        {
            AuthorProfileId = authorProfileId,
            Title = request.Title.Trim(),
            Slug = slug,
            Description = request.Description,
            CoverImageUrl = request.CoverImageUrl,
            Status = StoryStatus.Draft,
            ContentType = request.ContentType,
            OriginalSource = request.ContentType == StoryContentType.Translated ? request.OriginalSource : null,
            Language = string.IsNullOrWhiteSpace(request.Language) ? ApplicationConstants.DefaultLanguage : request.Language.Trim(),
            AgeRating = request.AgeRating
        };

        // Persist the new story.
        await _storyRepository.AddAsync(story, cancellationToken);
        await _storyRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.StoryCreated, story.Id, authorProfileId);

        return ContentDtoMapper.ToDetail(story, Array.Empty<StoryGenreDto>(), Array.Empty<string>());
    }

    /// <summary>Generates a URL-safe slug from the title and appends a numeric suffix until it no longer collides with an existing story.</summary>
    private async Task<string> BuildUniqueSlugAsync(string title, CancellationToken cancellationToken)
    {
        // Fall back to a random slug if the title yields nothing usable.
        var baseSlug = SlugGenerator.Generate(title);
        if (string.IsNullOrEmpty(baseSlug))
        {
            baseSlug = Guid.NewGuid().ToString("n")[..8];
        }

        // Keep incrementing the suffix until a non-colliding slug is found.
        var candidate = baseSlug;
        var suffix = 2;
        while (await _storyRepository.SlugExistsAsync(candidate, cancellationToken))
        {
            candidate = $"{baseSlug}{ApplicationConstants.SlugCollisionSeparator}{suffix}";
            suffix++;
        }

        return candidate;
    }
}
