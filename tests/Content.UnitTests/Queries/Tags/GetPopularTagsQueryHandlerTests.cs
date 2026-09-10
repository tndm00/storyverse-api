using Content.Application.Interfaces.Repositories;
using Content.Application.Queries.Tags.GetPopularTags;
using Content.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Queries.Tags;

public class GetPopularTagsQueryHandlerTests
{
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();

    private readonly GetPopularTagsQueryHandler _handler;

    public GetPopularTagsQueryHandlerTests()
    {
        _handler = new GetPopularTagsQueryHandler(_tagRepository);
    }

    [Fact]
    public async Task Handle_Should_ReturnPopularTags_When_RequestedCountIsValid()
    {
        var tag = new Tag { Id = 1, Name = "Isekai", Slug = "isekai", UsageCount = 42 };
        _tagRepository.GetPopularAsync(10, Arg.Any<CancellationToken>()).Returns(new List<Tag> { tag });

        var query = new GetPopularTagsQuery { Count = 10 };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().ContainSingle(t => t.Slug == "isekai" && t.UsageCount == 42);
    }

    [Fact]
    public async Task Handle_Should_UseDefaultCount_When_RequestedCountIsZeroOrNegative()
    {
        int capturedCount = -1;
        _tagRepository.GetPopularAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                capturedCount = ci.Arg<int>();
                return new List<Tag>();
            });

        var query = new GetPopularTagsQuery { Count = 0 };

        await _handler.Handle(query, CancellationToken.None);

        capturedCount.Should().Be(30);
    }
}
