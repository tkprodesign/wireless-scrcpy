namespace WirelessScrcpy.Core.Diagnostics;

public sealed record ProcessRunResult(int ExitCode, string StandardOutput, string StandardError, bool TimedOut, TimeSpan Duration);
