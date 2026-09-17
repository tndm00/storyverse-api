namespace Content.Application.Commands.Genres.HideGenre;

// TODO: restrict to an Admin role policy once the Authentication service issues roles.
public sealed class HideGenreCommandHandler : ICommandHandler<HideGenreCommand, GenreResponseDto>
{
    private readonly IGenreRepository _genreRepository;
    private readonly ILogger<HideGenreCommandHandler> _logger;

    /// <summary>Initializes the handler with the genre repository and logger it depends on.</summary>
    public HideGenreCommandHandler(IGenreRepository genreRepository, ILogger<HideGenreCommandHandler> logger)
    {
        _genreRepository = genreRepository;
        _logger = logger;
    }

    /// <summary>Soft-hides a genre by slug so it no longer appears in new story selections.</summary>
    public async Task<GenreResponseDto> Handle(HideGenreCommand request, CancellationToken cancellationToken)
    {
        // Look up the genre by its immutable slug.
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
