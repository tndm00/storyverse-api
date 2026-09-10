namespace Community.Application.Queries.Comments.GetCommentExcerpts;

/// <summary>Internal batch lookup: comment public id -&gt; content excerpt. X-Service-Token protected.</summary>
public sealed class GetCommentExcerptsQuery : IQuery<IReadOnlyList<CommentExcerptEntryDto>>
{
    public IReadOnlyCollection<Guid> Ids { get; init; } = Array.Empty<Guid>();
}
