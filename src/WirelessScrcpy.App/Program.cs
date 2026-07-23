using Microsoft.Extensions.DependencyInjection;
using WirelessScrcpy.App.Composition;
using WirelessScrcpy.App.UI;
using WirelessScrcpy.Core.Logging;

namespace WirelessScrcpy.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        ServiceProvider serviceProvider = new ApplicationBootstrapper().BuildServiceProvider();
        try
        {
            var retention = serviceProvider.GetRequiredService<LogRetentionPolicy>();
            retention.Apply(DateTimeOffset.UtcNow);
            var context = serviceProvider.GetRequiredService<WirelessScrcpyApplicationContext>();
            context.StartAsync().GetAwaiter().GetResult();
            Application.Run(context);
        }
        catch (Exception exception)
        {
            Logger logger = serviceProvider.GetRequiredService<Logger>();
            logger.ErrorAsync("FatalApplicationError", exception.ToString()).GetAwaiter().GetResult();
            MessageBox.Show("Wireless Scrcpy encountered a fatal startup error and will close.", "Wireless Scrcpy", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            serviceProvider.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }
}
