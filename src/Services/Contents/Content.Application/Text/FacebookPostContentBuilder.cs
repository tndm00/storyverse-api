namespace Content.Application.Text;

/// <summary>
/// Builds the message/link pair posted to the Facebook Page when a chapter goes
/// live, either via moderator approval or the scheduled auto-publisher. Shared
/// by both call sites so the two publish paths announce chapters identically.
/// </summary>
public static class FacebookPostContentBuilder
{
    public static (string Message, string Link) ForPublishedChapter(Story story, Chapter chapter)
    {
        // OrderIndex <= 1 is the established "first chapter / standalone short
        // story" convention used elsewhere (FE hides "Chương 1." labels, report
        // target selection, ...).
        if (chapter.OrderIndex <= 1)
        {
            var message = string.IsNullOrWhiteSpace(story.Description)
                ? $"👻 Truyện mới: {story.Title}"
                : $"👻 Truyện mới: {story.Title}\n\n{story.Description}";

            return (message, $"{ApplicationConstants.SiteBaseUrl}/truyen/{story.Slug}");
        }

        var updateMessage =
            $"📣 \"{story.Title}\" vừa có chương mới: Chương {chapter.OrderIndex:0.##}. {chapter.Title}";

        return (updateMessage, $"{ApplicationConstants.SiteBaseUrl}/truyen/{story.Slug}/chuong/{chapter.OrderIndex:0.##}");
    }
}
