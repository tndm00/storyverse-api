namespace Content.Application.Queries.Volumes.GetStoryVolumes;

/// <summary>Volumes of a story, ordered for display.</summary>
public sealed class GetStoryVolumesQuery : IQuery<IReadOnlyList<VolumeResponseDto>>
{
    public Guid StoryId { get; init; }
}
