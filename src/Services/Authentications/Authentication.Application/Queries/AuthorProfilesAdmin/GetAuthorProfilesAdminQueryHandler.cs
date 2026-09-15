namespace Authentication.Application.Queries.AuthorProfilesAdmin;

public sealed class GetAuthorProfilesAdminQueryHandler
    : IQueryHandler<GetAuthorProfilesAdminQuery, PagedResponseDto<AdminAuthorProfileResponseDto>>
{
    private readonly IAuthorProfileRepository _authorProfileRepository;
    private readonly IUserRepository _userRepository;

    public GetAuthorProfilesAdminQueryHandler(
        IAuthorProfileRepository authorProfileRepository,
        IUserRepository userRepository)
    {
        _authorProfileRepository = authorProfileRepository;
        _userRepository = userRepository;
    }

    public async Task<PagedResponseDto<AdminAuthorProfileResponseDto>> Handle(
        GetAuthorProfilesAdminQuery request,
        CancellationToken cancellationToken)
    {
        var (profiles, totalCount) = await _authorProfileRepository.GetPagedAsync(
            request.Keyword, request.PageNumber, request.PageSize, cancellationToken);

        var users = await _userRepository.GetByIdsAsync(
            profiles.Select(x => x.UserId), cancellationToken);
        var usersById = users.ToDictionary(u => u.Id);

        var items = profiles
            .Select(profile =>
            {
                usersById.TryGetValue(profile.UserId, out var user);

                return new AdminAuthorProfileResponseDto
                {
                    AuthorProfileId = profile.Id,
                    UserId = profile.UserId,
                    Email = user?.Email ?? string.Empty,
                    DisplayName = user?.DisplayName ?? string.Empty,
                    PenName = profile.PenName,
                    Bio = profile.Bio,
                    AvatarUrl = profile.AvatarUrl,
                    BannerUrl = profile.BannerUrl,
                    Verified = profile.Verified,
                    Status = profile.Status.ToString(),
                    CreatedAt = profile.CreatedAt
                };
            })
            .ToArray();

        return PagedResponseDto<AdminAuthorProfileResponseDto>.Create(
            items, request.PageNumber, request.PageSize, totalCount);
    }
}
