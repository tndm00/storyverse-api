namespace Content.Application.Commands.Volumes.UpdateVolume;

public sealed class UpdateVolumeCommandHandler : ICommandHandler<UpdateVolumeCommand, VolumeResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IVolumeRepository _volumeRepository;
    private readonly ICurrentAuthorContext _authorContext;
    private readonly ILogger<UpdateVolumeCommandHandler> _logger;

    public UpdateVolumeCommandHandler(
        IStoryRepository storyRepository,
        IVolumeRepository volumeRepository,
        ICurrentAuthorContext authorContext,
        ILogger<UpdateVolumeCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _volumeRepository = volumeRepository;
        _authorContext = authorContext;
        _logger = logger;
    }

    public async Task<VolumeResponseDto> Handle(UpdateVolumeCommand request, CancellationToken cancellationToken)
    {
        var authorProfileId = _authorContext.GetAuthorProfileId();

        var volume = await _volumeRepository.GetByPublicIdAsync(request.VolumeId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.VolumeNotFound);

        StoryOwnership.EnsureOwned(
            await _storyRepository.GetByIdAsync(volume.StoryId, cancellationToken),
            authorProfileId,
            _logger);

        volume.Title = request.Title.Trim();
        volume.OrderIndex = request.OrderIndex;
        volume.UpdatedAt = DateTime.UtcNow;

        _volumeRepository.Update(volume);
        await _volumeRepository.SaveChangesAsync(cancellationToken);

        return ContentDtoMapper.ToDto(volume);
    }
}
