using WirelessScrcpy.Core.Logging;
using WirelessScrcpy.Core.Settings;
using WirelessScrcpy.Core.Workflow;

namespace WirelessScrcpy.App.Composition;

public sealed class ApplicationLifetime : IAsyncDisposable
{
    private readonly WirelessScrcpyController _controller;
    private readonly SettingsManager _settingsManager;
    private readonly Logger _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _started;
    private bool _stopped;

    public ApplicationLifetime(WirelessScrcpyController controller, SettingsManager settingsManager, Logger logger)
    {
        _controller = controller;
        _settingsManager = settingsManager;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_started) return;
            await _settingsManager.LoadAsync(cancellationToken).ConfigureAwait(false);
            await _logger.InfoAsync("ApplicationStarted", "Wireless Scrcpy application started.", cancellationToken).ConfigureAwait(false);
            _started = true;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task StopAsync()
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_stopped) return;
            _stopped = true;
            await _logger.InfoAsync("ApplicationStopping", "Wireless Scrcpy application is stopping.").ConfigureAwait(false);
            await _controller.StopAsync().ConfigureAwait(false);
            await _settingsManager.SaveAsync(_settingsManager.Current).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        _gate.Dispose();
    }
}
