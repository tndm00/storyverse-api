namespace Content.Application.Commands.Volumes.ReorderVolumes;

/// <summary>Handles <see cref="ReorderVolumesCommand"/> by applying a caller-supplied volume order to a story.</summary>
public sealed class ReorderVolumesCommandHandler
    : ICommandHandler<ReorderVolumesCommand, IReadOnlyList<VolumeResponseDto>>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IVolumeRepository _volumeRepository;
    private readonly ICurrentAuthorContext _authorContext;
    private readonly ILogger<ReorderVolumesCommandHandler> _logger;

    /// <summary>Initializes the handler with the repositories and context needed to reorder volumes.</summary>
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

    /// <summary>Re-applies order indexes to all volumes of a story based on the given sequence.</summary>
    public async Task<IReadOnlyList<VolumeResponseDto>> Handle(
        ReorderVolumesCommand request,
        CancellationToken cancellationToken)
    {
        // Ensure the story exists and is mutable by the caller (owner, or staff with content.moderate).
        var story = ReorderPolicy.EnsureStoryMutable(
            await _storyRepository.GetByPublicIdAsync(request.StoryId, cancellationToken),
            _authorContext,
            _logger);

        // Load all volumes of the story as tracked entities so order changes can be saved.
        var volumes = await _volumeRepository.GetByStoryTrackedAsync(story.Id, cancellationToken);

        // The request must reference exactly the same set of volumes as the story currently has.
        ReorderPolicy.EnsureExactMatch(request.OrderedVolumeIds, volumes.Select(v => v.PublicId).ToArray());

        var byPublicId = volumes.ToDictionary(v => v.PublicId);
        var now = DateTime.UtcNow;

        // Assign a new 1-based order index to each volume based on its position in the requested sequence.
        for (var position = 0; position < request.OrderedVolumeIds.Count; position++)
        {
            var volume = byPublicId[request.OrderedVolumeIds[position]];
            volume.OrderIndex = position + 1;
            volume.UpdatedAt = now;
            _volumeRepository.Update(volume);
        }

        await _volumeRepository.SaveChangesAsync(cancellationToken);

        // Return the volumes in their new order.
        return request.OrderedVolumeIds
            .Select(id => ContentDtoMapper.ToDto(byPublicId[id]))
            .ToArray();
    }
}
