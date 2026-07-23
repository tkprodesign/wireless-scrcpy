namespace WirelessScrcpy.Core.Diagnostics;

public sealed class ExecutablePathValidator
{
    public bool IsValidExecutable(string? path, string expectedFileName)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        return File.Exists(path) && string.Equals(Path.GetFileName(path), expectedFileName, StringComparison.OrdinalIgnoreCase);
    }
}
