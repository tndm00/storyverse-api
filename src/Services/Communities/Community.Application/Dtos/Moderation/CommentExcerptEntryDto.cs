namespace Community.Application.Dtos;

/// <summary>Internal batch lookup entry: comment public id -&gt; short content excerpt.</summary>
public sealed class CommentExcerptEntryDto
{
    public Guid Id { get; init; }

    public string Excerpt { get; init; }
}
