using System.Net;
using WirelessScrcpy.Core.Common;

namespace WirelessScrcpy.Core.Devices;

public sealed class NetworkManager
{
    public Result<NetworkEndpoint> CreateEndpoint(string ipAddress, int port)
    {
        if (!IPAddress.TryParse(ipAddress, out IPAddress? parsed))
        {
            return Result<NetworkEndpoint>.Failure(new AppError(ErrorCode.PhoneIpDiscoveryFailed, "The phone IP address is invalid.", ipAddress, Severity.Error));
        }

        if (port is <= 0 or > 65535)
        {
            return Result<NetworkEndpoint>.Failure(new AppError(ErrorCode.WirelessConnectionFailed, "The wireless ADB port is invalid.", port.ToString(System.Globalization.CultureInfo.InvariantCulture), Severity.Error));
        }

        return Result<NetworkEndpoint>.Success(new NetworkEndpoint(parsed.ToString(), port));
    }
}
