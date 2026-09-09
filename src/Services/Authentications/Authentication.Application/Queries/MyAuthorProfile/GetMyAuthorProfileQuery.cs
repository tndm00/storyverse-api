namespace Authentication.Application.Queries.MyAuthorProfile;

/// <summary>
/// Returns the caller's own <see cref="AuthorProfile"/>. Carries no identifier
/// from the client, per auth-guidelines.md section 3 — the handler resolves the
/// user id from <see cref="Interfaces.Services.ICurrentUserService"/>.
/// </summary>
public sealed class GetMyAuthorProfileQuery : IQuery<AuthorProfileResponseDto>
{
}
