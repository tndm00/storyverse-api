namespace Content.Application.Queries.Stories.GetStoryById;

/// <summary>
/// Story page header by public id. Same visibility rules as
/// <see cref="GetStoryDetail.GetStoryDetailQuery"/>: the owner also sees a draft;
/// everyone else only sees a published story.
/// </summary>
public sealed class GetStoryByIdQuery : IQuery<StoryDetailResponseDto>
{
    public Guid StoryId { get; init; }
}
