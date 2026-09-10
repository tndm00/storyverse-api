using Be.StoryVerse.Core.Exceptions;
using Community.Application.Commands.Comments.SetCommentVisibility;
using Community.Application.Interfaces.Repositories;
using Community.Application.Interfaces.Services;
using Community.Domain.Entities;
using Community.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Community.UnitTests.Commands.Comments;

public class SetCommentVisibilityCommandHandlerTests
{
    private readonly ICommentRepository _commentRepository = Substitute.For<ICommentRepository>();
    private readonly ICurrentUserContext _userContext = Substitute.For<ICurrentUserContext>();
    private readonly ILogger<SetCommentVisibilityCommandHandler> _logger =
        Substitute.For<ILogger<SetCommentVisibilityCommandHandler>>();

    private readonly SetCommentVisibilityCommandHandler _handler;

    public SetCommentVisibilityCommandHandlerTests()
    {
        _handler = new SetCommentVisibilityCommandHandler(_commentRepository, _userContext, _logger);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_CommentDoesNotExist()
    {
        var commentId = Guid.NewGuid();
        _commentRepository.GetByPublicIdAsync(commentId, Arg.Any<CancellationToken>()).Returns((Comment)null);

        var command = new SetCommentVisibilityCommand { CommentId = commentId, Hide = true };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_HideComment_When_HideIsTrueAndCommentIsVisible()
    {
        var commentId = Guid.NewGuid();
        var comment = new Comment { PublicId = commentId, Status = CommentStatus.Visible };
        _commentRepository.GetByPublicIdAsync(commentId, Arg.Any<CancellationToken>()).Returns(comment);
        _userContext.GetUserId().Returns(1L);

        var command = new SetCommentVisibilityCommand { CommentId = commentId, Hide = true };

        var result = await _handler.Handle(command, CancellationToken.None);

        comment.Status.Should().Be(CommentStatus.Hidden);
        _commentRepository.Received(1).Update(comment);
        result.Status.Should().Be(CommentStatus.Hidden.ToString());
    }

    [Fact]
    public async Task Handle_Should_UnhideComment_When_HideIsFalseAndCommentIsHidden()
    {
        var commentId = Guid.NewGuid();
        var comment = new Comment { PublicId = commentId, Status = CommentStatus.Hidden };
        _commentRepository.GetByPublicIdAsync(commentId, Arg.Any<CancellationToken>()).Returns(comment);
        _userContext.GetUserId().Returns(1L);

        var command = new SetCommentVisibilityCommand { CommentId = commentId, Hide = false };

        var result = await _handler.Handle(command, CancellationToken.None);

        comment.Status.Should().Be(CommentStatus.Visible);
        _commentRepository.Received(1).Update(comment);
        result.Status.Should().Be(CommentStatus.Visible.ToString());
    }

    [Fact]
    public async Task Handle_Should_NotResurfaceComment_When_UnhideCalledOnDeletedComment()
    {
        var commentId = Guid.NewGuid();
        var comment = new Comment { PublicId = commentId, Status = CommentStatus.Deleted };
        _commentRepository.GetByPublicIdAsync(commentId, Arg.Any<CancellationToken>()).Returns(comment);
        _userContext.GetUserId().Returns(1L);

        var command = new SetCommentVisibilityCommand { CommentId = commentId, Hide = false };

        var result = await _handler.Handle(command, CancellationToken.None);

        comment.Status.Should().Be(CommentStatus.Deleted);
        _commentRepository.DidNotReceive().Update(Arg.Any<Comment>());
        await _commentRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        result.Status.Should().Be(CommentStatus.Deleted.ToString());
    }
}
