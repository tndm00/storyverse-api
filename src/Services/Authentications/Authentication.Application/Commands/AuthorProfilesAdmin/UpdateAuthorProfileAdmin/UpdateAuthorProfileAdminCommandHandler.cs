namespace Authentication.Application.Commands.AuthorProfilesAdmin.UpdateAuthorProfileAdmin;

public sealed class UpdateAuthorProfileAdminCommandHandler
    : ICommandHandler<UpdateAuthorProfileAdminCommand, AdminAuthorProfileResponseDto>
{
    private readonly IAuthorProfileRepository _authorProfileRepository;
    private readonly IUserRepository _userRepository;

    public UpdateAuthorProfileAdminCommandHandler(
        IAuthorProfileRepository authorProfileRepository,
        IUserRepository userRepository)
    {
        _authorProfileRepository = authorProfileRepository;
        _userRepository = userRepository;
    }

    public async Task<AdminAuthorProfileResponseDto> Handle(
        UpdateAuthorProfileAdminCommand request,
        CancellationToken cancellationToken)
    {
        var profile = await _authorProfileRepository.GetByIdAsync(request.AuthorProfileId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.AuthorProfileNotFound);

        profile.PenName = request.PenName.Trim();
        profile.Bio = request.Bio;
        profile.AvatarUrl = request.AvatarUrl;
        profile.BannerUrl = request.BannerUrl;
        profile.Verified = request.Verified;

        await _authorProfileRepository.SaveChangesAsync(cancellationToken);

        var user = await _userRepository.GetByIdAsync(profile.UserId, cancellationToken);

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
    }
}
