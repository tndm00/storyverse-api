namespace Content.Application.Commands.Volumes.UpdateVolume;

/// <summary>Updates a volume's title/order. Owner only.</summary>
public sealed class UpdateVolumeCommand : ICommand<VolumeResponseDto>
{
    public Guid VolumeId { get; init; }

    public string Title { get; init; } = string.Empty;

    public int OrderIndex { get; init; }
}
