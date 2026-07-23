using System.Text;

namespace WirelessScrcpy.Core.Settings;

public sealed class JsonSettingsStore : ISettingsStore
{
    private readonly SettingsFileLocator _locator;
    private readonly SettingsSerializer _serializer;

    public JsonSettingsStore(SettingsFileLocator locator, SettingsSerializer serializer)
    {
        _locator = locator;
        _serializer = serializer;
    }

    public async Task<ApplicationSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        string path = _locator.GetSettingsFilePath();
        if (!File.Exists(path))
        {
            return ApplicationSettings.Default;
        }

        try
        {
            string json = await File.ReadAllTextAsync(path, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
            return _serializer.Deserialize(json);
        }
        catch (IOException)
        {
            return ApplicationSettings.Default;
        }
        catch (System.Text.Json.JsonException)
        {
            return ApplicationSettings.Default;
        }
        catch (UnauthorizedAccessException)
        {
            return ApplicationSettings.Default;
        }
    }

    public async Task SaveAsync(ApplicationSettings settings, CancellationToken cancellationToken = default)
    {
        string directory = _locator.GetSettingsDirectory();
        Directory.CreateDirectory(directory);
        string path = _locator.GetSettingsFilePath();
        string tempPath = Path.Combine(directory, $"{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
        string json = _serializer.Serialize(settings);
        await File.WriteAllTextAsync(tempPath, json, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
        File.Move(tempPath, path, true);
    }
}
