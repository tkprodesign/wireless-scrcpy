namespace WirelessScrcpy.Core.Workflow;

public sealed class SessionHandle : IAsyncDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly List<IAsyncDisposable> _resources = [];

    public CancellationToken CancellationToken => _cancellationTokenSource.Token;
    public void AddResource(IAsyncDisposable resource) => _resources.Add(resource);
    public void Cancel() => _cancellationTokenSource.Cancel();

    public async ValueTask DisposeAsync()
    {
        Cancel();
        foreach (IAsyncDisposable resource in _resources.AsEnumerable().Reverse())
        {
            await resource.DisposeAsync().ConfigureAwait(false);
        }
        _cancellationTokenSource.Dispose();
    }
}
