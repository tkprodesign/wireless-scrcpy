using WirelessScrcpy.App.Composition;
using WirelessScrcpy.App.UI.Tray;
using WirelessScrcpy.Core.Logging;
using WirelessScrcpy.Core.Workflow;

namespace WirelessScrcpy.App.UI;

public sealed class WirelessScrcpyApplicationContext : ApplicationContext
{
    private readonly ApplicationLifetime _lifetime;
    private readonly WirelessScrcpyController _controller;
    private readonly WindowsFormsSynchronizationContextDispatcher _dispatcher;
    private readonly Logger _logger;
    private readonly StatusForm _statusForm;
    private readonly TrayIconController _trayIconController;
    private bool _shutdownStarted;

    public WirelessScrcpyApplicationContext(
        ApplicationLifetime lifetime,
        WirelessScrcpyController controller,
        WindowsFormsSynchronizationContextDispatcher dispatcher,
        Logger logger,
        StatusForm statusForm,
        TrayMenuFactory trayMenuFactory)
    {
        _lifetime = lifetime;
        _controller = controller;
        _dispatcher = dispatcher;
        _logger = logger;
        _statusForm = statusForm;
        _trayIconController = new TrayIconController(trayMenuFactory, ShowStatus, StartWorkflow, ExitThread);
        _controller.SnapshotChanged += OnSnapshotChanged;
        _statusForm.FormClosing += (_, e) =>
        {
            if (!_shutdownStarted)
            {
                e.Cancel = true;
                _statusForm.Hide();
            }
        };
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _lifetime.StartAsync(cancellationToken).ConfigureAwait(false);
        ShowStatus();
    }

    private void ShowStatus()
    {
        if (_statusForm.Visible)
        {
            _statusForm.Activate();
            return;
        }
        _statusForm.Show();
    }

    private void StartWorkflow()
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await _controller.StartAsync().ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                await _logger.ErrorAsync("UnhandledWorkflowError", exception.ToString()).ConfigureAwait(false);
            }
        });
    }

    private void OnSnapshotChanged(object? sender, WorkflowSnapshot snapshot) => _dispatcher.Post(() => _statusForm.ApplySnapshot(snapshot));

    protected override async void ExitThreadCore()
    {
        if (_shutdownStarted) return;
        _shutdownStarted = true;
        _controller.SnapshotChanged -= OnSnapshotChanged;
        try
        {
            await _lifetime.StopAsync().ConfigureAwait(false);
        }
        finally
        {
            _trayIconController.Dispose();
            _statusForm.Dispose();
            base.ExitThreadCore();
        }
    }
}
