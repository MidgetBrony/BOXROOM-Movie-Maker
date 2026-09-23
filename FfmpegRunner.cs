using System.Diagnostics;

namespace BoxroomMovieMaker;

public sealed class FfmpegRunner
{
    private Process? _currentProcess;

    public async Task<ConversionResult> ConvertAsync(
        string ffmpegPath,
        string inputPath,
        string outputPath,
        Action<string> onOutput,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        // This is the BOXROOM developer-provided compatibility command.
        startInfo.ArgumentList.Add("-i");
        startInfo.ArgumentList.Add(inputPath);
        startInfo.ArgumentList.Add("-c:v");
        startInfo.ArgumentList.Add("libx264");
        startInfo.ArgumentList.Add("-pix_fmt");
        startInfo.ArgumentList.Add("yuv420p");
        startInfo.ArgumentList.Add("-c:a");
        startInfo.ArgumentList.Add("aac");
        startInfo.ArgumentList.Add("-movflags");
        startInfo.ArgumentList.Add("+faststart");
        startInfo.ArgumentList.Add(outputPath);

        using var process = new Process { StartInfo = startInfo };
        _currentProcess = process;

        try
        {
            process.Start();
            using var registration = cancellationToken.Register(() =>
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill(entireProcessTree: true);
                    }
                }
                catch
                {
                    // The process may have exited between the check and kill request.
                }
            });

            var stdoutTask = DrainAsync(process.StandardOutput, _ => { });
            var stderrTask = DrainAsync(process.StandardError, onOutput);
            await process.WaitForExitAsync(cancellationToken);
            await Task.WhenAll(stdoutTask, stderrTask);

            return process.ExitCode == 0
                ? ConversionResult.Success()
                : ConversionResult.Failure($"FFmpeg exited with code {process.ExitCode}.");
        }
        catch (OperationCanceledException)
        {
            return ConversionResult.Cancelled();
        }
        catch (Exception ex)
        {
            return ConversionResult.Failure(ex.Message);
        }
        finally
        {
            _currentProcess = null;
        }
    }

    public void Cancel()
    {
        try
        {
            if (_currentProcess is { HasExited: false })
            {
                _currentProcess.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // Cancellation is best effort; the token handles the final state.
        }
    }

    private static async Task DrainAsync(StreamReader reader, Action<string> onLine)
    {
        while (await reader.ReadLineAsync() is { } line)
        {
            onLine(line);
        }
    }
}

public sealed record ConversionResult(bool Succeeded, bool WasCancelled, string Error)
{
    public static ConversionResult Success() => new(true, false, string.Empty);
    public static ConversionResult Failure(string error) => new(false, false, error);
    public static ConversionResult Cancelled() => new(false, true, "Cancelled");
}
