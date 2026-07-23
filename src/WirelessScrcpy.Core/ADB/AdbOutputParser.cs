using WirelessScrcpy.Core.Devices;

namespace WirelessScrcpy.Core.Adb;

public sealed class AdbOutputParser
{
    public IReadOnlyList<AndroidDevice> ParseDevices(string output)
    {
        var devices = new List<AndroidDevice>();
        foreach (string rawLine in output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (rawLine.StartsWith("List of devices", StringComparison.OrdinalIgnoreCase)) continue;
            string[] parts = rawLine.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length < 2) continue;
            string serial = parts[0];
            DeviceState state = parts[1] switch
            {
                "device" => DeviceState.Authorized,
                "unauthorized" => DeviceState.Unauthorized,
                "offline" => DeviceState.Offline,
                _ => DeviceState.Unknown
            };
            bool wireless = serial.Contains(':', StringComparison.Ordinal);
            string? ip = wireless ? serial.Split(':', 2)[0] : null;
            devices.Add(new AndroidDevice(serial, wireless ? DeviceConnectionType.Wireless : DeviceConnectionType.Usb, state, ip));
        }
        return devices;
    }

    public bool IsTcpIpEnabled(AdbCommandResult result) =>
        result.ExitCode == 0 &&
        (result.StandardOutput.Contains("restarting in TCP mode", StringComparison.OrdinalIgnoreCase) ||
         result.StandardOutput.Contains("restarting in tcp mode", StringComparison.OrdinalIgnoreCase));

    public bool IsConnected(AdbCommandResult result) =>
        result.ExitCode == 0 &&
        (result.StandardOutput.Contains("connected", StringComparison.OrdinalIgnoreCase) ||
         result.StandardOutput.Contains("already connected", StringComparison.OrdinalIgnoreCase));
}
