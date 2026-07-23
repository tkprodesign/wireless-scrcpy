namespace WirelessScrcpy.Core.Logging;

public sealed class LogFileLocator
{
    private const string CompanyFolder = "WirelessScrcpy";
    private const string LogFolder = "Logs";

    public string GetLogDirectory()
    {
        string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(root, CompanyFolder, LogFolder);
    }

    public string CreateSessionLogPath(DateTimeOffset timestampUtc)
    {
        string fileName = $"session-{timestampUtc:yyyyMMdd-HHmmss}.log";
        return Path.Combine(GetLogDirectory(), fileName);
    }
}
