namespace Content.Application.Commands.Genres.CreateGenre;

// TODO: restrict to an Admin role policy once the Authentication service issues roles.
public sealed class CreateGenreCommandHandler : ICommandHandler<CreateGenreCommand, GenreResponseDto>
{
    private readonly IGenreRepository _genreRepository;
    private readonly ILogger<CreateGenreCommandHandler> _logger;

    /// <summary>Initializes the handler with the genre repository and logger it depends on.</summary>
    public CreateGenreCommandHandler(IGenreRepository genreRepository, ILogger<CreateGenreCommandHandler> logger)
    {
        _genreRepository = genreRepository;
        _logger = logger;
    }

    /// <summary>Creates a new genre with a unique name and a slug derived from it.</summary>
    public async Task<GenreResponseDto> Handle(CreateGenreCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();

        // Enforce name uniqueness across the taxonomy.
        if (await _genreRepository.NameExistsAsync(name, cancellationToken))
        {
            throw new ConflictException(ApplicationErrorConstants.GenreNameAlreadyUsed);
        }

        var genre = new Genre
        {
            Name = name,
            Slug = SlugGenerator.Generate(name),
            Description = request.Description,
            DisplayOrder = request.DisplayOrder,
            IsActive = true
        };

        // Persist the new genre.
        await _genreRepository.AddAsync(genre, cancellationToken);
        await _genreRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.GenreCreated, genre.Id);

        return ContentDtoMapper.ToDto(genre);
    }
}
