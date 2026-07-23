using WirelessScrcpy.Core.Common;

namespace WirelessScrcpy.Core.Logging;

public sealed class Logger
{
    private readonly ISessionLogger _sessionLogger;

    public Logger(ISessionLogger sessionLogger) => _sessionLogger = sessionLogger;

    public Task InfoAsync(string eventName, string message, CancellationToken cancellationToken = default) =>
        _sessionLogger.WriteAsync(Severity.Info, eventName, message, cancellationToken);

    public Task WarningAsync(string eventName, string message, CancellationToken cancellationToken = default) =>
        _sessionLogger.WriteAsync(Severity.Warning, eventName, message, cancellationToken);

    public Task ErrorAsync(string eventName, string message, CancellationToken cancellationToken = default) =>
        _sessionLogger.WriteAsync(Severity.Error, eventName, message, cancellationToken);
}
