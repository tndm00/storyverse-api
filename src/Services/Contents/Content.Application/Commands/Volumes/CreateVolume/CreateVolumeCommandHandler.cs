namespace Content.Application.Commands.Volumes.CreateVolume;

/// <summary>Handles <see cref="CreateVolumeCommand"/> by creating a new volume under a story the caller owns.</summary>
public sealed class CreateVolumeCommandHandler : ICommandHandler<CreateVolumeCommand, VolumeResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IVolumeRepository _volumeRepository;
    private readonly ICurrentAuthorContext _authorContext;
    private readonly ILogger<CreateVolumeCommandHandler> _logger;

    /// <summary>Initializes the handler with the repositories and context needed to create a volume.</summary>
    public CreateVolumeCommandHandler(
        IStoryRepository storyRepository,
        IVolumeRepository volumeRepository,
        ICurrentAuthorContext authorContext,
        ILogger<CreateVolumeCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _volumeRepository = volumeRepository;
        _authorContext = authorContext;
        _logger = logger;
    }

    /// <summary>Validates ownership of the target story, then creates and persists the new volume.</summary>
    public async Task<VolumeResponseDto> Handle(CreateVolumeCommand request, CancellationToken cancellationToken)
    {
        var authorProfileId = _authorContext.GetAuthorProfileId();

        // Ensure the story exists and belongs to the requesting author before allowing mutation.
        var story = StoryOwnership.EnsureOwned(
            await _storyRepository.GetByPublicIdAsync(request.StoryId, cancellationToken),
            authorProfileId,
            _logger);

        // Use the caller-supplied order index when given; otherwise append after the current max with a gap
        // so volumes can later be reordered/inserted without renumbering everything.
        var orderIndex = request.OrderIndex > 0
            ? request.OrderIndex
            : (await _volumeRepository.GetMaxOrderIndexAsync(story.Id, cancellationToken) ?? 0)
              + ApplicationConstants.VolumeOrderIndexGap;

        var volume = new Volume
        {
            StoryId = story.Id,
            Title = request.Title.Trim(),
            OrderIndex = orderIndex
        };

        // Persist the new volume.
        await _volumeRepository.AddAsync(volume, cancellationToken);
        await _volumeRepository.SaveChangesAsync(cancellationToken);

        return ContentDtoMapper.ToDto(volume);
    }
}
