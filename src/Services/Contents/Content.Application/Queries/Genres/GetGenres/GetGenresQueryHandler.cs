namespace Content.Application.Queries.Genres.GetGenres;

public sealed class GetGenresQueryHandler : IQueryHandler<GetGenresQuery, IReadOnlyList<GenreResponseDto>>
{
    private readonly IGenreRepository _genreRepository;

    public GetGenresQueryHandler(IGenreRepository genreRepository)
    {
        _genreRepository = genreRepository;
    }

    /// <summary>Returns the ordered genre list, including inactive genres only when requested.</summary>
    public async Task<IReadOnlyList<GenreResponseDto>> Handle(GetGenresQuery request, CancellationToken cancellationToken)
    {
        // Admin management view includes inactive genres; discovery/filter UIs see active only.
        var genres = request.IncludeInactive
            ? await _genreRepository.GetAllOrderedAsync(cancellationToken)
            : await _genreRepository.GetActiveOrderedAsync(cancellationToken);

        return genres.Select(ContentDtoMapper.ToDto).ToArray();
    }
}
