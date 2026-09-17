namespace Content.Application.Policies;

/// <summary>
/// Shared ownership guard: content is owned by an <c>AuthorProfile</c>, never a
/// user (rules section 10.1). Authors may only mutate their own stories.
/// </summary>
public static class StoryOwnership
{
    /// <summary>Returns the story unchanged if it exists and belongs to <paramref name="authorProfileId"/>; otherwise throws not-found or forbidden.</summary>
    public static Story EnsureOwned(Story story, long authorProfileId, ILogger logger)
    {
        // No such story at all.
        if (story is null)
        {
            throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);
        }

        // Story exists but belongs to a different author profile - log and deny.
        if (story.AuthorProfileId != authorProfileId)
        {
            logger.LogWarning(ApplicationLogConstants.OwnershipCheckFailed, authorProfileId, story.Id);
            throw new ForbiddenException(ApplicationErrorConstants.NotStoryOwner);
        }

        return story;
    }
}
