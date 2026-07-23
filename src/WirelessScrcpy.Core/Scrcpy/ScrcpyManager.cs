using WirelessScrcpy.Core.Common;

namespace WirelessScrcpy.Core.Scrcpy;

public sealed class ScrcpyManager
{
    private readonly ScrcpyLauncher _launcher;

    public ScrcpyManager(ScrcpyLauncher launcher) => _launcher = launcher;

    public void ConfigurePath(string scrcpyPath) => _launcher.ConfigurePath(scrcpyPath);

    public Result<ScrcpySession> Launch(string serialOrEndpoint)
    {
        try
        {
            return Result<ScrcpySession>.Success(_launcher.Launch(serialOrEndpoint));
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception or FileNotFoundException)
        {
            return Result<ScrcpySession>.Failure(new AppError(ErrorCode.ScrcpyLaunchFailed, "scrcpy could not be launched.", exception.ToString(), Severity.Error));
        }
    }
}
