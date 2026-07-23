namespace WirelessScrcpy.Core.Settings;

public sealed class SettingsManager
{
    private readonly ISettingsStore _store;
    private ApplicationSettings _current = ApplicationSettings.Default;

    public SettingsManager(ISettingsStore store) => _store = store;

    public ApplicationSettings Current => _current;

    public async Task<ApplicationSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        _current = await _store.LoadAsync(cancellationToken).ConfigureAwait(false);
        return _current;
    }

    public async Task SaveAsync(ApplicationSettings settings, CancellationToken cancellationToken = default)
    {
        await _store.SaveAsync(settings, cancellationToken).ConfigureAwait(false);
        _current = settings;
    }
}
