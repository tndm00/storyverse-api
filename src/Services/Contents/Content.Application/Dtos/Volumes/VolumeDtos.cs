namespace Content.Application.Dtos;

public sealed class CreateVolumeRequestDto
{
    public string Title { get; init; } = string.Empty;

    /// <summary><c>0</c> (default) appends after the last volume.</summary>
    public int OrderIndex { get; init; }
}

public sealed class UpdateVolumeRequestDto
{
    public string Title { get; init; } = string.Empty;

    public int OrderIndex { get; init; }
}

public sealed class VolumeResponseDto
{
    public Guid Id { get; init; }

    public string Title { get; init; }

    public int OrderIndex { get; init; }
}
