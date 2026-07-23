namespace WirelessScrcpy.Core.Logging;

public sealed class LogRetentionPolicy
{
    private static readonly TimeSpan Retention = TimeSpan.FromDays(14);
    private readonly LogFileLocator _locator;

    public LogRetentionPolicy(LogFileLocator locator) => _locator = locator;

    public void Apply(DateTimeOffset nowUtc)
    {
        string directory = _locator.GetLogDirectory();
        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (string path in Directory.EnumerateFiles(directory, "session-*.log"))
        {
            var info = new FileInfo(path);
            if (nowUtc - info.LastWriteTimeUtc > Retention)
            {
                info.Delete();
            }
        }
    }
}
