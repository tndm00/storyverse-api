namespace Be.StoryVerse.Shared.Dtos;

/// <summary>
/// Standard API response envelope used by every service. All controller actions
/// must return this shape for both success and error responses, per
/// api-guidelines.md section 12 (Response Envelope) and code-standard.md section 16.
/// </summary>
/// <typeparam name="T">Type of the payload returned on success.</typeparam>
public sealed class ResponseDto<T>
{
    public bool Success { get; init; }

    public T Data { get; init; }

    public ResponseErrorDto Error { get; init; }

    public ResponseMetaDto Meta { get; init; } = new();

    public static ResponseDto<T> Ok(T data, ResponseMetaDto meta = null)
    {
        return new ResponseDto<T>
        {
            Success = true,
            Data = data,
            Error = null,
            Meta = meta ?? new ResponseMetaDto()
        };
    }

    public static ResponseDto<T> Fail(ResponseErrorDto error, ResponseMetaDto meta = null)
    {
        return new ResponseDto<T>
        {
            Success = false,
            Data = default,
            Error = error,
            Meta = meta ?? new ResponseMetaDto()
        };
    }
}

/// <summary>
/// Error payload for a failed <see cref="ResponseDto{T}"/>.
/// </summary>
public sealed class ResponseErrorDto
{
    public string Code { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public IReadOnlyCollection<string> Details { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Response metadata: correlation/trace identifiers and timestamps, per
/// code-standard.md section 16.
/// </summary>
public sealed class ResponseMetaDto
{
    public string RequestId { get; init; }

    public string CorrelationId { get; init; }

    public string TraceId { get; init; }

    public string SpanId { get; init; }

    public string Version { get; init; } = "1.0";

    public DateTimeOffset RequestTimestamp { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset ResponseTimestamp { get; init; } = DateTimeOffset.UtcNow;
}
