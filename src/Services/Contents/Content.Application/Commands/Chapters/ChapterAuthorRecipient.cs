namespace Content.Application.Commands.Chapters;

/// <summary>
/// Decides whether a chapter-review notification has a real author recipient.
/// <para>
/// A story carries either <see cref="Story.AuthorProfileId"/> (a real author,
/// whose AuthorProfile-&gt;User mapping lives in the Authentication service and
/// is resolved via <see cref="Content.Application.Interfaces.Services.IAuthorDirectoryClient"/>)
/// or, for a guest-published story, a free-text <see cref="Story.GuestAuthorName"/>
/// and an <see cref="Story.AuthorProfileId"/> of 0. Guest authors get no
/// notification.
/// </para>
/// </summary>
internal static class ChapterAuthorRecipient
{
    /// <summary>
    /// True when the story has a real (non-guest) author; <paramref name="authorProfileId"/>
    /// is then the value to resolve to a user id.
    /// </summary>
    public static bool TryResolveAuthorProfileId(Story story, out long authorProfileId)
    {
        authorProfileId = story?.AuthorProfileId ?? 0;
        return story is not null && story.GuestAuthorName is null && authorProfileId > 0;
    }
}
