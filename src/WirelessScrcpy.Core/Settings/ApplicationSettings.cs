namespace WirelessScrcpy.Core.Settings;

public sealed record ApplicationSettings(string? AdbPath, string? ScrcpyPath, bool ShowStatusWindowOnStart)
{
    public static ApplicationSettings Default { get; } = new(null, null, true);
}
