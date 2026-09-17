namespace Authentication.Application.Commands.AuthorProfilesAdmin.UpdateAuthorProfileAdmin;

/// <summary>Handles <see cref="UpdateAuthorProfileAdminCommand"/>: overwrites an existing author profile's editable fields.</summary>
public sealed class UpdateAuthorProfileAdminCommandHandler
    : ICommandHandler<UpdateAuthorProfileAdminCommand, AdminAuthorProfileResponseDto>
{
    private readonly IAuthorProfileRepository _authorProfileRepository;
    private readonly IUserRepository _userRepository;

    /// <summary>Initializes the handler with the repositories it depends on.</summary>
    public UpdateAuthorProfileAdminCommandHandler(
        IAuthorProfileRepository authorProfileRepository,
        IUserRepository userRepository)
    {
        _authorProfileRepository = authorProfileRepository;
        _userRepository = userRepository;
    }

    /// <summary>Applies the requested field updates to the profile and returns the updated admin view.</summary>
    public async Task<AdminAuthorProfileResponseDto> Handle(
        UpdateAuthorProfileAdminCommand request,
        CancellationToken cancellationToken)
    {
        // Ensure the target profile exists.
        var profile = await _authorProfileRepository.GetByIdAsync(request.AuthorProfileId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.AuthorProfileNotFound);

        // Overwrite editable fields and persist.
        profile.PenName = request.PenName.Trim();
        profile.Bio = request.Bio;
        profile.AvatarUrl = request.AvatarUrl;
        profile.BannerUrl = request.BannerUrl;
        profile.Verified = request.Verified;

        await _authorProfileRepository.SaveChangesAsync(cancellationToken);

        // Load the owning account for the response projection.
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
