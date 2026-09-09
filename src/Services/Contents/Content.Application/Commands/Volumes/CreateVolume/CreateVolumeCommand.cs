namespace Content.Application.Commands.Volumes.CreateVolume;

/// <summary>Creates a volume (chapter grouping) inside a story. Owner only.</summary>
public sealed class CreateVolumeCommand : ICommand<VolumeResponseDto>
{
    public Guid StoryId { get; init; }

    public string Title { get; init; } = string.Empty;

    public int OrderIndex { get; init; }
}
