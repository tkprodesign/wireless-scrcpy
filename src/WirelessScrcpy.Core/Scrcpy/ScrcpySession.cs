using WirelessScrcpy.Core.Diagnostics;

namespace WirelessScrcpy.Core.Scrcpy;

public sealed class ScrcpySession : IAsyncDisposable
{
    private readonly ProcessHandle _process;
    private bool _stopRequested;

    public ScrcpySession(ProcessHandle process) => _process = process;

    public bool HasExited => _process.HasExited;
    public bool StopRequested => _stopRequested;
    public int? ExitCode => _process.ExitCode;

    public Task WaitForExitAsync(CancellationToken cancellationToken = default) => _process.WaitForExitAsync(cancellationToken);

    public ValueTask StopAsync()
    {
        _stopRequested = true;
        _process.Stop();
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        await _process.DisposeAsync().ConfigureAwait(false);
    }
}
