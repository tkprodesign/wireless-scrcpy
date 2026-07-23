using System.Text;
using WirelessScrcpy.Core.Abstractions;
using WirelessScrcpy.Core.Common;

namespace WirelessScrcpy.Core.Logging;

public sealed class FileSessionLogger : ISessionLogger
{
    private readonly IClock _clock;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly StreamWriter _writer;

    public FileSessionLogger(LogFileLocator locator, IClock clock)
    {
        _clock = clock;
        Directory.CreateDirectory(locator.GetLogDirectory());
        _writer = new StreamWriter(new FileStream(locator.CreateSessionLogPath(clock.UtcNow), FileMode.CreateNew, FileAccess.Write, FileShare.Read), Encoding.UTF8)
        {
            AutoFlush = true
        };
    }

    public async Task WriteAsync(Severity level, string eventName, string message, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
        string line = $"{_clock.UtcNow:O}\t{level}\t{eventName}\t{message.ReplaceLineEndings(" ")}";
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await _writer.WriteLineAsync(line.AsMemory(), cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _writer.DisposeAsync().ConfigureAwait(false);
        _gate.Dispose();
    }
}
