using WirelessScrcpy.Core.Abstractions;
using WirelessScrcpy.Core.Adb;
using WirelessScrcpy.Core.Common;
using WirelessScrcpy.Core.Devices;
using WirelessScrcpy.Core.Diagnostics;
using WirelessScrcpy.Core.Logging;
using WirelessScrcpy.Core.Notifications;
using WirelessScrcpy.Core.Scrcpy;
using WirelessScrcpy.Core.Settings;

namespace WirelessScrcpy.Core.Workflow;

public sealed class WirelessScrcpyWorkflow
{
    private readonly WorkflowStateMachine _stateMachine;
    private readonly SettingsManager _settingsManager;
    private readonly ToolDiscoveryService _toolDiscoveryService;
    private readonly AdbManager _adbManager;
    private readonly DeviceManager _deviceManager;
    private readonly NetworkManager _networkManager;
    private readonly ScrcpyManager _scrcpyManager;
    private readonly ReconnectSupervisor _reconnectSupervisor;
    private readonly IUsbDisconnectPrompt _usbDisconnectPrompt;
    private readonly NotificationManager _notificationManager;
    private readonly Logger _logger;
    private readonly ExceptionHandler _exceptionHandler;
    private readonly WorkflowOptions _options;

    public WirelessScrcpyWorkflow(
        WorkflowStateMachine stateMachine,
        SettingsManager settingsManager,
        ToolDiscoveryService toolDiscoveryService,
        AdbManager adbManager,
        DeviceManager deviceManager,
        NetworkManager networkManager,
        ScrcpyManager scrcpyManager,
        ReconnectSupervisor reconnectSupervisor,
        IUsbDisconnectPrompt usbDisconnectPrompt,
        NotificationManager notificationManager,
        Logger logger,
        ExceptionHandler exceptionHandler,
        WorkflowOptions options)
    {
        _stateMachine = stateMachine;
        _settingsManager = settingsManager;
        _toolDiscoveryService = toolDiscoveryService;
        _adbManager = adbManager;
        _deviceManager = deviceManager;
        _networkManager = networkManager;
        _scrcpyManager = scrcpyManager;
        _reconnectSupervisor = reconnectSupervisor;
        _usbDisconnectPrompt = usbDisconnectPrompt;
        _notificationManager = notificationManager;
        _logger = logger;
        _exceptionHandler = exceptionHandler;
        _options = options;
    }

    public event EventHandler<WorkflowSnapshot>? SnapshotChanged
    {
        add => _stateMachine.SnapshotChanged += value;
        remove => _stateMachine.SnapshotChanged -= value;
    }

    public async Task RunAsync(SessionHandle session, CancellationToken cancellationToken = default)
    {
        try
        {
            await ExecuteAsync(session, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            await StopAsync("Session cancelled.", cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            AppError error = await _exceptionHandler.HandleAsync("UnhandledWorkflowError", exception, CancellationToken.None).ConfigureAwait(false);
            await FailAsync(error, WorkflowEvent.ScrcpyLaunchFailed, CancellationToken.None).ConfigureAwait(false);
        }
    }

    private async Task ExecuteAsync(SessionHandle session, CancellationToken cancellationToken)
    {
        Apply(WorkflowEvent.LaunchRequested, "Starting Wireless Scrcpy session.");
        await _logger.InfoAsync("WorkflowStarted", "Wireless Scrcpy workflow started.", cancellationToken).ConfigureAwait(false);

        Apply(WorkflowEvent.WorkflowInitialized, "Detecting adb.exe.");
        ApplicationSettings settings = await _settingsManager.LoadAsync(cancellationToken).ConfigureAwait(false);
        ToolDiscoveryResult tools = _toolDiscoveryService.Discover(settings);
        if (tools.Adb is null)
        {
            await FailAsync(tools.Error!, WorkflowEvent.AdbMissing, cancellationToken).ConfigureAwait(false);
            return;
        }

        _adbManager.ConfigurePath(tools.Adb.Path);
        await _logger.InfoAsync("AdbDetected", $"ADB detected from {tools.Adb.Source}.", cancellationToken).ConfigureAwait(false);
        Apply(WorkflowEvent.AdbFound, "Detecting scrcpy.exe.");

        if (tools.Scrcpy is null)
        {
            await FailAsync(tools.Error!, WorkflowEvent.ScrcpyMissing, cancellationToken).ConfigureAwait(false);
            return;
        }

        _scrcpyManager.ConfigurePath(tools.Scrcpy.Path);
        await _settingsManager.SaveAsync(settings with { AdbPath = tools.Adb.Path, ScrcpyPath = tools.Scrcpy.Path }, cancellationToken).ConfigureAwait(false);
        await _logger.InfoAsync("ScrcpyDetected", $"scrcpy detected from {tools.Scrcpy.Source}.", cancellationToken).ConfigureAwait(false);
        Apply(WorkflowEvent.ScrcpyFound, "Starting ADB server.");

        Result<bool> server = await _adbManager.EnsureServerStartedAsync(cancellationToken).ConfigureAwait(false);
        if (!server.IsSuccess)
        {
            await FailAsync(server.Error!, WorkflowEvent.AdbMissing, cancellationToken).ConfigureAwait(false);
            return;
        }
        Apply(WorkflowEvent.AdbServerReady, "Detecting USB device.");

        Result<AndroidDevice> usbDevice = await _deviceManager.DetectSingleUsbDeviceAsync(cancellationToken).ConfigureAwait(false);
        if (!usbDevice.IsSuccess)
        {
            WorkflowEvent failureEvent = usbDevice.Error!.Code switch
            {
                ErrorCode.MultipleUsbDevices => WorkflowEvent.MultipleUsbDevicesFound,
                ErrorCode.UnauthorizedDevice => WorkflowEvent.UsbDeviceUnauthorized,
                _ => WorkflowEvent.NoUsbDeviceFound
            };
            await FailAsync(usbDevice.Error, failureEvent, cancellationToken).ConfigureAwait(false);
            return;
        }
        Apply(WorkflowEvent.SingleUsbDeviceFound, $"Enabling TCP/IP on {usbDevice.Value.Serial}.", usbDevice.Value.Serial);

        Result<bool> tcpIp = await _adbManager.EnableTcpIpAsync(usbDevice.Value.Serial, _options.TcpPort, cancellationToken).ConfigureAwait(false);
        if (!tcpIp.IsSuccess)
        {
            await FailAsync(tcpIp.Error!, WorkflowEvent.TcpIpFailed, cancellationToken).ConfigureAwait(false);
            return;
        }
        Apply(WorkflowEvent.TcpIpEnabled, "Discovering phone IP address.", usbDevice.Value.Serial);

        Result<string> phoneIp = await _deviceManager.DiscoverPhoneIpAsync(usbDevice.Value.Serial, cancellationToken).ConfigureAwait(false);
        if (!phoneIp.IsSuccess)
        {
            await FailAsync(phoneIp.Error!, WorkflowEvent.PhoneIpMissing, cancellationToken).ConfigureAwait(false);
            return;
        }

        Result<NetworkEndpoint> endpoint = _networkManager.CreateEndpoint(phoneIp.Value, _options.TcpPort);
        if (!endpoint.IsSuccess)
        {
            await FailAsync(endpoint.Error!, WorkflowEvent.PhoneIpMissing, cancellationToken).ConfigureAwait(false);
            return;
        }
        Apply(WorkflowEvent.PhoneIpDiscovered, $"Phone IP discovered: {endpoint.Value}.", usbDevice.Value.Serial);

        bool disconnectConfirmed = await _usbDisconnectPrompt.ConfirmUsbDisconnectedAsync(cancellationToken).ConfigureAwait(false);
        if (!disconnectConfirmed)
        {
            Apply(WorkflowEvent.UserCancelled, "Launch cancelled by user.", usbDevice.Value.Serial, Severity.Warning);
            await StopAsync("Launch cancelled by user.", cancellationToken).ConfigureAwait(false);
            return;
        }
        Apply(WorkflowEvent.UserConfirmedUsbDisconnected, $"Connecting to {endpoint.Value}.", endpoint.Value.ToString());

        Result<bool> wirelessConnect = await _adbManager.ConnectWirelessAsync(endpoint.Value, cancellationToken).ConfigureAwait(false);
        if (!wirelessConnect.IsSuccess)
        {
            await FailAsync(wirelessConnect.Error!, WorkflowEvent.WirelessAdbFailed, cancellationToken).ConfigureAwait(false);
            return;
        }

        Result<AndroidDevice> wirelessDevice = await _deviceManager.VerifyWirelessDeviceAsync(endpoint.Value, cancellationToken).ConfigureAwait(false);
        if (!wirelessDevice.IsSuccess)
        {
            await FailAsync(wirelessDevice.Error!, WorkflowEvent.WirelessAdbFailed, cancellationToken).ConfigureAwait(false);
            return;
        }
        Apply(WorkflowEvent.WirelessAdbConnected, $"Wireless ADB connected: {endpoint.Value}.", endpoint.Value.ToString());

        Result<ScrcpySession> scrcpy = _scrcpyManager.Launch(endpoint.Value.ToString());
        if (!scrcpy.IsSuccess)
        {
            await FailAsync(scrcpy.Error!, WorkflowEvent.ScrcpyLaunchFailed, cancellationToken).ConfigureAwait(false);
            return;
        }
        session.AddResource(scrcpy.Value);
        Apply(WorkflowEvent.ScrcpyStarted, "scrcpy is running with --no-audio.", endpoint.Value.ToString());
        await NotifyAsync("Wireless Scrcpy", "scrcpy is running wirelessly.", Severity.Info, cancellationToken).ConfigureAwait(false);
        await MonitorSessionAsync(session, endpoint.Value, scrcpy.Value, cancellationToken).ConfigureAwait(false);
    }

    private async Task MonitorSessionAsync(SessionHandle session, NetworkEndpoint endpoint, ScrcpySession currentScrcpy, CancellationToken cancellationToken)
    {
        ScrcpySession activeScrcpy = currentScrcpy;
        while (!cancellationToken.IsCancellationRequested)
        {
            bool interrupted = await WaitForInterruptionAsync(endpoint, activeScrcpy, cancellationToken).ConfigureAwait(false);
            if (!interrupted)
            {
                await StopAsync("scrcpy session ended.", cancellationToken).ConfigureAwait(false);
                return;
            }

            Apply(WorkflowEvent.NetworkInterrupted, "Wireless connection interrupted. Reconnecting.", endpoint.ToString(), Severity.Warning);
            await NotifyAsync("Wireless Scrcpy", "Connection interrupted. Reconnecting.", Severity.Warning, cancellationToken).ConfigureAwait(false);
            Result<ScrcpySession> reconnect = await _reconnectSupervisor.ReconnectAsync(endpoint, cancellationToken).ConfigureAwait(false);
            if (!reconnect.IsSuccess)
            {
                await FailAsync(reconnect.Error!, WorkflowEvent.ReconnectExpired, cancellationToken).ConfigureAwait(false);
                return;
            }

            session.AddResource(reconnect.Value);
            activeScrcpy = reconnect.Value;
            Apply(WorkflowEvent.ReconnectSucceeded, "Wireless connection restored.", endpoint.ToString());
            await NotifyAsync("Wireless Scrcpy", "Connection restored.", Severity.Info, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<bool> WaitForInterruptionAsync(NetworkEndpoint endpoint, ScrcpySession scrcpySession, CancellationToken cancellationToken)
    {
        using var monitorCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task scrcpyExit = scrcpySession.WaitForExitAsync(monitorCancellation.Token);
        Task adbMonitor = MonitorAdbConnectionAsync(endpoint, monitorCancellation.Token);
        Task completed = await Task.WhenAny(scrcpyExit, adbMonitor).ConfigureAwait(false);
        await monitorCancellation.CancelAsync().ConfigureAwait(false);

        if (completed == scrcpyExit)
        {
            await _logger.WarningAsync("ScrcpyExited", $"scrcpy exited with code {scrcpySession.ExitCode}.", CancellationToken.None).ConfigureAwait(false);
            return !scrcpySession.StopRequested;
        }

        await _logger.WarningAsync("AdbConnectionLost", $"Wireless ADB verification failed for {endpoint}.", CancellationToken.None).ConfigureAwait(false);
        return true;
    }

    private async Task MonitorAdbConnectionAsync(NetworkEndpoint endpoint, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(_options.AdbMonitorInterval, cancellationToken).ConfigureAwait(false);
            Result<AndroidDevice> verified = await _deviceManager.VerifyWirelessDeviceAsync(endpoint, cancellationToken).ConfigureAwait(false);
            if (!verified.IsSuccess)
            {
                return;
            }
        }
    }

    private void Apply(WorkflowEvent workflowEvent, string message, string? deviceIdentity = null, Severity severity = Severity.Info)
    {
        bool applied = _stateMachine.TryApply(workflowEvent, message, deviceIdentity, severity);
        if (!applied)
        {
            _ = _logger.WarningAsync("InvalidStateTransition", $"Transition {workflowEvent} was rejected from {_stateMachine.CurrentState}.");
        }
    }

    private async Task FailAsync(AppError error, WorkflowEvent failureEvent, CancellationToken cancellationToken)
    {
        Apply(failureEvent, error.UserMessage, severity: error.Severity);
        await _logger.ErrorAsync(error.Code.ToString(), error.DiagnosticMessage, cancellationToken).ConfigureAwait(false);
        await NotifyAsync("Wireless Scrcpy error", error.UserMessage, error.Severity, cancellationToken).ConfigureAwait(false);
    }

    private async Task StopAsync(string message, CancellationToken cancellationToken)
    {
        Apply(WorkflowEvent.StopRequested, message, severity: Severity.Warning);
        await _logger.InfoAsync("WorkflowStopping", message, cancellationToken).ConfigureAwait(false);
        Apply(WorkflowEvent.Stopped, "Wireless Scrcpy session stopped.");
    }

    private Task NotifyAsync(string title, string body, Severity severity, CancellationToken cancellationToken) =>
        _notificationManager.NotifyAsync(new NotificationMessage(title, body, severity), cancellationToken);
}
