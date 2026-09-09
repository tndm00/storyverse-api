namespace Content.Application.Queries.Genres.GetGenres;

public sealed class GetGenresQueryHandler : IQueryHandler<GetGenresQuery, IReadOnlyList<GenreResponseDto>>
{
    private readonly IGenreRepository _genreRepository;

    public GetGenresQueryHandler(IGenreRepository genreRepository)
    {
        _genreRepository = genreRepository;
    }

    public async Task<IReadOnlyList<GenreResponseDto>> Handle(GetGenresQuery request, CancellationToken cancellationToken)
    {
        var genres = request.IncludeInactive
            ? await _genreRepository.GetAllOrderedAsync(cancellationToken)
            : await _genreRepository.GetActiveOrderedAsync(cancellationToken);

        return genres.Select(ContentDtoMapper.ToDto).ToArray();
    }
}
