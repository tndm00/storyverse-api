namespace Content.Application.Text;

/// <summary>
/// Derives a short story description from chapter content when the author
/// leaves the description blank (quick-publish / guest-publish flows no
/// longer collect it explicitly).
/// </summary>
public static class DescriptionExcerpt
{
    private const int MaxLength = 200;

    public static string From(string chapterContent)
    {
        if (string.IsNullOrWhiteSpace(chapterContent))
        {
            return string.Empty;
        }

        var flattened = Regex.Replace(chapterContent.Trim(), @"\s+", " ");
        if (flattened.Length <= MaxLength)
        {
            return flattened;
        }

        var cut = flattened[..MaxLength];
        var lastSpace = cut.LastIndexOf(' ');
        if (lastSpace > 0)
        {
            cut = cut[..lastSpace];
        }

        return cut.TrimEnd() + "…";
    }
}
