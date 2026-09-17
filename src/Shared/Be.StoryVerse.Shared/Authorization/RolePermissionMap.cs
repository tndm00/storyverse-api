namespace Be.StoryVerse.Shared.Authorization;

/// <summary>
/// Which permissions each role grants. The JWT carries only role names (small
/// and stable, auth-guidelines.md section 8/10); every service expands those
/// roles to concrete permissions locally through this map, so the token never
/// has to list permissions.
/// </summary>
public static class RolePermissionMap
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Map =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.Ordinal)
        {
            [StoryVerseRoles.Reader] = Array.Empty<string>(),
            [StoryVerseRoles.Author] = Array.Empty<string>(),
            [StoryVerseRoles.Moderator] = new[]
            {
                StoryVersePermissions.Reports.Review,
                StoryVersePermissions.Reports.Resolve,
                StoryVersePermissions.Content.Moderate,
                StoryVersePermissions.Community.Moderate,
            },
            [StoryVerseRoles.PlatformAdmin] = new List<string>(StoryVersePermissions.All),
        };

    /// <summary>Distinct permissions granted by the given role names.</summary>
    public static IReadOnlySet<string> PermissionsFor(IEnumerable<string> roles)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);

        foreach (var role in roles)
        {
            if (Map.TryGetValue(role, out var permissions))
            {
                foreach (var permission in permissions)
                {
                    result.Add(permission);
                }
            }
        }

        return result;
    }
}
