using Community.Application.Commands.Comments.AddComment;
using Community.Application.Interfaces.Repositories;
using Community.Application.Interfaces.Services;
using Community.Domain.Entities;
using Community.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Community.UnitTests.Commands.Comments;

public class AddCommentCommandHandlerTests
{
    private readonly ICommentRepository _commentRepository = Substitute.For<ICommentRepository>();
    private readonly ICurrentUserContext _userContext = Substitute.For<ICurrentUserContext>();
    private readonly IContentCommentCountSyncClient _commentCountSyncClient = Substitute.For<IContentCommentCountSyncClient>();
    private readonly ILogger<AddCommentCommandHandler> _logger = Substitute.For<ILogger<AddCommentCommandHandler>>();

    private readonly AddCommentCommandHandler _handler;

    public AddCommentCommandHandlerTests()
    {
        _handler = new AddCommentCommandHandler(_commentRepository, _userContext, _commentCountSyncClient, _logger);
    }

    [Fact]
    public async Task Handle_Should_AddVisibleTopLevelComment_When_Called()
    {
        const long userId = 42;
        var chapterId = Guid.NewGuid();
        _userContext.GetUserId().Returns(userId);

        var command = new AddCommentCommand { ChapterId = chapterId, Content = "Great chapter!" };

        var result = await _handler.Handle(command, CancellationToken.None);

        await _commentRepository.Received(1).AddAsync(
            Arg.Is<Comment>(c =>
                c.ChapterId == chapterId &&
                c.AuthorUserId == userId &&
                c.ParentCommentId == null &&
                c.Status == CommentStatus.Visible),
            Arg.Any<CancellationToken>());
        await _commentRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        result.ChapterId.Should().Be(chapterId);
        result.AuthorUserId.Should().Be(userId);
    }

    [Fact]
    public async Task Handle_Should_TrimContent_When_ContentHasSurroundingWhitespace()
    {
        _userContext.GetUserId().Returns(1L);
        var command = new AddCommentCommand { ChapterId = Guid.NewGuid(), Content = "  hello world  " };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Content.Should().Be("hello world");
    }

    [Fact]
    public async Task Handle_Should_ReturnMappedDto_When_CommentAdded()
    {
        _userContext.GetUserId().Returns(7L);
        var command = new AddCommentCommand { ChapterId = Guid.NewGuid(), Content = "nice" };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(CommentStatus.Visible.ToString());
        result.Id.Should().NotBeEmpty();
    }
}
