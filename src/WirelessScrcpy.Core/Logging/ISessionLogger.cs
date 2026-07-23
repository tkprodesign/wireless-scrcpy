using WirelessScrcpy.Core.Common;

namespace WirelessScrcpy.Core.Logging;

public interface ISessionLogger : IAsyncDisposable
{
    Task WriteAsync(Severity level, string eventName, string message, CancellationToken cancellationToken = default);
}
