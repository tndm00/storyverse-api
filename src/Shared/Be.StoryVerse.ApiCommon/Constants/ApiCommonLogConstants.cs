namespace Be.StoryVerse.ApiCommon.Constants;

/// <summary>
/// Centralized structured-logging message templates for shared API pipeline
/// components (for example <see cref="Middlewares.GlobalExceptionHandlingMiddleware"/>),
/// per code-standard.md section 12 (Logging Rules).
/// </summary>
public static class ApiCommonLogConstants
{
    public const string UnhandledException = "Unhandled exception while processing {Path}";

    public const string RequestFailed = "Request to {Path} failed with {ErrorCode}: {Message}";
}
