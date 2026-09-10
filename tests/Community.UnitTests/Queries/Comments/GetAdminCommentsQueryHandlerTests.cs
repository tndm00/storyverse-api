using Be.StoryVerse.Core.Exceptions;
using Community.Application.Interfaces.Repositories;
using Community.Application.Interfaces.Services;
using Community.Application.Queries.Comments.GetAdminComments;
using Community.Domain.Entities;
using Community.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Community.UnitTests.Queries.Comments;

public class GetAdminCommentsQueryHandlerTests
{
    private readonly ICommentRepository _commentRepository = Substitute.For<ICommentRepository>();
    private readonly IUserDirectoryClient _userDirectory = Substitute.For<IUserDirectoryClient>();

    private readonly GetAdminCommentsQueryHandler _handler;

    public GetAdminCommentsQueryHandlerTests()
    {
        _userDirectory.GetDisplayNamesAsync(Arg.Any<IEnumerable<long>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<long, string> { [7] = "Neo" });

        _handler = new GetAdminCommentsQueryHandler(_commentRepository, _userDirectory);
    }

    private void SetupRepo(params Comment[] comments)
    {
        _commentRepository.SearchAsync(
                Arg.Any<Guid?>(), Arg.Any<long?>(), Arg.Any<CommentStatus?>(), Arg.Any<string>(),
                Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((comments, comments.Length));
    }

    [Fact]
    public async Task Handle_Should_MapCommentsAcrossChapters_And_FillDisplayName()
    {
        var chapterA = Guid.NewGuid();
        var chapterB = Guid.NewGuid();
        SetupRepo(
            new Comment { PublicId = Guid.NewGuid(), ChapterId = chapterA, AuthorUserId = 7, Content = "a", Status = CommentStatus.Visible },
            new Comment { PublicId = Guid.NewGuid(), ChapterId = chapterB, AuthorUserId = 9, Content = "b", Status = CommentStatus.Hidden });

        var result = await _handler.Handle(new GetAdminCommentsQuery { Status = "all" }, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items.Select(x => x.ChapterId).Should().BeEquivalentTo(new[] { chapterA, chapterB });
        result.Items.First(x => x.AuthorUserId == 7).AuthorDisplayName.Should().Be("Neo");
        result.Items.First(x => x.AuthorUserId == 9).AuthorDisplayName.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_PassParsedFilters_To_Repository()
    {
        SetupRepo();
        var chapterId = Guid.NewGuid();

        await _handler.Handle(
            new GetAdminCommentsQuery
            {
                Status = "hidden",
                Keyword = "spam",
                ChapterId = chapterId,
                AuthorUserId = 42,
                SortDirection = "asc"
            },
            CancellationToken.None);

        await _commentRepository.Received(1).SearchAsync(
            chapterId, 42L, CommentStatus.Hidden, "spam", true,
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_TreatAllAndEmpty_As_NoStatusFilter()
    {
        SetupRepo();

        await _handler.Handle(new GetAdminCommentsQuery { Status = null }, CancellationToken.None);

        await _commentRepository.Received(1).SearchAsync(
            Arg.Any<Guid?>(), Arg.Any<long?>(), null, Arg.Any<string>(), false,
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Throw_When_StatusFilterInvalid()
    {
        SetupRepo();

        var act = () => _handler.Handle(new GetAdminCommentsQuery { Status = "bogus" }, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }
}
