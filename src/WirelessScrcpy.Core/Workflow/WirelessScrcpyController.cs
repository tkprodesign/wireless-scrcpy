namespace WirelessScrcpy.Core.Workflow;

public sealed class WirelessScrcpyController : IAsyncDisposable
{
    private readonly WirelessScrcpyWorkflow _workflow;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private SessionHandle? _activeSession;
    private Task? _activeTask;

    public WirelessScrcpyController(WirelessScrcpyWorkflow workflow) => _workflow = workflow;

    public event EventHandler<WorkflowSnapshot>? SnapshotChanged
    {
        add => _workflow.SnapshotChanged += value;
        remove => _workflow.SnapshotChanged -= value;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_activeTask is { IsCompleted: false }) return;
            _activeSession = new SessionHandle();
            _activeTask = Task.Run(() => _workflow.RunAsync(_activeSession, _activeSession.CancellationToken), CancellationToken.None);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task StopAsync()
    {
        SessionHandle? session;
        Task? activeTask;
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            session = _activeSession;
            activeTask = _activeTask;
            _activeSession = null;
            _activeTask = null;
            session?.Cancel();
        }
        finally
        {
            _gate.Release();
        }

        if (activeTask is not null)
        {
            try
            {
                await activeTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
        }

        if (session is not null)
        {
            await session.DisposeAsync().ConfigureAwait(false);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        _gate.Dispose();
    }
}
