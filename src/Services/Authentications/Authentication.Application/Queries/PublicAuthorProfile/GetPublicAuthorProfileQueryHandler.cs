namespace Authentication.Application.Queries.PublicAuthorProfile;

/// <summary>Handles <see cref="GetPublicAuthorProfileQuery"/>.</summary>
public sealed class GetPublicAuthorProfileQueryHandler
    : IQueryHandler<GetPublicAuthorProfileQuery, PublicAuthorProfileResponseDto>
{
    private readonly IAuthorProfileRepository _authorProfileRepository;

    public GetPublicAuthorProfileQueryHandler(IAuthorProfileRepository authorProfileRepository)
    {
        _authorProfileRepository = authorProfileRepository;
    }

    /// <summary>Resolves the public view of an author profile by id, or throws if unknown.</summary>
    public async Task<PublicAuthorProfileResponseDto> Handle(
        GetPublicAuthorProfileQuery request,
        CancellationToken cancellationToken)
    {
        // Anonymous lookup by id; unknown id is a 404.
        var profile = await _authorProfileRepository.GetByIdAsync(request.AuthorProfileId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.AuthorProfileNotFound);

        return new PublicAuthorProfileResponseDto
        {
            AuthorProfileId = profile.Id,
            PenName = profile.PenName,
            Bio = profile.Bio,
            AvatarUrl = profile.AvatarUrl,
            BannerUrl = profile.BannerUrl,
            Verified = profile.Verified
        };
    }
}
