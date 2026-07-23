using System.Diagnostics;
using System.Text;
using WirelessScrcpy.Core.Abstractions;

namespace WirelessScrcpy.Core.Diagnostics;

public sealed class ProcessRunner : IProcessRunner
{
    public async Task<ProcessRunResult> RunAsync(ProcessStartRequest request, CancellationToken cancellationToken = default)
    {
        using var process = CreateProcess(request);
        var output = new StringBuilder();
        var error = new StringBuilder();
        object outputGate = new();
        object errorGate = new();
        var stopwatch = Stopwatch.StartNew();
        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null)
            {
                lock (outputGate)
                {
                    output.AppendLine(e.Data);
                }
            }
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null)
            {
                lock (errorGate)
                {
                    error.AppendLine(e.Data);
                }
            }
        };
        process.Start();
        if (request.CaptureOutput)
        {
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }

        bool timedOut = false;
        using CancellationTokenSource? timeoutCts = request.Timeout is null ? null : new CancellationTokenSource(request.Timeout.Value);
        using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts?.Token ?? CancellationToken.None);
        try
        {
            await process.WaitForExitAsync(linked.Token).ConfigureAwait(false);
            if (request.CaptureOutput)
            {
                process.WaitForExit();
            }
        }
        catch (OperationCanceledException) when (timeoutCts?.IsCancellationRequested == true && !cancellationToken.IsCancellationRequested)
        {
            timedOut = true;
            Kill(process);
            await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
            if (request.CaptureOutput)
            {
                process.WaitForExit();
            }
        }
        catch (OperationCanceledException)
        {
            Kill(process);
            throw;
        }

        stopwatch.Stop();
        string standardOutput;
        string standardError;
        lock (outputGate)
        {
            standardOutput = output.ToString();
        }
        lock (errorGate)
        {
            standardError = error.ToString();
        }
        return new ProcessRunResult(process.ExitCode, standardOutput, standardError, timedOut, stopwatch.Elapsed);
    }

    public ProcessHandle Start(ProcessStartRequest request)
    {
        var process = CreateProcess(request);
        process.Start();
        return new ProcessHandle(process);
    }

    private static Process CreateProcess(ProcessStartRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.FileName);
        var startInfo = new ProcessStartInfo
        {
            FileName = request.FileName,
            UseShellExecute = false,
            RedirectStandardOutput = request.CaptureOutput,
            RedirectStandardError = request.CaptureOutput,
            CreateNoWindow = true
        };
        if (!string.IsNullOrWhiteSpace(request.WorkingDirectory))
        {
            startInfo.WorkingDirectory = request.WorkingDirectory;
        }
        foreach (string argument in request.Arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }
        return new Process { StartInfo = startInfo, EnableRaisingEvents = true };
    }

    private static void Kill(Process process)
    {
        if (!process.HasExited)
        {
            process.Kill(entireProcessTree: true);
        }
    }
}
