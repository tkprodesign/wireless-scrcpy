using Microsoft.Extensions.DependencyInjection;
using WirelessScrcpy.App.UI;
using WirelessScrcpy.App.UI.Notifications;
using WirelessScrcpy.App.UI.Tray;
using WirelessScrcpy.Core.Abstractions;
using WirelessScrcpy.Core.Adb;
using WirelessScrcpy.Core.Common;
using WirelessScrcpy.Core.Devices;
using WirelessScrcpy.Core.Diagnostics;
using WirelessScrcpy.Core.Logging;
using WirelessScrcpy.Core.Notifications;
using WirelessScrcpy.Core.Scrcpy;
using WirelessScrcpy.Core.Settings;
using WirelessScrcpy.Core.Workflow;

namespace WirelessScrcpy.App.Composition;

public sealed class ApplicationBootstrapper
{
    public ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<WorkflowOptions>(_ => WorkflowOptions.Default);
        services.AddSingleton<SettingsFileLocator>();
        services.AddSingleton<SettingsSerializer>();
        services.AddSingleton<ISettingsStore, JsonSettingsStore>();
        services.AddSingleton<SettingsManager>();
        services.AddSingleton<LogFileLocator>();
        services.AddSingleton<LogRetentionPolicy>();
        services.AddSingleton<ISessionLogger, FileSessionLogger>();
        services.AddSingleton<Logger>();
        services.AddSingleton<IProcessRunner, ProcessRunner>();
        services.AddSingleton<ExecutablePathValidator>();
        services.AddSingleton<EnvironmentPathScanner>();
        services.AddSingleton<ToolDiscoveryService>();
        services.AddSingleton<ExceptionHandler>();
        services.AddSingleton<AdbCommandBuilder>();
        services.AddSingleton<AdbOutputParser>();
        services.AddSingleton<AdbDeviceSelector>();
        services.AddSingleton<AdbClient>();
        services.AddSingleton<AdbServerManager>();
        services.AddSingleton<AdbTcpIpService>();
        services.AddSingleton<AdbWirelessConnector>();
        services.AddSingleton<AdbManager>();
        services.AddSingleton<IpAddressCandidateSelector>();
        services.AddSingleton<PhoneIpDiscoveryService>();
        services.AddSingleton<DeviceManager>();
        services.AddSingleton<NetworkManager>();
        services.AddSingleton<ScrcpyCommandBuilder>();
        services.AddSingleton<ScrcpyLauncher>();
        services.AddSingleton<ScrcpyManager>();
        services.AddSingleton<WorkflowStateMachine>();
        services.AddSingleton<ReconnectSupervisor>();
        services.AddSingleton<WirelessScrcpyWorkflow>();
        services.AddSingleton<WirelessScrcpyController>();
        services.AddSingleton<WindowsFormsSynchronizationContextDispatcher>();
        services.AddSingleton<StatusViewModel>();
        services.AddSingleton<StatusForm>();
        services.AddSingleton<TrayMenuFactory>();
        services.AddSingleton<IUsbDisconnectPrompt, UsbDisconnectPrompt>();
        services.AddSingleton<IUserNotifier, TrayBalloonNotifier>();
        services.AddSingleton<NotificationManager>();
        services.AddSingleton<ApplicationLifetime>();
        services.AddSingleton<WirelessScrcpyApplicationContext>();
        return services.BuildServiceProvider(validateScopes: true);
    }
}
