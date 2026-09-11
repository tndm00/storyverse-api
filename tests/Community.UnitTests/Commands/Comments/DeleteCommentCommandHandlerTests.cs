using Be.StoryVerse.Core.Exceptions;
using Community.Application.Commands.Comments.DeleteComment;
using Community.Application.Interfaces.Repositories;
using Community.Application.Interfaces.Services;
using Community.Domain.Entities;
using Community.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Community.UnitTests.Commands.Comments;

public class DeleteCommentCommandHandlerTests
{
    private readonly ICommentRepository _commentRepository = Substitute.For<ICommentRepository>();
    private readonly ICurrentUserContext _userContext = Substitute.For<ICurrentUserContext>();
    private readonly IContentCommentCountSyncClient _commentCountSyncClient = Substitute.For<IContentCommentCountSyncClient>();
    private readonly ILogger<DeleteCommentCommandHandler> _logger =
        Substitute.For<ILogger<DeleteCommentCommandHandler>>();

    private readonly DeleteCommentCommandHandler _handler;

    public DeleteCommentCommandHandlerTests()
    {
        _handler = new DeleteCommentCommandHandler(_commentRepository, _userContext, _commentCountSyncClient, _logger);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_CommentDoesNotExist()
    {
        var commentId = Guid.NewGuid();
        _commentRepository.GetByPublicIdAsync(commentId, Arg.Any<CancellationToken>()).Returns((Comment)null);

        var command = new DeleteCommentCommand { CommentId = commentId };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowForbiddenException_When_CallerIsNotTheAuthor()
    {
        var commentId = Guid.NewGuid();
        var comment = new Comment { PublicId = commentId, AuthorUserId = 1, Status = CommentStatus.Visible };
        _commentRepository.GetByPublicIdAsync(commentId, Arg.Any<CancellationToken>()).Returns(comment);
        _userContext.GetUserId().Returns(2L);

        var command = new DeleteCommentCommand { CommentId = commentId };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_Should_SetStatusDeleted_When_AuthorDeletesVisibleComment()
    {
        const long userId = 4;
        var commentId = Guid.NewGuid();
        var comment = new Comment { PublicId = commentId, AuthorUserId = userId, Status = CommentStatus.Visible };
        _commentRepository.GetByPublicIdAsync(commentId, Arg.Any<CancellationToken>()).Returns(comment);
        _userContext.GetUserId().Returns(userId);

        var command = new DeleteCommentCommand { CommentId = commentId };

        var result = await _handler.Handle(command, CancellationToken.None);

        comment.Status.Should().Be(CommentStatus.Deleted);
        _commentRepository.Received(1).Update(comment);
        await _commentRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        result.Status.Should().Be(CommentStatus.Deleted.ToString());
    }

    [Fact]
    public async Task Handle_Should_BeIdempotent_When_CommentIsAlreadyDeleted()
    {
        const long userId = 6;
        var commentId = Guid.NewGuid();
        var comment = new Comment { PublicId = commentId, AuthorUserId = userId, Status = CommentStatus.Deleted };
        _commentRepository.GetByPublicIdAsync(commentId, Arg.Any<CancellationToken>()).Returns(comment);
        _userContext.GetUserId().Returns(userId);

        var command = new DeleteCommentCommand { CommentId = commentId };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().NotThrowAsync();
        comment.Status.Should().Be(CommentStatus.Deleted);
        _commentRepository.DidNotReceive().Update(Arg.Any<Comment>());
        await _commentRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
