using Be.StoryVerse.Shared.Authorization;
using FluentAssertions;
using Xunit;

namespace Authentication.UnitTests.Authorization;

public class RolePermissionMapTests
{
    [Fact]
    public void PermissionsFor_Should_ReturnEmptySet_When_RoleIsReader()
    {
        var result = RolePermissionMap.PermissionsFor(new[] { StoryVerseRoles.Reader });

        result.Should().BeEmpty();
    }

    [Fact]
    public void PermissionsFor_Should_ReturnEmptySet_When_RoleIsAuthor()
    {
        var result = RolePermissionMap.PermissionsFor(new[] { StoryVerseRoles.Author });

        result.Should().BeEmpty();
    }

    [Fact]
    public void PermissionsFor_Should_ReturnModerationPermissions_When_RoleIsModerator()
    {
        var result = RolePermissionMap.PermissionsFor(new[] { StoryVerseRoles.Moderator });

        result.Should().BeEquivalentTo(new[]
        {
            StoryVersePermissions.Reports.Review,
            StoryVersePermissions.Reports.Resolve,
            StoryVersePermissions.Content.Moderate,
            StoryVersePermissions.Community.Moderate
        });
    }

    [Fact]
    public void PermissionsFor_Should_NotGrantAnalytics_When_RoleIsModerator()
    {
        var result = RolePermissionMap.PermissionsFor(new[] { StoryVerseRoles.Moderator });

        result.Should().NotContain(StoryVersePermissions.Analytics.View);
    }

    [Fact]
    public void PermissionsFor_Should_GrantAnalytics_When_RoleIsPlatformAdmin()
    {
        var result = RolePermissionMap.PermissionsFor(new[] { StoryVerseRoles.PlatformAdmin });

        result.Should().Contain(StoryVersePermissions.Analytics.View);
    }

    [Fact]
    public void PermissionsFor_Should_ReturnAllPermissions_When_RoleIsPlatformAdmin()
    {
        var result = RolePermissionMap.PermissionsFor(new[] { StoryVerseRoles.PlatformAdmin });

        result.Should().BeEquivalentTo(StoryVersePermissions.All);
    }

    [Fact]
    public void PermissionsFor_Should_ReturnUnion_When_CallerHasMultipleRoles()
    {
        var result = RolePermissionMap.PermissionsFor(new[] { StoryVerseRoles.Reader, StoryVerseRoles.Moderator });

        result.Should().BeEquivalentTo(new[]
        {
            StoryVersePermissions.Reports.Review,
            StoryVersePermissions.Reports.Resolve,
            StoryVersePermissions.Content.Moderate,
            StoryVersePermissions.Community.Moderate
        });
    }

    [Fact]
    public void PermissionsFor_Should_ContributeNoPermissions_When_RoleIsUnrecognized()
    {
        var result = RolePermissionMap.PermissionsFor(new[] { "SomeUnknownRole" });

        result.Should().BeEmpty();
    }
}
