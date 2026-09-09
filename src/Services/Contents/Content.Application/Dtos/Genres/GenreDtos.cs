namespace Content.Application.Dtos;

public sealed class CreateGenreRequestDto
{
    public string Name { get; init; } = string.Empty;

    public string Description { get; init; }

    public int DisplayOrder { get; init; }
}

public sealed class UpdateGenreRequestDto
{
    public string Name { get; init; } = string.Empty;

    public string Description { get; init; }

    public int DisplayOrder { get; init; }

    public bool IsActive { get; init; } = true;
}

public sealed class GenreResponseDto
{
    public string Name { get; init; }

    public string Slug { get; init; }

    public string Description { get; init; }

    public int DisplayOrder { get; init; }

    public bool IsActive { get; init; }
}
