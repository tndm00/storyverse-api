namespace Community.Application.Queries.Comments.GetChapterComments;

public sealed class GetChapterCommentsQueryHandler
    : IQueryHandler<GetChapterCommentsQuery, PagedResponseDto<CommentResponseDto>>
{
    private readonly ICommentRepository _commentRepository;

    public GetChapterCommentsQueryHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<PagedResponseDto<CommentResponseDto>> Handle(
        GetChapterCommentsQuery request,
        CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize) = PagingParameters.Normalize(request.PageNumber, request.PageSize);

        var (items, totalCount) = await _commentRepository.GetVisibleByChapterAsync(
            request.ChapterId, pageNumber, pageSize, cancellationToken);

        var dtos = items.Select(CommunityDtoMapper.ToDto).ToArray();

        return PagedResponseDto<CommentResponseDto>.Create(dtos, pageNumber, pageSize, totalCount);
    }
}
