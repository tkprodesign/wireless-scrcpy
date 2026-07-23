using WirelessScrcpy.Core.Common;
using WirelessScrcpy.Core.Settings;

namespace WirelessScrcpy.Core.Diagnostics;

public sealed class ToolDiscoveryService
{
    private readonly ExecutablePathValidator _validator;
    private readonly EnvironmentPathScanner _pathScanner;

    public ToolDiscoveryService(ExecutablePathValidator validator, EnvironmentPathScanner pathScanner)
    {
        _validator = validator;
        _pathScanner = pathScanner;
    }

    public ToolDiscoveryResult Discover(ApplicationSettings settings)
    {
        ToolLocation? adb = DiscoverTool("adb.exe", settings.AdbPath);
        if (adb is null)
        {
            return new ToolDiscoveryResult(null, null, new AppError(ErrorCode.MissingAdb, "ADB could not be found.", "adb.exe was not found in settings, application-local paths, PATH, or common install locations.", Severity.Error));
        }

        ToolLocation? scrcpy = DiscoverTool("scrcpy.exe", settings.ScrcpyPath);
        if (scrcpy is null)
        {
            return new ToolDiscoveryResult(adb, null, new AppError(ErrorCode.MissingScrcpy, "scrcpy could not be found.", "scrcpy.exe was not found in settings, application-local paths, PATH, or common install locations.", Severity.Error));
        }

        return new ToolDiscoveryResult(adb, scrcpy, null);
    }

    private ToolLocation? DiscoverTool(string executableName, string? persistedPath)
    {
        if (_validator.IsValidExecutable(persistedPath, executableName)) return new ToolLocation(persistedPath!, "Settings");
        string baseDirectory = AppContext.BaseDirectory;
        string local = Path.Combine(baseDirectory, executableName);
        if (_validator.IsValidExecutable(local, executableName)) return new ToolLocation(local, "ApplicationDirectory");
        string tools = Path.Combine(baseDirectory, "tools", executableName);
        if (_validator.IsValidExecutable(tools, executableName)) return new ToolLocation(tools, "ApplicationToolsDirectory");
        string? path = _pathScanner.FindExecutable(executableName);
        if (_validator.IsValidExecutable(path, executableName)) return new ToolLocation(path!, "Path");
        foreach (string candidate in CommonLocations(executableName))
        {
            if (_validator.IsValidExecutable(candidate, executableName)) return new ToolLocation(candidate, "CommonLocation");
        }
        return null;
    }

    private static IEnumerable<string> CommonLocations(string executableName)
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        yield return Path.Combine(localAppData, "Android", "Sdk", "platform-tools", executableName);
        yield return Path.Combine(programFiles, "scrcpy", executableName);
        yield return Path.Combine(programFilesX86, "scrcpy", executableName);
    }
}
