namespace Content.Application.Commands.Volumes.ReorderVolumes;

/// <summary>
/// Sets the order of every volume in a story from a client-provided sequence.
/// Story owner only, or staff with <c>content.moderate</c>.
/// </summary>
public sealed class ReorderVolumesCommand : ICommand<IReadOnlyList<VolumeResponseDto>>
{
    public Guid StoryId { get; init; }

    public IReadOnlyList<Guid> OrderedVolumeIds { get; init; } = Array.Empty<Guid>();
}
