namespace Content.Application.Commands.Volumes.UpdateVolume;

/// <summary>Handles <see cref="UpdateVolumeCommand"/> by updating a volume's title/order for its owner.</summary>
public sealed class UpdateVolumeCommandHandler : ICommandHandler<UpdateVolumeCommand, VolumeResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IVolumeRepository _volumeRepository;
    private readonly ICurrentAuthorContext _authorContext;
    private readonly ILogger<UpdateVolumeCommandHandler> _logger;

    /// <summary>Initializes the handler with the repositories and context needed to update a volume.</summary>
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

    /// <summary>Validates the volume exists and its story is owned by the caller, then applies the update.</summary>
    public async Task<VolumeResponseDto> Handle(UpdateVolumeCommand request, CancellationToken cancellationToken)
    {
        var authorProfileId = _authorContext.GetAuthorProfileId();

        // Volume must exist.
        var volume = await _volumeRepository.GetByPublicIdAsync(request.VolumeId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.VolumeNotFound);

        // The parent story must belong to the requesting author.
        StoryOwnership.EnsureOwned(
            await _storyRepository.GetByIdAsync(volume.StoryId, cancellationToken),
            authorProfileId,
            _logger);

        // Apply the requested changes.
        volume.Title = request.Title.Trim();
        volume.OrderIndex = request.OrderIndex;
        volume.UpdatedAt = DateTime.UtcNow;

        _volumeRepository.Update(volume);
        await _volumeRepository.SaveChangesAsync(cancellationToken);

        return ContentDtoMapper.ToDto(volume);
    }
}
