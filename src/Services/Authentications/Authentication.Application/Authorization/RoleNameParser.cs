namespace Authentication.Application.Authorization;

/// <summary>
/// Parses a client-supplied role name to the <see cref="Role"/> enum. Accepts
/// any casing but only the four defined platform role names; the canonical
/// enum-name casing is what gets persisted and later emitted in the JWT.
/// </summary>
public static class RoleNameParser
{
    public static bool TryParse(string value, out Role role)
    {
        role = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return Enum.TryParse(value.Trim(), ignoreCase: true, out role) && Enum.IsDefined(role);
    }
}
