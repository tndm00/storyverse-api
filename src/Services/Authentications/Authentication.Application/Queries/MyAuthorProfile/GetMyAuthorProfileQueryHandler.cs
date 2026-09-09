namespace Authentication.Application.Queries.MyAuthorProfile;

public sealed class GetMyAuthorProfileQueryHandler
    : IQueryHandler<GetMyAuthorProfileQuery, AuthorProfileResponseDto>
{
    private readonly IAuthorProfileRepository _authorProfileRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetMyAuthorProfileQueryHandler(
        IAuthorProfileRepository authorProfileRepository,
        ICurrentUserService currentUserService)
    {
        _authorProfileRepository = authorProfileRepository;
        _currentUserService = currentUserService;
    }

    public async Task<AuthorProfileResponseDto> Handle(
        GetMyAuthorProfileQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var authorProfile = await _authorProfileRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.AuthorProfileNotFound);

        return new AuthorProfileResponseDto
        {
            AuthorProfileId = authorProfile.Id,
            UserId = authorProfile.UserId,
            PenName = authorProfile.PenName,
            Bio = authorProfile.Bio,
            AvatarUrl = authorProfile.AvatarUrl,
            BannerUrl = authorProfile.BannerUrl,
            Verified = authorProfile.Verified,
            Status = authorProfile.Status.ToString(),
            CreatedAt = authorProfile.CreatedAt,
            RequiresTokenRefresh = false
        };
    }
}
