using System.Collections.Generic;
using Community.Application.Interfaces.Repositories;
using Community.Application.Interfaces.Services;
using Community.Application.Queries.Ratings.GetStoryRatings;
using Community.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Community.UnitTests.Queries.Ratings;

public class GetStoryRatingsQueryHandlerTests
{
    private readonly IRatingRepository _ratingRepository = Substitute.For<IRatingRepository>();
    private readonly IUserDirectoryClient _userDirectory = Substitute.For<IUserDirectoryClient>();

    private readonly GetStoryRatingsQueryHandler _handler;

    public GetStoryRatingsQueryHandlerTests()
    {
        _userDirectory.GetDisplayNamesAsync(Arg.Any<IEnumerable<long>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<long, string>());
        _handler = new GetStoryRatingsQueryHandler(_ratingRepository, _userDirectory);
    }

    [Fact]
    public async Task Handle_Should_ReturnPagedRatings_When_Called()
    {
        var storyId = Guid.NewGuid();
        var ratings = new List<Rating>
        {
            new() { StoryId = storyId, UserId = 1, Score = 5 },
            new() { StoryId = storyId, UserId = 2, Score = 3 }
        };
        _ratingRepository.GetByStoryAsync(storyId, 1, 20, Arg.Any<CancellationToken>())
            .Returns((ratings, 2));

        var query = new GetStoryRatingsQuery { StoryId = storyId, PageNumber = 1, PageSize = 20 };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task Handle_Should_NormalizePagingParameters_When_InvalidValuesGiven()
    {
        var storyId = Guid.NewGuid();
        _ratingRepository
            .GetByStoryAsync(storyId, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((new List<Rating>(), 0));

        var query = new GetStoryRatingsQuery { StoryId = storyId, PageNumber = 0, PageSize = -5 };

        await _handler.Handle(query, CancellationToken.None);

        await _ratingRepository.Received(1).GetByStoryAsync(storyId, 1, Arg.Is<int>(size => size > 0), Arg.Any<CancellationToken>());
    }
}
