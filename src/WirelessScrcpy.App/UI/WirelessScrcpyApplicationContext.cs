using WirelessScrcpy.App.Composition;
using WirelessScrcpy.App.UI.Tray;
using WirelessScrcpy.Core.Logging;
using WirelessScrcpy.Core.Settings;
using WirelessScrcpy.Core.Workflow;

namespace WirelessScrcpy.App.UI;

public sealed class WirelessScrcpyApplicationContext : ApplicationContext
{
    private readonly ApplicationLifetime _lifetime;
    private readonly WirelessScrcpyController _controller;
    private readonly WindowsFormsSynchronizationContextDispatcher _dispatcher;
    private readonly Logger _logger;
    private readonly SettingsManager _settingsManager;
    private readonly StatusForm _statusForm;
    private readonly TrayIconController _trayIconController;
    private bool _shutdownStarted;
    private bool _minimizeNoticeShown;

    public WirelessScrcpyApplicationContext(
        ApplicationLifetime lifetime,
        WirelessScrcpyController controller,
        WindowsFormsSynchronizationContextDispatcher dispatcher,
        Logger logger,
        SettingsManager settingsManager,
        StatusForm statusForm,
        TrayMenuFactory trayMenuFactory)
    {
        _lifetime = lifetime;
        _controller = controller;
        _dispatcher = dispatcher;
        _logger = logger;
        _settingsManager = settingsManager;
        _statusForm = statusForm;
        _trayIconController = new TrayIconController(trayMenuFactory, ShowStatus, StartWorkflow, StopWorkflow, ExitThread);
        _controller.SnapshotChanged += OnSnapshotChanged;
        _statusForm.ConnectRequested += (_, _) => StartWorkflow();
        _statusForm.DisconnectRequested += (_, _) => StopWorkflow();
        _statusForm.ExitRequested += (_, _) => ExitThread();
        _statusForm.Resize += (_, _) => NotifyMinimizedToTray();
        _statusForm.FormClosing += (_, e) =>
        {
            if (!_shutdownStarted)
            {
                e.Cancel = true;
                _statusForm.Hide();
                NotifyMinimizedToTray();
            }
        };
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _lifetime.StartAsync(cancellationToken).ConfigureAwait(false);
        if (_settingsManager.Current.ShowStatusWindowOnStart)
        {
            ShowStatus();
        }
    }

    private void ShowStatus()
    {
        if (_statusForm.IsDisposed) return;
        if (_statusForm.InvokeRequired)
        {
            _statusForm.BeginInvoke((Action)ShowStatus);
            return;
        }

        _statusForm.Show();
        _statusForm.WindowState = FormWindowState.Normal;
        _statusForm.ShowInTaskbar = true;
        _statusForm.Activate();
    }

    private void StartWorkflow() => RunControllerOperation(() => _controller.StartAsync(), "UnhandledWorkflowStartError");

    private void StopWorkflow() => RunControllerOperation(() => _controller.StopAsync(), "UnhandledWorkflowStopError");

    private void RunControllerOperation(Func<Task> operation, string errorCode)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await operation().ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                await _logger.ErrorAsync(errorCode, exception.ToString()).ConfigureAwait(false);
            }
        });
    }

    private void OnSnapshotChanged(object? sender, WorkflowSnapshot snapshot) => _dispatcher.Post(() =>
    {
        _statusForm.ApplySnapshot(snapshot);
        _trayIconController.Update(snapshot.State, snapshot.DetailMessage);
    });

    private void NotifyMinimizedToTray()
    {
        if (_minimizeNoticeShown || _shutdownStarted || _statusForm.WindowState != FormWindowState.Minimized && _statusForm.Visible)
        {
            return;
        }

        _minimizeNoticeShown = true;
        _trayIconController.ShowBalloon("Wireless Scrcpy", "Wireless Scrcpy is still running in the system tray.", ToolTipIcon.Info);
    }

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
