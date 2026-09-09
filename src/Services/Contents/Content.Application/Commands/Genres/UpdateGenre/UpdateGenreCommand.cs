namespace Content.Application.Commands.Genres.UpdateGenre;

/// <summary>Updates an existing genre. The slug is immutable once created.</summary>
public sealed class UpdateGenreCommand : ICommand<GenreResponseDto>
{
    public string Slug { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; }

    public int DisplayOrder { get; init; }

    public bool IsActive { get; init; } = true;
}
