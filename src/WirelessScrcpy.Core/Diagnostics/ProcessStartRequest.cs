namespace WirelessScrcpy.Core.Diagnostics;

public sealed record ProcessStartRequest(
    string FileName,
    IReadOnlyList<string> Arguments,
    string? WorkingDirectory = null,
    TimeSpan? Timeout = null,
    bool CaptureOutput = true);
