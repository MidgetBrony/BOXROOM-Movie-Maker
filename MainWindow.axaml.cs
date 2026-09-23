using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace BoxroomMovieMaker;

public sealed partial class MainWindow : Window
{
    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".mkv", ".mov", ".avi", ".webm", ".m4v", ".mpg", ".mpeg",
        ".wmv", ".flv", ".ts", ".mts", ".m2ts", ".ogv"
    };

    private readonly ObservableCollection<VideoJob> _jobs = [];
    private readonly FfmpegRunner _runner = new();
    private CancellationTokenSource? _cancellation;
    private string? _ffmpegPath;
    private bool _isConverting;

    public MainWindow()
    {
        InitializeComponent();
        Closing += (_, _) =>
        {
            _cancellation?.Cancel();
            _runner.Cancel();
        };
        DragDrop.SetAllowDrop(DropZone, true);
        DropZone.AddHandler(DragDrop.DragOverEvent, DropZone_OnDragOver);
        DropZone.AddHandler(DragDrop.DropEvent, DropZone_OnDrop);
        QueueList.ItemsSource = _jobs;
        _ffmpegPath = FindFfmpeg();
        UpdateFfmpegStatus();
        UpdateQueueUi();
    }

    private async void ChooseVideos_OnClick(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choose videos for BOXROOM",
            AllowMultiple = true,
            FileTypeFilter =
            [
                new FilePickerFileType("Video files")
                {
                    Patterns = VideoExtensions.Select(extension => $"*{extension}").ToArray()
                },
                FilePickerFileTypes.All
            ]
        });

        AddPaths(files.Select(file => file.Path.LocalPath));
    }

    private async void ChooseFolder_OnClick(object? sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Choose a folder of videos",
            AllowMultiple = true
        });

        AddPaths(folders.Select(folder => folder.Path.LocalPath));
    }

    private async void ChooseFfmpeg_OnClick(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choose the FFmpeg executable",
            AllowMultiple = false,
            FileTypeFilter = [FilePickerFileTypes.All]
        });

        var path = files.FirstOrDefault()?.Path.LocalPath;
        if (!string.IsNullOrWhiteSpace(path))
        {
            _ffmpegPath = path;
            UpdateFfmpegStatus();
        }
    }

    private async void About_OnClick(object? sender, RoutedEventArgs e)
    {
        await new AboutWindow().ShowDialog(this);
    }

    private void DropZone_OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.DataTransfer.Contains(DataFormat.File)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
    }

    private void DropZone_OnDrop(object? sender, DragEventArgs e)
    {
        var files = e.DataTransfer.TryGetFiles();
        if (files is not null)
        {
            AddPaths(files.Select(file => file.Path.LocalPath));
        }
    }

    private void AddPaths(IEnumerable<string> paths)
    {
        if (_isConverting)
        {
            SetStatus("Wait for the current conversion to finish before adding more movies.");
            return;
        }

        var candidates = new List<string>();
        foreach (var path in paths)
        {
            try
            {
                if (File.Exists(path) && IsVideo(path))
                {
                    candidates.Add(Path.GetFullPath(path));
                }
                else if (Directory.Exists(path))
                {
                    candidates.AddRange(Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
                        .Where(IsVideo)
                        .Select(Path.GetFullPath));
                }
            }
            catch (Exception ex)
            {
                AppendLog($"Could not read {path}: {ex.Message}");
            }
        }

        var existing = _jobs.Select(job => job.InputPath).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var added = 0;
        foreach (var path in candidates.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(path => path))
        {
            if (!existing.Add(path))
            {
                continue;
            }

            _jobs.Add(new VideoJob
            {
                InputPath = path,
                DisplayName = Path.GetFileName(path)
            });
            added++;
        }

        UpdateQueueUi();
        SetStatus(added == 0 ? "No new supported videos were found." : $"Added {added} video{(added == 1 ? "" : "s")}.");
    }

    private async void Convert_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_isConverting || _jobs.Count == 0)
        {
            SetStatus(_jobs.Count == 0 ? "Add at least one video first." : "Conversion is already running.");
            return;
        }

        _ffmpegPath ??= FindFfmpeg();
        if (string.IsNullOrWhiteSpace(_ffmpegPath))
        {
            SetStatus("FFmpeg was not found. Install it or use Choose FFmpeg.");
            return;
        }

        if (!await CanStartFfmpegAsync(_ffmpegPath))
        {
            SetStatus("The selected FFmpeg executable could not be started.");
            return;
        }

        _isConverting = true;
        _cancellation = new CancellationTokenSource();
        SetBusyUi(true);
        LogTextBox.Text = string.Empty;

        var completed = 0;
        var failed = 0;
        try
        {
            for (var index = 0; index < _jobs.Count; index++)
            {
                if (_cancellation.IsCancellationRequested)
                {
                    break;
                }

                var job = _jobs[index];
                job.OutputPath = MakeUniqueOutputPath(job.InputPath);
                job.Status = $"Converting {index + 1}/{_jobs.Count}";
                job.StatusColor = "#70D6B2";
                QueueSummaryText.Text = $"Converting {index + 1} of {_jobs.Count}: {job.DisplayName}";
                AppendLog($"> {job.DisplayName}");

                var result = await _runner.ConvertAsync(
                    _ffmpegPath,
                    job.InputPath,
                    job.OutputPath,
                    line => Dispatcher.UIThread.Post(() => AppendUsefulFfmpegLine(line)),
                    _cancellation.Token);

                if (result.Succeeded)
                {
                    completed++;
                    job.Status = "Ready for BOXROOM";
                    job.StatusColor = "#70D6B2";
                    AppendLog($"Saved: {job.OutputPath}");
                }
                else if (result.WasCancelled)
                {
                    job.Status = "Cancelled";
                    job.StatusColor = "#E2B8FF";
                    DeletePartialOutput(job.OutputPath);
                    break;
                }
                else
                {
                    failed++;
                    job.Status = "Failed";
                    job.StatusColor = "#F08A96";
                    DeletePartialOutput(job.OutputPath);
                    AppendLog($"Failed: {result.Error}");
                }
            }
        }
        finally
        {
            _isConverting = false;
            _cancellation.Dispose();
            _cancellation = null;
            SetBusyUi(false);
            QueueSummaryText.Text = $"{completed} ready, {failed} failed, {_jobs.Count - completed - failed} not converted.";
            SetStatus(completed > 0
                ? $"Finished. {completed} BOXROOM-ready movie{(completed == 1 ? "" : "s")} created."
                : "No movies were converted.");
        }
    }

    private void Cancel_OnClick(object? sender, RoutedEventArgs e)
    {
        _cancellation?.Cancel();
        _runner.Cancel();
        SetStatus("Cancelling after the current FFmpeg process stops…");
    }

    private void ClearQueue_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_isConverting)
        {
            SetStatus("Cancel the current conversion before clearing the queue.");
            return;
        }

        _jobs.Clear();
        LogTextBox.Text = string.Empty;
        UpdateQueueUi();
        SetStatus("Queue cleared.");
    }

    private static bool IsVideo(string path) => VideoExtensions.Contains(Path.GetExtension(path));

    private static string MakeUniqueOutputPath(string inputPath)
    {
        var folder = Path.GetDirectoryName(inputPath) ?? Environment.CurrentDirectory;
        var stem = Path.GetFileNameWithoutExtension(inputPath);
        var path = Path.Combine(folder, $"{stem}_BOXROOM.mp4");
        var suffix = 2;
        while (File.Exists(path) || string.Equals(path, inputPath, StringComparison.OrdinalIgnoreCase))
        {
            path = Path.Combine(folder, $"{stem}_BOXROOM_{suffix++}.mp4");
        }

        return path;
    }

    private static void DeletePartialOutput(string path)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // A failed conversion should still report its original FFmpeg error.
        }
    }

    private static string? FindFfmpeg()
    {
        var fileName = OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";
        var besideApp = Path.Combine(AppContext.BaseDirectory, fileName);
        if (File.Exists(besideApp))
        {
            return besideApp;
        }

        var pathValue = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var folder in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                var candidate = Path.Combine(folder.Trim(), fileName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
            catch
            {
                // Ignore malformed PATH entries and keep searching.
            }
        }

        return null;
    }

    private static async Task<bool> CanStartFfmpegAsync(string path)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = path,
                Arguments = "-version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });
            if (process is null)
            {
                return false;
            }

            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private void UpdateFfmpegStatus()
    {
        if (string.IsNullOrWhiteSpace(_ffmpegPath))
        {
            FfmpegStatusText.Text = "Not found. Install FFmpeg, place it beside the app, or choose it below.";
            FfmpegStatusText.Foreground = Avalonia.Media.Brushes.IndianRed;
        }
        else
        {
            FfmpegStatusText.Text = $"Found: {_ffmpegPath}";
            FfmpegStatusText.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#A9EBD4"));
        }
    }

    private void UpdateQueueUi()
    {
        EmptyQueueText.IsVisible = _jobs.Count == 0;
        QueueSummaryText.Text = _jobs.Count == 0
            ? "Drop movies here to begin."
            : $"{_jobs.Count} movie{(_jobs.Count == 1 ? "" : "s")} ready to convert.";
        ConvertButton.IsEnabled = _jobs.Count > 0 && !_isConverting;
    }

    private void SetBusyUi(bool busy)
    {
        BusyProgress.IsVisible = busy;
        CancelButton.IsVisible = busy;
        ConvertButton.IsVisible = !busy;
    }

    private void AppendUsefulFfmpegLine(string line)
    {
        if (line.Contains("time=", StringComparison.OrdinalIgnoreCase) ||
            line.Contains("error", StringComparison.OrdinalIgnoreCase) ||
            line.Contains("invalid", StringComparison.OrdinalIgnoreCase) ||
            line.Contains("failed", StringComparison.OrdinalIgnoreCase))
        {
            AppendLog(line.Trim());
        }
    }

    private void AppendLog(string text)
    {
        LogTextBox.Text = string.IsNullOrEmpty(LogTextBox.Text)
            ? text
            : $"{LogTextBox.Text}{Environment.NewLine}{text}";
        LogTextBox.CaretIndex = LogTextBox.Text.Length;
    }

    private void SetStatus(string text) => StatusText.Text = text;
}
