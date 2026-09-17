namespace Authentication.Application.Queries.AuthorProfileLookup;

/// <summary>Handles <see cref="GetAuthorProfileLookupQuery"/>.</summary>
public sealed class GetAuthorProfileLookupQueryHandler
    : IQueryHandler<GetAuthorProfileLookupQuery, AuthorProfileLookupResponseDto>
{
    private readonly IAuthorProfileRepository _authorProfileRepository;

    public GetAuthorProfileLookupQueryHandler(IAuthorProfileRepository authorProfileRepository)
    {
        _authorProfileRepository = authorProfileRepository;
    }

    /// <summary>Resolves the owning user id for an author-profile id, or throws if unknown.</summary>
    public async Task<AuthorProfileLookupResponseDto> Handle(
        GetAuthorProfileLookupQuery request,
        CancellationToken cancellationToken)
    {
        // Fetch the profile by id; unknown id is a 404.
        var profile = await _authorProfileRepository.GetByIdAsync(request.AuthorProfileId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.AuthorProfileNotFound);

        return new AuthorProfileLookupResponseDto
        {
            AuthorProfileId = profile.Id,
            UserId = profile.UserId
        };
    }
}
