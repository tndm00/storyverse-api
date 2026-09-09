namespace Authentication.Application.Queries.PublicAuthorProfile;

public sealed class GetPublicAuthorProfileQueryHandler
    : IQueryHandler<GetPublicAuthorProfileQuery, PublicAuthorProfileResponseDto>
{
    private readonly IAuthorProfileRepository _authorProfileRepository;

    public GetPublicAuthorProfileQueryHandler(IAuthorProfileRepository authorProfileRepository)
    {
        _authorProfileRepository = authorProfileRepository;
    }

    public async Task<PublicAuthorProfileResponseDto> Handle(
        GetPublicAuthorProfileQuery request,
        CancellationToken cancellationToken)
    {
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
