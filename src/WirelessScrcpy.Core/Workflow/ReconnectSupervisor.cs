using WirelessScrcpy.Core.Adb;
using WirelessScrcpy.Core.Common;
using WirelessScrcpy.Core.Devices;
using WirelessScrcpy.Core.Logging;
using WirelessScrcpy.Core.Scrcpy;

namespace WirelessScrcpy.Core.Workflow;

public sealed class ReconnectSupervisor
{
    private readonly WorkflowOptions _options;
    private readonly AdbManager _adbManager;
    private readonly ScrcpyManager _scrcpyManager;
    private readonly Logger _logger;

    public ReconnectSupervisor(WorkflowOptions options, AdbManager adbManager, ScrcpyManager scrcpyManager, Logger logger)
    {
        _options = options;
        _adbManager = adbManager;
        _scrcpyManager = scrcpyManager;
        _logger = logger;
    }

    public async Task<Result<ScrcpySession>> ReconnectAsync(NetworkEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        DateTimeOffset deadline = DateTimeOffset.UtcNow.Add(_options.ReconnectWindow);
        int attempt = 0;
        while (DateTimeOffset.UtcNow <= deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            attempt++;
            await _logger.WarningAsync("ReconnectAttempt", $"Attempt {attempt} to reconnect {endpoint}.", cancellationToken).ConfigureAwait(false);
            Result<bool> connect = await _adbManager.ConnectWirelessAsync(endpoint, cancellationToken).ConfigureAwait(false);
            if (connect.IsSuccess)
            {
                Result<AndroidDevice> verified = await _adbManager.VerifyWirelessDeviceAsync(endpoint, cancellationToken).ConfigureAwait(false);
                if (verified.IsSuccess)
                {
                    Result<ScrcpySession> scrcpy = _scrcpyManager.Launch(endpoint.ToString());
                    if (scrcpy.IsSuccess)
                    {
                        await _logger.InfoAsync("ReconnectSucceeded", $"Reconnected {endpoint} and relaunched scrcpy.", cancellationToken).ConfigureAwait(false);
                        return scrcpy;
                    }
                    return scrcpy;
                }
            }

            await Task.Delay(_options.ReconnectDelay, cancellationToken).ConfigureAwait(false);
        }

        return Result<ScrcpySession>.Failure(new AppError(ErrorCode.WirelessConnectionFailed, "The wireless connection could not be restored.", $"Reconnect window expired for {endpoint}.", Severity.Error));
    }
}
