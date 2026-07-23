namespace WirelessScrcpy.Core.Devices;

public sealed record AndroidDevice(string Serial, DeviceConnectionType ConnectionType, DeviceState State, string? IpAddress);
