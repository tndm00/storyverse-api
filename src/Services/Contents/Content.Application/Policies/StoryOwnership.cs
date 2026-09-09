namespace Content.Application.Policies;

/// <summary>
/// Shared ownership guard: content is owned by an <c>AuthorProfile</c>, never a
/// user (rules section 10.1). Authors may only mutate their own stories.
/// </summary>
public static class StoryOwnership
{
    public static Story EnsureOwned(Story story, long authorProfileId, ILogger logger)
    {
        if (story is null)
        {
            throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);
        }

        if (story.AuthorProfileId != authorProfileId)
        {
            logger.LogWarning(ApplicationLogConstants.OwnershipCheckFailed, authorProfileId, story.Id);
            throw new ForbiddenException(ApplicationErrorConstants.NotStoryOwner);
        }

        return story;
    }
}
