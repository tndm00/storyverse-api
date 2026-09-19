namespace Be.StoryVerse.Shared.Authorization;

/// <summary>
/// Business capabilities checked by policy-based authorization
/// (auth-guidelines.md section 4/6: prefer permission checks over role checks,
/// keep permission constants centralized). Names are stable and
/// business-readable, grouped by resource.
/// </summary>
public static class StoryVersePermissions
{
    public static class Genres
    {
        /// <summary>Create, update, or hide a platform genre.</summary>
        public const string Manage = "genres.manage";
    }

    public static class Tags
    {
        /// <summary>Merge or hide free-form tags.</summary>
        public const string Manage = "tags.manage";
    }

    public static class Reports
    {
        /// <summary>Pick up a report and move it into review.</summary>
        public const string Review = "reports.review";

        /// <summary>Record a moderation action that resolves a report.</summary>
        public const string Resolve = "reports.resolve";
    }

    public static class Content
    {
        /// <summary>Hide or remove another author's published content.</summary>
        public const string Moderate = "content.moderate";
    }

    public static class Community
    {
        /// <summary>Hide or unhide reader-generated community content (comments).</summary>
        public const string Moderate = "community.moderate";
    }

    public static class Users
    {
        /// <summary>Manage accounts and role assignments.</summary>
        public const string Manage = "users.manage";
    }

    public static class Analytics
    {
        /// <summary>See platform-wide traffic statistics (total/daily views, top stories). Granted to PlatformAdmin only.</summary>
        public const string View = "analytics.view";
    }

    /// <summary>Every permission constant — used to register one policy per permission.</summary>
    public static IReadOnlyCollection<string> All { get; } = new[]
    {
        Genres.Manage,
        Tags.Manage,
        Reports.Review,
        Reports.Resolve,
        Content.Moderate,
        Community.Moderate,
        Users.Manage,
        Analytics.View,
    };
}
