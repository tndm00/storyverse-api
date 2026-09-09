namespace Content.Application.Commands.Volumes.CreateVolume;

public sealed class CreateVolumeCommandHandler : ICommandHandler<CreateVolumeCommand, VolumeResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IVolumeRepository _volumeRepository;
    private readonly ICurrentAuthorContext _authorContext;
    private readonly ILogger<CreateVolumeCommandHandler> _logger;

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

    public async Task<VolumeResponseDto> Handle(CreateVolumeCommand request, CancellationToken cancellationToken)
    {
        var authorProfileId = _authorContext.GetAuthorProfileId();

        var story = StoryOwnership.EnsureOwned(
            await _storyRepository.GetByPublicIdAsync(request.StoryId, cancellationToken),
            authorProfileId,
            _logger);

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

        await _volumeRepository.AddAsync(volume, cancellationToken);
        await _volumeRepository.SaveChangesAsync(cancellationToken);

        return ContentDtoMapper.ToDto(volume);
    }
}
