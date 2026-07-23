namespace WirelessScrcpy.Core.Diagnostics;

public sealed class EnvironmentPathScanner
{
    public string? FindExecutable(string executableName)
    {
        string? pathVariable = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathVariable)) return null;
        foreach (string directory in pathVariable.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string candidate = Path.Combine(directory, executableName);
            if (File.Exists(candidate)) return candidate;
        }
        return null;
    }
}
