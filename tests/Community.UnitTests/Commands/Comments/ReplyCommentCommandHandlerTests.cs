using Be.StoryVerse.Core.Exceptions;
using Community.Application.Commands.Comments.ReplyComment;
using Community.Application.Interfaces.Repositories;
using Community.Application.Interfaces.Services;
using Community.Domain.Entities;
using Community.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Community.UnitTests.Commands.Comments;

public class ReplyCommentCommandHandlerTests
{
    private readonly ICommentRepository _commentRepository = Substitute.For<ICommentRepository>();
    private readonly ICurrentUserContext _userContext = Substitute.For<ICurrentUserContext>();
    private readonly ILogger<ReplyCommentCommandHandler> _logger =
        Substitute.For<ILogger<ReplyCommentCommandHandler>>();

    private readonly ReplyCommentCommandHandler _handler;

    public ReplyCommentCommandHandlerTests()
    {
        _handler = new ReplyCommentCommandHandler(_commentRepository, _userContext, _logger);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_ParentCommentDoesNotExist()
    {
        var parentId = Guid.NewGuid();
        _commentRepository.GetByPublicIdAsync(parentId, Arg.Any<CancellationToken>()).Returns((Comment)null);

        var command = new ReplyCommentCommand { ParentCommentId = parentId, Content = "reply" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Theory]
    [InlineData(CommentStatus.Hidden)]
    [InlineData(CommentStatus.Deleted)]
    public async Task Handle_Should_ThrowBusinessRuleException_When_ParentCommentIsNotVisible(CommentStatus status)
    {
        var parentId = Guid.NewGuid();
        var parent = new Comment { PublicId = parentId, Status = status, ParentCommentId = null };
        _commentRepository.GetByPublicIdAsync(parentId, Arg.Any<CancellationToken>()).Returns(parent);

        var command = new ReplyCommentCommand { ParentCommentId = parentId, Content = "reply" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowBusinessRuleException_When_ParentCommentIsItselfAReply()
    {
        var grandparentId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var parent = new Comment
        {
            PublicId = parentId,
            Status = CommentStatus.Visible,
            ParentCommentId = grandparentId
        };
        _commentRepository.GetByPublicIdAsync(parentId, Arg.Any<CancellationToken>()).Returns(parent);

        var command = new ReplyCommentCommand { ParentCommentId = parentId, Content = "reply" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Handle_Should_AddReplyUnderParent_When_ParentIsVisibleTopLevelComment()
    {
        const long userId = 9;
        var chapterId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var parent = new Comment
        {
            PublicId = parentId,
            ChapterId = chapterId,
            Status = CommentStatus.Visible,
            ParentCommentId = null
        };
        _commentRepository.GetByPublicIdAsync(parentId, Arg.Any<CancellationToken>()).Returns(parent);
        _userContext.GetUserId().Returns(userId);

        var command = new ReplyCommentCommand { ParentCommentId = parentId, Content = "reply" };

        var result = await _handler.Handle(command, CancellationToken.None);

        await _commentRepository.Received(1).AddAsync(
            Arg.Is<Comment>(c =>
                c.ChapterId == chapterId &&
                c.ParentCommentId == parentId &&
                c.AuthorUserId == userId &&
                c.Status == CommentStatus.Visible),
            Arg.Any<CancellationToken>());
        result.ParentCommentId.Should().Be(parentId);
    }
}
