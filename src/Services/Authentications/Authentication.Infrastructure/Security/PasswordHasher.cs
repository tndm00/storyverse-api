namespace Authentication.Infrastructure.Security;

/// <summary>
/// BCrypt-based password hashing. BCrypt was chosen because it is a widely
/// vetted, self-salting adaptive hash well suited to a login credential store;
/// no specific algorithm was mandated by the rule files, so this is a judgment
/// call made during scaffolding.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public bool Verify(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
