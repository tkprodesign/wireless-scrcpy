namespace WirelessScrcpy.Core.Settings;

public sealed class SettingsManager : IDisposable
{
    private readonly ISettingsStore _store;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private ApplicationSettings _current = ApplicationSettings.Default;

    public SettingsManager(ISettingsStore store) => _store = store;

    public ApplicationSettings Current => _current;

    public async Task<ApplicationSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _current = await _store.LoadAsync(cancellationToken).ConfigureAwait(false);
            return _current;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveAsync(ApplicationSettings settings, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await _store.SaveAsync(settings, cancellationToken).ConfigureAwait(false);
            _current = settings;
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose() => _gate.Dispose();
}
