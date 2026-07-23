using System.Net;
using System.Net.Sockets;

namespace WirelessScrcpy.Core.Devices;

public sealed class IpAddressCandidateSelector
{
    public string? Select(IEnumerable<string> candidates)
    {
        foreach (string candidate in candidates)
        {
            if (!IPAddress.TryParse(candidate, out IPAddress? address) || address.AddressFamily != AddressFamily.InterNetwork) continue;
            byte[] bytes = address.GetAddressBytes();
            bool privateAddress = bytes[0] == 10 || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) || (bytes[0] == 192 && bytes[1] == 168);
            if (privateAddress) return candidate;
        }
        return null;
    }
}
