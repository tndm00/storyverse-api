namespace Content.Application.Queries.Tags.GetPopularTags;

public sealed class GetPopularTagsQueryHandler : IQueryHandler<GetPopularTagsQuery, IReadOnlyList<TagResponseDto>>
{
    private readonly ITagRepository _tagRepository;

    public GetPopularTagsQueryHandler(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    /// <summary>Returns the most-used tags, clamped to a safe count, for tag-cloud/suggestion UIs.</summary>
    public async Task<IReadOnlyList<TagResponseDto>> Handle(
        GetPopularTagsQuery request,
        CancellationToken cancellationToken)
    {
        // Normalize the requested count to a safe bound.
        var count = Math.Clamp(
            request.Count <= 0 ? ApplicationConstants.PopularTagsCount : request.Count,
            1,
            ApplicationConstants.MaxPageSize);

        var tags = await _tagRepository.GetPopularAsync(count, cancellationToken);
        return tags.Select(ContentDtoMapper.ToDto).ToArray();
    }
}
