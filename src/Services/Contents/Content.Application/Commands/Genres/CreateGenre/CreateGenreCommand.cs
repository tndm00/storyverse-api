namespace Content.Application.Commands.Genres.CreateGenre;

/// <summary>Adds a genre to the closed, admin-managed taxonomy.</summary>
public sealed class CreateGenreCommand : ICommand<GenreResponseDto>
{
    public string Name { get; init; } = string.Empty;

    public string Description { get; init; }

    public int DisplayOrder { get; init; }
}
