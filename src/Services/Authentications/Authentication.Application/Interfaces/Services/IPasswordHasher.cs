namespace Authentication.Application.Interfaces.Services;

/// <summary>
/// Password hashing abstraction. Implemented in Infrastructure using a
/// vetted algorithm (BCrypt), so the Application layer never depends on the
/// concrete hashing library.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}
