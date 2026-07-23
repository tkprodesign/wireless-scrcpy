namespace WirelessScrcpy.Core.Workflow;

public enum WorkflowEvent
{
    LaunchRequested,
    WorkflowInitialized,
    AdbFound,
    AdbMissing,
    ScrcpyFound,
    ScrcpyMissing,
    AdbServerReady,
    SingleUsbDeviceFound,
    NoUsbDeviceFound,
    MultipleUsbDevicesFound,
    UsbDeviceUnauthorized,
    TcpIpEnabled,
    TcpIpFailed,
    PhoneIpDiscovered,
    PhoneIpMissing,
    UserConfirmedUsbDisconnected,
    UserCancelled,
    WirelessAdbConnected,
    WirelessAdbFailed,
    ScrcpyStarted,
    ScrcpyLaunchFailed,
    NetworkInterrupted,
    ReconnectSucceeded,
    ReconnectExpired,
    StopRequested,
    Stopped
}
