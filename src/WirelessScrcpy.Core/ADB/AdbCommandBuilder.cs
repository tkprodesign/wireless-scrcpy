using WirelessScrcpy.Core.Devices;

namespace WirelessScrcpy.Core.Adb;

public sealed class AdbCommandBuilder
{
    public IReadOnlyList<string> DevicesLong() => ["devices", "-l"];
    public IReadOnlyList<string> StartServer() => ["start-server"];
    public IReadOnlyList<string> TcpIp(string serial, int port) => ["-s", serial, "tcpip", port.ToString(System.Globalization.CultureInfo.InvariantCulture)];
    public IReadOnlyList<string> Connect(NetworkEndpoint endpoint) => ["connect", endpoint.ToString()];
    public IReadOnlyList<string> Disconnect(string serial) => ["disconnect", serial];
    public IReadOnlyList<string> Shell(string serial, params string[] command) => ["-s", serial, "shell", .. command];
}
