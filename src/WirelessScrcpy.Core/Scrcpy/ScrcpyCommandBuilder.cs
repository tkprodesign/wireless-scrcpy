namespace WirelessScrcpy.Core.Scrcpy;

public sealed class ScrcpyCommandBuilder
{
    public IReadOnlyList<string> Build(string serialOrEndpoint) => ["--serial", serialOrEndpoint, "--no-audio"];
}
