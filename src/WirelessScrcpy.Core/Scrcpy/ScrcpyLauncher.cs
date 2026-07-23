using WirelessScrcpy.Core.Abstractions;
using WirelessScrcpy.Core.Diagnostics;

namespace WirelessScrcpy.Core.Scrcpy;

public sealed class ScrcpyLauncher
{
    private readonly IProcessRunner _runner;
    private readonly ScrcpyCommandBuilder _commandBuilder;

    public ScrcpyLauncher(IProcessRunner runner, ScrcpyCommandBuilder commandBuilder)
    {
        _runner = runner;
        _commandBuilder = commandBuilder;
    }

    public string ScrcpyPath { get; private set; } = "scrcpy.exe";
    public void ConfigurePath(string scrcpyPath) => ScrcpyPath = scrcpyPath;
    public ScrcpySession Launch(string serialOrEndpoint) => new(_runner.Start(new ProcessStartRequest(ScrcpyPath, _commandBuilder.Build(serialOrEndpoint), CaptureOutput: false)));
}
