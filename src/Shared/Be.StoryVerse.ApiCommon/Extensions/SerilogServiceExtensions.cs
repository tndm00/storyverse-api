using Be.StoryVerse.ApiCommon.Logging;
using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Be.StoryVerse.ApiCommon.Extensions;

/// <summary>
/// Structured JSON console logging for every API host, replacing the default
/// plain-text console formatter. Filebeat's <c>decode_json_fields</c> (see
/// deploy/filebeat.yml) flattens each line straight into Elasticsearch
/// fields, so <see cref="CorrelationIdEnricher"/>'s <c>CorrelationId</c> and
/// every other log property (SourceContext, level, message template, ...)
/// become searchable/filterable in Kibana instead of only appearing as free
/// text inside a single <c>message</c> field.
/// </summary>
public static class SerilogServiceExtensions
{
    public static WebApplicationBuilder UseStoryVerseSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((_, _, configuration) => configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.With<CorrelationIdEnricher>()
            .WriteTo.Console(new CompactJsonFormatter()));

        return builder;
    }
}
