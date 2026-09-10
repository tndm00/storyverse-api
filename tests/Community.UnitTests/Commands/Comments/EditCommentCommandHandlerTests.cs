using Be.StoryVerse.Core.Exceptions;
using Community.Application.Commands.Comments.EditComment;
using Community.Application.Interfaces.Repositories;
using Community.Application.Interfaces.Services;
using Community.Domain.Entities;
using Community.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Community.UnitTests.Commands.Comments;

public class EditCommentCommandHandlerTests
{
    private readonly ICommentRepository _commentRepository = Substitute.For<ICommentRepository>();
    private readonly ICurrentUserContext _userContext = Substitute.For<ICurrentUserContext>();
    private readonly ILogger<EditCommentCommandHandler> _logger =
        Substitute.For<ILogger<EditCommentCommandHandler>>();

    private readonly EditCommentCommandHandler _handler;

    public EditCommentCommandHandlerTests()
    {
        _handler = new EditCommentCommandHandler(_commentRepository, _userContext, _logger);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_CommentDoesNotExist()
    {
        var commentId = Guid.NewGuid();
        _commentRepository.GetByPublicIdAsync(commentId, Arg.Any<CancellationToken>()).Returns((Comment)null);

        var command = new EditCommentCommand { CommentId = commentId, Content = "updated" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowForbiddenException_When_CallerIsNotTheAuthor()
    {
        var commentId = Guid.NewGuid();
        var comment = new Comment
        {
            PublicId = commentId,
            AuthorUserId = 1,
            Status = CommentStatus.Visible
        };
        _commentRepository.GetByPublicIdAsync(commentId, Arg.Any<CancellationToken>()).Returns(comment);
        _userContext.GetUserId().Returns(2L);

        var command = new EditCommentCommand { CommentId = commentId, Content = "updated" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Theory]
    [InlineData(CommentStatus.Hidden)]
    [InlineData(CommentStatus.Deleted)]
    public async Task Handle_Should_ThrowBusinessRuleException_When_CommentIsNotVisible(CommentStatus status)
    {
        const long userId = 3;
        var commentId = Guid.NewGuid();
        var comment = new Comment
        {
            PublicId = commentId,
            AuthorUserId = userId,
            Status = status
        };
        _commentRepository.GetByPublicIdAsync(commentId, Arg.Any<CancellationToken>()).Returns(comment);
        _userContext.GetUserId().Returns(userId);

        var command = new EditCommentCommand { CommentId = commentId, Content = "updated" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Handle_Should_UpdateContentAndTimestamp_When_AuthorEditsVisibleComment()
    {
        const long userId = 5;
        var commentId = Guid.NewGuid();
        var comment = new Comment
        {
            PublicId = commentId,
            AuthorUserId = userId,
            Status = CommentStatus.Visible,
            Content = "old content"
        };
        _commentRepository.GetByPublicIdAsync(commentId, Arg.Any<CancellationToken>()).Returns(comment);
        _userContext.GetUserId().Returns(userId);

        var command = new EditCommentCommand { CommentId = commentId, Content = "  new content  " };

        var result = await _handler.Handle(command, CancellationToken.None);

        comment.Content.Should().Be("new content");
        comment.UpdatedAt.Should().NotBeNull();
        _commentRepository.Received(1).Update(comment);
        await _commentRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        result.Content.Should().Be("new content");
    }
}
