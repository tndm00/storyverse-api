namespace Content.Application.Commands.Marketing.PostFacebookDigest;

/// <summary>
/// Posts a single "truyện hot hôm nay" digest to the Facebook Page, listing the
/// top-viewed published stories. Invoked on a timer by
/// <c>FacebookDailyDigestPublisher</c>, not from an HTTP endpoint.
/// </summary>
public sealed class PostFacebookDigestCommand : ICommand<PostFacebookDigestResultDto>
{
    /// <summary>How many top stories to list in the digest.</summary>
    public int TopCount { get; init; } = 5;
}

/// <summary>Outcome of one digest posting attempt.</summary>
public sealed class PostFacebookDigestResultDto
{
    public int StoryCount { get; init; }
}
