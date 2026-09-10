namespace Authentication.Application.Queries.AuthorProfileLookup;

public sealed class GetAuthorProfileLookupQueryHandler
    : IQueryHandler<GetAuthorProfileLookupQuery, AuthorProfileLookupResponseDto>
{
    private readonly IAuthorProfileRepository _authorProfileRepository;

    public GetAuthorProfileLookupQueryHandler(IAuthorProfileRepository authorProfileRepository)
    {
        _authorProfileRepository = authorProfileRepository;
    }

    public async Task<AuthorProfileLookupResponseDto> Handle(
        GetAuthorProfileLookupQuery request,
        CancellationToken cancellationToken)
    {
        var profile = await _authorProfileRepository.GetByIdAsync(request.AuthorProfileId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.AuthorProfileNotFound);

        return new AuthorProfileLookupResponseDto
        {
            AuthorProfileId = profile.Id,
            UserId = profile.UserId
        };
    }
}
