namespace Content.Application.Text;

/// <summary>
/// Derives a short story description from chapter content when the author
/// leaves the description blank (quick-publish / guest-publish flows no
/// longer collect it explicitly).
/// </summary>
public static class DescriptionExcerpt
{
    private const int MaxLength = 200;

    /// <summary>Builds a short excerpt (up to <see cref="MaxLength"/> chars) from chapter content, cutting cleanly at a word boundary and appending an ellipsis when truncated.</summary>
    public static string From(string chapterContent)
    {
        if (string.IsNullOrWhiteSpace(chapterContent))
        {
            return string.Empty;
        }

        // Collapse all whitespace/newlines into single spaces.
        var flattened = Regex.Replace(chapterContent.Trim(), @"\s+", " ");
        if (flattened.Length <= MaxLength)
        {
            return flattened;
        }

        // Truncate at the max length, then back up to the last full word so the
        // excerpt doesn't end mid-word.
        var cut = flattened[..MaxLength];
        var lastSpace = cut.LastIndexOf(' ');
        if (lastSpace > 0)
        {
            cut = cut[..lastSpace];
        }

        return cut.TrimEnd() + "…";
    }
}
