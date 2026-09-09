using Be.StoryVerse.Core.Exceptions;
using Content.Application.Policies;
using Content.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Policies;

public class StoryOwnershipTests
{
    private readonly ILogger _logger = Substitute.For<ILogger>();

    [Fact]
    public void EnsureOwned_Should_ThrowNotFoundException_When_StoryIsNull()
    {
        Action act = () => StoryOwnership.EnsureOwned(null, authorProfileId: 1, _logger);

        act.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void EnsureOwned_Should_ThrowForbiddenException_When_AuthorProfileIdMismatch()
    {
        var story = new Story { Id = 1, AuthorProfileId = 10 };

        Action act = () => StoryOwnership.EnsureOwned(story, authorProfileId: 99, _logger);

        act.Should().Throw<ForbiddenException>();
    }

    [Fact]
    public void EnsureOwned_Should_ReturnStory_When_AuthorProfileIdMatches()
    {
        var story = new Story { Id = 1, AuthorProfileId = 10 };

        var result = StoryOwnership.EnsureOwned(story, authorProfileId: 10, _logger);

        result.Should().BeSameAs(story);
    }
}
