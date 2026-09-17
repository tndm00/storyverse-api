namespace Content.Application.Text;

/// <summary>
/// Approximate word count for chapter content, recomputed on every write.
/// Whitespace-delimited tokens; good enough for reading-time estimates and
/// author stats.
/// </summary>
public static class WordCounter
{
    private static readonly Regex Whitespace = new(@"\s+", RegexOptions.Compiled);

    /// <summary>Counts whitespace-delimited words in <paramref name="content"/>, returning 0 for blank input.</summary>
    public static int Count(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return 0;
        }

        return Whitespace.Split(content.Trim()).Count(token => token.Length > 0);
    }
}
