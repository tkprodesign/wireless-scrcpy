using WirelessScrcpy.Core.Common;
using WirelessScrcpy.Core.Devices;

namespace WirelessScrcpy.Core.Adb;

public sealed class AdbDeviceSelector
{
    public Result<AndroidDevice> SelectSingleUsbDevice(IEnumerable<AndroidDevice> devices)
    {
        AndroidDevice[] usbDevices = devices.Where(device => device.ConnectionType == DeviceConnectionType.Usb).ToArray();
        if (usbDevices.Length == 0) return Result<AndroidDevice>.Failure(new AppError(ErrorCode.NoUsbDevice, "No USB device was detected.", "adb devices returned no USB transports.", Severity.Error));
        if (usbDevices.Length > 1) return Result<AndroidDevice>.Failure(new AppError(ErrorCode.MultipleUsbDevices, "More than one USB device was detected.", "Multiple USB transports were returned by adb devices.", Severity.Error));
        AndroidDevice selected = usbDevices[0];
        return selected.State switch
        {
            DeviceState.Authorized => Result<AndroidDevice>.Success(selected),
            DeviceState.Unauthorized => Result<AndroidDevice>.Failure(new AppError(ErrorCode.UnauthorizedDevice, "Authorize USB debugging on the phone.", $"Device {selected.Serial} is unauthorized.", Severity.Error)),
            DeviceState.Offline => Result<AndroidDevice>.Failure(new AppError(ErrorCode.DeviceOffline, "The USB device is offline.", $"Device {selected.Serial} is offline.", Severity.Error)),
            _ => Result<AndroidDevice>.Failure(new AppError(ErrorCode.NoUsbDevice, "The USB device is not ready.", $"Device {selected.Serial} state is {selected.State}.", Severity.Error))
        };
    }
}
