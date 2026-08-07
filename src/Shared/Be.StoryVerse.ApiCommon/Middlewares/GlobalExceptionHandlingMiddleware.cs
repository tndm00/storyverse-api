namespace Be.StoryVerse.ApiCommon.Middlewares;

/// <summary>
/// Centralized exception handling for every API host. Converts unhandled
/// exceptions into the standard <see cref="ResponseDto{T}"/> error envelope so
/// controllers never need their own try/catch for this, per code-standard.md
/// section 35 (Exception Handling Rules).
/// </summary>
public sealed class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errorCode) = Map(exception);

        // Unexpected exceptions are logged with the full exception; known business
        // exceptions are expected control flow and logged at a lower level.
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, ApiCommonLogConstants.UnhandledException, context.Request.Path);
        }
        else
        {
            _logger.LogWarning(ApiCommonLogConstants.RequestFailed, context.Request.Path, errorCode, exception.Message);
        }

        var response = ResponseDto<object>.Fail(new ResponseErrorDto
        {
            Code = errorCode,
            // Do not leak stack traces, SQL errors, or connection strings to clients.
            Message = exception.Message,
            Details = Array.Empty<string>()
        });

        context.Response.ContentType = ApiCommonConstants.JsonContentType;
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static (HttpStatusCode StatusCode, string ErrorCode) Map(Exception exception) => exception switch
    {
        BadRequestException => (HttpStatusCode.BadRequest, ErrorConstants.BadRequest),
        NotFoundException => (HttpStatusCode.NotFound, ErrorConstants.NotFound),
        ForbiddenException => (HttpStatusCode.Forbidden, ErrorConstants.Forbidden),
        ConflictException => (HttpStatusCode.Conflict, ErrorConstants.Conflict),
        BusinessRuleException => (HttpStatusCode.UnprocessableEntity, ErrorConstants.ValidationFailed),
        _ => (HttpStatusCode.InternalServerError, ErrorConstants.Unexpected)
    };
}

/// <summary>
/// Registers <see cref="GlobalExceptionHandlingMiddleware"/> in the request pipeline.
/// </summary>
public static class GlobalExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseStoryVerseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
}
