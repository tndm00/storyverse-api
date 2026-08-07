namespace Authentication.Application.Queries.CurrentUser;

/// <summary>
/// Resolves the caller's own profile from the trusted auth context. Carries
/// no identifier from the client, per auth-guidelines.md section 3 — the
/// handler resolves the user id from <see cref="Interfaces.Services.ICurrentUserService"/>.
/// </summary>
public sealed class GetCurrentUserQuery : IQuery<CurrentUserResponseDto>
{
}
