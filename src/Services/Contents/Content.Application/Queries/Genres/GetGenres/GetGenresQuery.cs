namespace Content.Application.Queries.Genres.GetGenres;

/// <summary>
/// Genres ordered for display. By default only active genres (discovery/filter
/// UIs); <see cref="IncludeInactive"/> also returns hidden ones for the admin
/// management page.
/// </summary>
public sealed class GetGenresQuery : IQuery<IReadOnlyList<GenreResponseDto>>
{
    public bool IncludeInactive { get; init; }
}
