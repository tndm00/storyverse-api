namespace Content.Application.Commands.Genres.HideGenre;

/// <summary>
/// Hides a genre from new story selections without deleting it. Existing story
/// references are preserved (rules section 10.5).
/// </summary>
public sealed class HideGenreCommand : ICommand<GenreResponseDto>
{
    public string Slug { get; init; } = string.Empty;
}
