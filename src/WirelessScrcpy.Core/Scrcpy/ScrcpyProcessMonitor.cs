namespace WirelessScrcpy.Core.Scrcpy;

public sealed class ScrcpyProcessMonitor
{
    public Task WaitForExitAsync(ScrcpySession session, CancellationToken cancellationToken = default) => session.WaitForExitAsync(cancellationToken);
}
