using System.Threading;

namespace Be.StoryVerse.Shared;

/// <summary>
/// Ambient holder for the current request's correlation ID, set by
/// CorrelationIdMiddleware (Be.StoryVerse.ApiCommon) at the start of the
/// pipeline. <see cref="Dtos.ResponseMetaDto.CorrelationId"/> reads this as
/// its default so every response envelope carries the ID without every
/// ResponseDto.Ok/Fail call site needing to pass it explicitly.
/// </summary>
public static class CorrelationContext
{
    private static readonly AsyncLocal<string> CurrentValue = new();

    public static string CorrelationId
    {
        get => CurrentValue.Value;
        set => CurrentValue.Value = value;
    }
}
