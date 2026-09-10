namespace Authentication.Application.Commands.Refresh;

/// <summary>
/// Exchanges a valid refresh token for a fresh access + refresh token pair
/// (rotation). The new access token reflects the account's current roles and
/// <c>author_id</c>, read from the database — never copied from the old token.
/// </summary>
public sealed class RefreshCommand : ICommand<LoginResponseDto>
{
    public string RefreshToken { get; init; } = string.Empty;
}
