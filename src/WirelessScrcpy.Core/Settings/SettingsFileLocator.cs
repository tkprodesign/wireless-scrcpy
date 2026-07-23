namespace WirelessScrcpy.Core.Settings;

public sealed class SettingsFileLocator
{
    private const string CompanyFolder = "WirelessScrcpy";
    private const string FileName = "settings.json";

    public string GetSettingsDirectory()
    {
        string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(root, CompanyFolder);
    }

    public string GetSettingsFilePath() => Path.Combine(GetSettingsDirectory(), FileName);
}
