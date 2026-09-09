namespace Content.Application.Commands.Genres.UpdateGenre;

// TODO: restrict to an Admin role policy once the Authentication service issues roles.
public sealed class UpdateGenreCommandHandler : ICommandHandler<UpdateGenreCommand, GenreResponseDto>
{
    private readonly IGenreRepository _genreRepository;
    private readonly ILogger<UpdateGenreCommandHandler> _logger;

    public UpdateGenreCommandHandler(IGenreRepository genreRepository, ILogger<UpdateGenreCommandHandler> logger)
    {
        _genreRepository = genreRepository;
        _logger = logger;
    }

    public async Task<GenreResponseDto> Handle(UpdateGenreCommand request, CancellationToken cancellationToken)
    {
        var genre = await _genreRepository.GetBySlugAsync(request.Slug, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.GenreNotFound);

        var name = request.Name.Trim();
        if (!string.Equals(name, genre.Name, StringComparison.Ordinal)
            && await _genreRepository.NameExistsAsync(name, cancellationToken))
        {
            throw new ConflictException(ApplicationErrorConstants.GenreNameAlreadyUsed);
        }

        genre.Name = name;
        genre.Description = request.Description;
        genre.DisplayOrder = request.DisplayOrder;
        genre.IsActive = request.IsActive;
        genre.UpdatedAt = DateTime.UtcNow;

        _genreRepository.Update(genre);
        await _genreRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.GenreUpdated, genre.Id);

        return ContentDtoMapper.ToDto(genre);
    }
}
