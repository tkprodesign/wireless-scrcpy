namespace WirelessScrcpy.Core.Common;

public sealed record AppError(ErrorCode Code, string UserMessage, string DiagnosticMessage, Severity Severity)
{
    public static AppError Unexpected(Exception exception) => new(
        ErrorCode.UnexpectedError,
        "An unexpected error occurred. The session was stopped safely.",
        exception.ToString(),
        Severity.Error);
}
