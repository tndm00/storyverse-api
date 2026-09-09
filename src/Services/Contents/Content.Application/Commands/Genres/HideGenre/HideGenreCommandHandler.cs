namespace Content.Application.Commands.Genres.HideGenre;

// TODO: restrict to an Admin role policy once the Authentication service issues roles.
public sealed class HideGenreCommandHandler : ICommandHandler<HideGenreCommand, GenreResponseDto>
{
    private readonly IGenreRepository _genreRepository;
    private readonly ILogger<HideGenreCommandHandler> _logger;

    public HideGenreCommandHandler(IGenreRepository genreRepository, ILogger<HideGenreCommandHandler> logger)
    {
        _genreRepository = genreRepository;
        _logger = logger;
    }

    public async Task<GenreResponseDto> Handle(HideGenreCommand request, CancellationToken cancellationToken)
    {
        var genre = await _genreRepository.GetBySlugAsync(request.Slug, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.GenreNotFound);

        genre.IsActive = false;
        genre.UpdatedAt = DateTime.UtcNow;

        _genreRepository.Update(genre);
        await _genreRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.GenreHidden, genre.Id);

        return ContentDtoMapper.ToDto(genre);
    }
}
