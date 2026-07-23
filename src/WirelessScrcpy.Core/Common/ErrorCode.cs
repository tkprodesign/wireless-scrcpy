namespace WirelessScrcpy.Core.Common;

public enum ErrorCode
{
    None,
    MissingAdb,
    MissingScrcpy,
    NoUsbDevice,
    MultipleUsbDevices,
    UnauthorizedDevice,
    DeviceOffline,
    TcpIpEnablementFailed,
    PhoneIpDiscoveryFailed,
    WirelessConnectionFailed,
    ScrcpyLaunchFailed,
    ProcessStartFailed,
    ProcessTimedOut,
    SettingsReadFailed,
    SettingsWriteFailed,
    LoggingFailed,
    InvalidStateTransition,
    OperationCancelled,
    UnexpectedError
}
