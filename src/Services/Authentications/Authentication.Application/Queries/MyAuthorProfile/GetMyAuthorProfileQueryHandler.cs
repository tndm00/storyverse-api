namespace Authentication.Application.Queries.MyAuthorProfile;

/// <summary>Handles <see cref="GetMyAuthorProfileQuery"/>.</summary>
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

    /// <summary>Resolves the caller's own author profile, or throws if the caller has none.</summary>
    public async Task<AuthorProfileResponseDto> Handle(
        GetMyAuthorProfileQuery request,
        CancellationToken cancellationToken)
    {
        // Identity comes from the trusted auth context, never from the request.
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
