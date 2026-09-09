namespace Content.Application.Queries.Tags.GetPopularTags;

public sealed class GetPopularTagsQueryHandler : IQueryHandler<GetPopularTagsQuery, IReadOnlyList<TagResponseDto>>
{
    private readonly ITagRepository _tagRepository;

    public GetPopularTagsQueryHandler(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<IReadOnlyList<TagResponseDto>> Handle(
        GetPopularTagsQuery request,
        CancellationToken cancellationToken)
    {
        var count = Math.Clamp(
            request.Count <= 0 ? ApplicationConstants.PopularTagsCount : request.Count,
            1,
            ApplicationConstants.MaxPageSize);

        var tags = await _tagRepository.GetPopularAsync(count, cancellationToken);
        return tags.Select(ContentDtoMapper.ToDto).ToArray();
    }
}
