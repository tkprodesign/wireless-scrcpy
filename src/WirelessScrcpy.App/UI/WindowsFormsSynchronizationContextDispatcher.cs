namespace WirelessScrcpy.App.UI;

public sealed class WindowsFormsSynchronizationContextDispatcher
{
    private readonly SynchronizationContext _context;
    public WindowsFormsSynchronizationContextDispatcher() => _context = SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext();
    public void Post(Action action) => _context.Post(_ => action(), null);
}
