namespace Content.Application.Policies;

/// <summary>
/// Shared guards for drag-and-drop reordering: the client must send exactly the
/// ids it is allowed to reorder — no missing, extra, or duplicate ids — so the
/// resulting order is unambiguous.
/// </summary>
public static class ReorderPolicy
{
    public static void EnsureExactMatch(IReadOnlyList<Guid> requested, IReadOnlyCollection<Guid> actual)
    {
        var requestedSet = new HashSet<Guid>(requested);

        if (requestedSet.Count != requested.Count)
        {
            throw new BusinessRuleException(ApplicationErrorConstants.ReorderDuplicateIds);
        }

        if (!requestedSet.SetEquals(actual))
        {
            throw new BusinessRuleException(ApplicationErrorConstants.ReorderSetMismatch);
        }
    }

    /// <summary>
    /// Resolves the story that owns the mutation, allowing staff with
    /// <c>content.moderate</c> to reorder any author's story.
    /// </summary>
    public static Story EnsureStoryMutable(Story story, ICurrentAuthorContext authorContext, ILogger logger)
    {
        if (story is null)
        {
            throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);
        }

        if (authorContext.HasPermission(
            Be.StoryVerse.Shared.Authorization.StoryVersePermissions.Content.Moderate))
        {
            return story;
        }

        return StoryOwnership.EnsureOwned(story, authorContext.GetAuthorProfileId(), logger);
    }
}
