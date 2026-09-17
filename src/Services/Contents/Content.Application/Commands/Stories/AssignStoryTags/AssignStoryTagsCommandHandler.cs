namespace Content.Application.Commands.Stories.AssignStoryTags;

public sealed class AssignStoryTagsCommandHandler : ICommandHandler<AssignStoryTagsCommand, StoryDetailResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly ITagRepository _tagRepository;
    private readonly ICurrentAuthorContext _authorContext;
    private readonly ILogger<AssignStoryTagsCommandHandler> _logger;

    public AssignStoryTagsCommandHandler(
        IStoryRepository storyRepository,
        ITagRepository tagRepository,
        ICurrentAuthorContext authorContext,
        ILogger<AssignStoryTagsCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _tagRepository = tagRepository;
        _authorContext = authorContext;
        _logger = logger;
    }

    /// <summary>
    /// Replaces a story's tag assignments, creating any new tags on demand and keeping each
    /// tag's usage counter in sync with the add/remove diff.
    /// </summary>
    public async Task<StoryDetailResponseDto> Handle(AssignStoryTagsCommand request, CancellationToken cancellationToken)
    {
        var authorProfileId = _authorContext.GetAuthorProfileId();

        // Load the story with its classification data (genres/tags) included.
        var story = await _storyRepository.GetWithClassificationByPublicIdAsync(request.StoryId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);

        // Only the owning author may reassign tags.
        if (story.AuthorProfileId != authorProfileId)
        {
            _logger.LogWarning(ApplicationLogConstants.OwnershipCheckFailed, authorProfileId, story.Id);
            throw new ForbiddenException(ApplicationErrorConstants.NotStoryOwner);
        }

        // Normalize requested tag names into unique, non-blank slugs.
        var normalized = request.Tags
            .Select(name => (Name: (name ?? string.Empty).Trim(), Slug: SlugGenerator.Generate(name ?? string.Empty)))
            .Where(t => t.Slug.Length > 0)
            .GroupBy(t => t.Slug)
            .Select(g => g.First())
            .ToArray();

        // Resolve to existing tags, creating any that don't exist yet.
        var resolvedTags = normalized.Length == 0
            ? Array.Empty<Tag>()
            : (await _tagRepository.GetOrCreateBySlugAsync(normalized, cancellationToken)).ToArray();

        // Decrement usage on tags being removed, increment on tags newly added.
        var newSlugs = resolvedTags.Select(t => t.Slug).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var currentSlugs = story.Tags
            .Where(st => st.Tag is not null)
            .Select(st => st.Tag.Slug)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var storyTag in story.Tags.Where(st => st.Tag is not null && !newSlugs.Contains(st.Tag.Slug)))
        {
            storyTag.Tag.UsageCount = Math.Max(0, storyTag.Tag.UsageCount - 1);
        }

        foreach (var tag in resolvedTags.Where(t => !currentSlugs.Contains(t.Slug)))
        {
            tag.UsageCount += 1;
        }

        // Replace the story's entire tag set with the resolved tags.
        story.Tags.Clear();
        foreach (var tag in resolvedTags)
        {
            story.Tags.Add(new StoryTag { StoryId = story.Id, TagId = tag.Id, Tag = tag });
        }

        // Persist the change.
        story.UpdatedAt = DateTime.UtcNow;
        _storyRepository.Update(story);
        await _storyRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.StoryTagsAssigned, story.Id, story.Tags.Count);

        return ContentDtoMapper.ToDetail(story);
    }
}
