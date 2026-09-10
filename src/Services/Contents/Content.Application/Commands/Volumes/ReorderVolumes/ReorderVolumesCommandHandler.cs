namespace Content.Application.Commands.Volumes.ReorderVolumes;

public sealed class ReorderVolumesCommandHandler
    : ICommandHandler<ReorderVolumesCommand, IReadOnlyList<VolumeResponseDto>>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IVolumeRepository _volumeRepository;
    private readonly ICurrentAuthorContext _authorContext;
    private readonly ILogger<ReorderVolumesCommandHandler> _logger;

    public ReorderVolumesCommandHandler(
        IStoryRepository storyRepository,
        IVolumeRepository volumeRepository,
        ICurrentAuthorContext authorContext,
        ILogger<ReorderVolumesCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _volumeRepository = volumeRepository;
        _authorContext = authorContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<VolumeResponseDto>> Handle(
        ReorderVolumesCommand request,
        CancellationToken cancellationToken)
    {
        var story = ReorderPolicy.EnsureStoryMutable(
            await _storyRepository.GetByPublicIdAsync(request.StoryId, cancellationToken),
            _authorContext,
            _logger);

        var volumes = await _volumeRepository.GetByStoryTrackedAsync(story.Id, cancellationToken);

        ReorderPolicy.EnsureExactMatch(request.OrderedVolumeIds, volumes.Select(v => v.PublicId).ToArray());

        var byPublicId = volumes.ToDictionary(v => v.PublicId);
        var now = DateTime.UtcNow;

        for (var position = 0; position < request.OrderedVolumeIds.Count; position++)
        {
            var volume = byPublicId[request.OrderedVolumeIds[position]];
            volume.OrderIndex = position + 1;
            volume.UpdatedAt = now;
            _volumeRepository.Update(volume);
        }

        await _volumeRepository.SaveChangesAsync(cancellationToken);

        return request.OrderedVolumeIds
            .Select(id => ContentDtoMapper.ToDto(byPublicId[id]))
            .ToArray();
    }
}
