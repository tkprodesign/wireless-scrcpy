using System.Text.Json;

namespace WirelessScrcpy.Core.Settings;

public sealed class SettingsSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.General)
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public string Serialize(ApplicationSettings settings) => JsonSerializer.Serialize(settings, Options);

    public ApplicationSettings Deserialize(string json) =>
        JsonSerializer.Deserialize<ApplicationSettings>(json, Options) ?? ApplicationSettings.Default;
}
