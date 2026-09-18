using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Content.Infrastructure.Search;

/// <summary>
/// See <see cref="IStorySearchService"/>. Every method is a no-op (writes) or
/// throws-caught-by-caller (search) when <see cref="ElasticsearchOptions.Enabled"/>
/// is false, so the feature can ship dark and be toggled purely by config.
/// Failures never propagate to the caller as a request failure — they're
/// logged and swallowed (writes) or rethrown for the caller's own fallback to
/// Postgres to handle (search), per the plan's log-and-continue policy.
/// </summary>
public sealed class ElasticsearchStorySearchService : IStorySearchService
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticsearchOptions _options;
    private readonly ILogger<ElasticsearchStorySearchService> _logger;

    public bool IsEnabled => _options.Enabled;

    public bool IsSearchReadEnabled => _options.SearchReadEnabled;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElasticsearchStorySearchService"/> class.
    /// </summary>
    public ElasticsearchStorySearchService(
        ElasticsearchClient client,
        IOptions<ElasticsearchOptions> options,
        ILogger<ElasticsearchStorySearchService> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Maps <paramref name="story"/> to a search document and upserts it into the index.
    /// No-op when Elasticsearch is disabled or the story is a draft; indexing failures are
    /// logged and swallowed rather than surfaced to the caller.
    /// </summary>
    public async Task IndexAsync(Story story, string publishedChapterContent, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return;
        }

        // Draft stories are never returned by public search and have no
        // published content worth indexing yet — see IStorySearchService.IndexAsync.
        if (story.Status == StoryStatus.Draft)
        {
            return;
        }

        try
        {
            var document = StoryDocumentMapper.ToDocument(story, publishedChapterContent);

            await _client.IndexAsync(document, _options.IndexName, story.Id.ToString(), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, InfrastructureLogConstants.SearchIndexFailed, story.Id);
        }
    }

    /// <summary>
    /// Removes a story's document from the index. No-op when Elasticsearch is disabled;
    /// failures are logged and swallowed rather than surfaced to the caller.
    /// </summary>
    public async Task DeleteAsync(long storyId, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return;
        }

        try
        {
            await _client.DeleteAsync(new DeleteRequest(_options.IndexName, storyId.ToString()), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, InfrastructureLogConstants.SearchDeleteFailed, storyId);
        }
    }

    /// <summary>
    /// Runs a keyword/filter/sort search against the story index and returns the matching
    /// story ids plus total hit count. Returns an empty result when search reads are
    /// disabled; failures propagate so the caller can fall back to Postgres.
    /// </summary>
    public async Task<(IReadOnlyList<long> StoryIds, int TotalCount)> SearchAsync(
        StorySearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || !_options.SearchReadEnabled)
        {
            return (Array.Empty<long>(), 0);
        }

        // Keyword match across title (boosted), description and chapter content, with
        // fuzzy matching to tolerate minor typos.
        var must = new List<Query>
        {
            new MultiMatchQuery
            {
                Query = criteria.Keyword,
                Fields = new[] { "title^3", "description", "chapterContent" },
                Fuzziness = new Fuzziness("AUTO")
            }
        };

        // Status/genreSlugs/tagSlugs are dynamically mapped as analyzed "text" with
        // an unanalyzed ".keyword" sub-field (see storyverse-stories mapping) — a
        // TermQuery against the bare field name compares against lowercased,
        // tokenized text and would never match an exact value like "Ongoing", so
        // every filter here must target the ".keyword" sub-field instead.
        // Status filter always applies; genre/tag/author filters are added only when
        // the caller actually supplied them.
        var filter = new List<Query>
        {
            new TermQuery("status.keyword") { Value = (criteria.Status ?? StoryStatus.Ongoing).ToString() }
        };

        if (!string.IsNullOrEmpty(criteria.GenreSlug))
        {
            filter.Add(new TermQuery("genreSlugs.keyword") { Value = criteria.GenreSlug });
        }

        if (!string.IsNullOrEmpty(criteria.TagSlug))
        {
            filter.Add(new TermQuery("tagSlugs.keyword") { Value = criteria.TagSlug });
        }

        if (criteria.AuthorProfileId is { } authorProfileId)
        {
            filter.Add(new TermQuery("authorProfileId") { Value = authorProfileId });
        }

        var query = new BoolQuery { Must = must, Filter = filter };

        // Execute the paged search with the combined query and requested sort order.
        var response = await _client.SearchAsync<StoryDocument>(s => s
            .Index(_options.IndexName)
            .Query(query)
            .Sort(BuildSort(criteria))
            .From((criteria.PageNumber - 1) * criteria.PageSize)
            .Size(criteria.PageSize),
            cancellationToken);

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException(
                $"Elasticsearch search failed: {response.DebugInformation}");
        }

        // Only story ids and the total count are returned — callers hydrate full
        // story data from Postgres.
        var storyIds = response.Documents.Select(d => d.StoryId).ToArray();
        var totalCount = (int)(response.Total);

        return (storyIds, totalCount);
    }

    /// <summary>
    /// Returns the number of documents currently in the index, or 0 when Elasticsearch is
    /// disabled or the count request fails.
    /// </summary>
    public async Task<long> GetDocumentCountAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return 0;
        }

        try
        {
            var response = await _client.CountAsync(c => c.Index(_options.IndexName), cancellationToken);
            return response.IsValidResponse ? response.Count : 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, InfrastructureLogConstants.SearchCountFailed);
            return 0;
        }
    }

    /// <summary>
    /// Builds the Elasticsearch sort clause matching the requested <see cref="StorySearchCriteria.SortBy"/>
    /// field and direction, defaulting to <c>publishedAt</c> for unrecognized values.
    /// </summary>
    private static Action<SortOptionsDescriptor<StoryDocument>> BuildSort(StorySearchCriteria criteria)
    {
        return sort =>
        {
            var order = criteria.Descending ? SortOrder.Desc : SortOrder.Asc;
            switch (criteria.SortBy)
            {
                case StorySortField.Title:
                    sort.Field("title.keyword", f => f.Order(order));
                    break;
                case StorySortField.ViewCount:
                    sort.Field("viewCount", f => f.Order(order));
                    break;
                case StorySortField.RatingAvg:
                    sort.Field("ratingAvg", f => f.Order(order));
                    break;
                case StorySortField.CreatedAt:
                    sort.Field("createdAt", f => f.Order(order));
                    break;
                default:
                    sort.Field("publishedAt", f => f.Order(order));
                    break;
            }
        };
    }
}
