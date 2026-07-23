using WirelessScrcpy.Core.Common;
using WirelessScrcpy.Core.Logging;

namespace WirelessScrcpy.Core.Diagnostics;

public sealed class ExceptionHandler
{
    private readonly Logger _logger;

    public ExceptionHandler(Logger logger) => _logger = logger;

    public async Task<AppError> HandleAsync(string eventName, Exception exception, CancellationToken cancellationToken = default)
    {
        AppError error = AppError.Unexpected(exception);
        await _logger.ErrorAsync(eventName, error.DiagnosticMessage, cancellationToken).ConfigureAwait(false);
        return error;
    }
}
