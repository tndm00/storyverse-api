namespace Content.Application.Dtos;

public sealed class TagResponseDto
{
    public string Name { get; init; }

    public string Slug { get; init; }

    public int UsageCount { get; init; }
}
