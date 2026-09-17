namespace Content.Application.Queries.Volumes.GetStoryVolumes;

public sealed class GetStoryVolumesQueryHandler
    : IQueryHandler<GetStoryVolumesQuery, IReadOnlyList<VolumeResponseDto>>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IVolumeRepository _volumeRepository;

    public GetStoryVolumesQueryHandler(IStoryRepository storyRepository, IVolumeRepository volumeRepository)
    {
        _storyRepository = storyRepository;
        _volumeRepository = volumeRepository;
    }

    /// <summary>Returns a story's volumes ordered for display.</summary>
    public async Task<IReadOnlyList<VolumeResponseDto>> Handle(
        GetStoryVolumesQuery request,
        CancellationToken cancellationToken)
    {
        // Resolve the story; it must exist.
        var story = await _storyRepository.GetByPublicIdAsync(request.StoryId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);

        var volumes = await _volumeRepository.GetByStoryAsync(story.Id, cancellationToken);

        // Order volumes for display and map to DTOs.
        return volumes
            .OrderBy(v => v.OrderIndex)
            .Select(ContentDtoMapper.ToDto)
            .ToArray();
    }
}
