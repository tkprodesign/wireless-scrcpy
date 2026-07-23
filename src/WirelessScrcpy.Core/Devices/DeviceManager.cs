using WirelessScrcpy.Core.Adb;
using WirelessScrcpy.Core.Common;

namespace WirelessScrcpy.Core.Devices;

public sealed class DeviceManager
{
    private readonly AdbManager _adbManager;
    private readonly PhoneIpDiscoveryService _ipDiscovery;

    public DeviceManager(AdbManager adbManager, PhoneIpDiscoveryService ipDiscovery)
    {
        _adbManager = adbManager;
        _ipDiscovery = ipDiscovery;
    }

    public Task<Result<AndroidDevice>> DetectSingleUsbDeviceAsync(CancellationToken cancellationToken = default) => _adbManager.DetectSingleUsbDeviceAsync(cancellationToken);
    public Task<Result<string>> DiscoverPhoneIpAsync(string serial, CancellationToken cancellationToken = default) => _ipDiscovery.DiscoverAsync(serial, cancellationToken);
    public Task<Result<AndroidDevice>> VerifyWirelessDeviceAsync(NetworkEndpoint endpoint, CancellationToken cancellationToken = default) => _adbManager.VerifyWirelessDeviceAsync(endpoint, cancellationToken);
}
