namespace WirelessScrcpy.Core.Adb;

public sealed record AdbCommandResult(int ExitCode, string StandardOutput, string StandardError, TimeSpan Duration);
