using Content.Application.Commands.Marketing.PostFacebookDigest;
using Content.Application.Dtos;
using Content.Application.Interfaces.Repositories;
using Content.Application.Interfaces.Services;
using Content.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Marketing;

public class PostFacebookDigestCommandHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly IFacebookPageClient _facebookPageClient = Substitute.For<IFacebookPageClient>();
    private readonly ILogger<PostFacebookDigestCommandHandler> _logger =
        Substitute.For<ILogger<PostFacebookDigestCommandHandler>>();

    private readonly PostFacebookDigestCommandHandler _handler;

    public PostFacebookDigestCommandHandlerTests()
    {
        _handler = new PostFacebookDigestCommandHandler(_storyRepository, _facebookPageClient, _logger);
    }

    [Fact]
    public async Task Handle_Should_QueryByViewCountDescending_WithRequestedTopCount()
    {
        _storyRepository.SearchPublishedAsync(Arg.Any<StorySearchCriteria>(), Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Story>(), 0));

        await _handler.Handle(new PostFacebookDigestCommand { TopCount = 7 }, CancellationToken.None);

        await _storyRepository.Received(1).SearchPublishedAsync(
            Arg.Is<StorySearchCriteria>(c =>
                c.SortBy == StorySortField.ViewCount && c.Descending && c.PageSize == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_PostMessageListingEachStoryWithItsLink()
    {
        var stories = new[]
        {
            new Story { Id = 1, Title = "Nhà hoang", Slug = "nha-hoang" },
            new Story { Id = 2, Title = "Bệnh viện cũ", Slug = "benh-vien-cu" }
        };
        _storyRepository.SearchPublishedAsync(Arg.Any<StorySearchCriteria>(), Arg.Any<CancellationToken>())
            .Returns((stories, 2));

        var result = await _handler.Handle(new PostFacebookDigestCommand { TopCount = 5 }, CancellationToken.None);

        result.StoryCount.Should().Be(2);
        await _facebookPageClient.Received(1).PostAsync(
            Arg.Is<string>(m =>
                m.Contains("Nhà hoang") && m.Contains("nha-hoang") &&
                m.Contains("Bệnh viện cũ") && m.Contains("benh-vien-cu")),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotPost_When_NoPublishedStoriesExist()
    {
        _storyRepository.SearchPublishedAsync(Arg.Any<StorySearchCriteria>(), Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Story>(), 0));

        var result = await _handler.Handle(new PostFacebookDigestCommand(), CancellationToken.None);

        result.StoryCount.Should().Be(0);
        await _facebookPageClient.DidNotReceive()
            .PostAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotThrow_When_FacebookPostFails()
    {
        var stories = new[] { new Story { Id = 1, Title = "Nhà hoang", Slug = "nha-hoang" } };
        _storyRepository.SearchPublishedAsync(Arg.Any<StorySearchCriteria>(), Arg.Any<CancellationToken>())
            .Returns((stories, 1));
        _facebookPageClient
            .PostAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("facebook down"));

        var act = () => _handler.Handle(new PostFacebookDigestCommand(), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
