using System.Diagnostics;

namespace WirelessScrcpy.Core.Diagnostics;

public sealed class ProcessHandle : IAsyncDisposable, IDisposable
{
    private readonly Process _process;

    public ProcessHandle(Process process) => _process = process;

    public int Id => _process.Id;
    public bool HasExited => _process.HasExited;
    public int? ExitCode => _process.HasExited ? _process.ExitCode : null;
    public Task WaitForExitAsync(CancellationToken cancellationToken = default) => _process.WaitForExitAsync(cancellationToken);

    public void Stop()
    {
        if (!_process.HasExited)
        {
            _process.Kill(entireProcessTree: true);
        }
    }

    public void Dispose() => _process.Dispose();

    public ValueTask DisposeAsync()
    {
        _process.Dispose();
        return ValueTask.CompletedTask;
    }
}
