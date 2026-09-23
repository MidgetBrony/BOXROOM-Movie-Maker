using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace BoxroomMovieMaker;

public sealed partial class AboutWindow : Window
{
    private const string FfmpegSourceUrl =
        "https://github.com/FFmpeg/FFmpeg/tree/a5923073bfd8f25b7300d93af3f8e690174ebd30";
    private const string BuildSourceUrl =
        "https://github.com/BtbN/FFmpeg-Builds/tree/ccbffa4f85d0e8de5c135c69ebb10e4c14911fa9";

    public AboutWindow() => InitializeComponent();

    private void OpenLicence_OnClick(object? sender, RoutedEventArgs e) =>
        OpenLocalFile(Path.Combine(AppContext.BaseDirectory, "Licences", "FFmpeg-GPLv3.txt"));

    private void OpenNotice_OnClick(object? sender, RoutedEventArgs e) =>
        OpenLocalFile(Path.Combine(AppContext.BaseDirectory, "Licences", "THIRD-PARTY-NOTICES.md"));

    private void OpenFfmpegSource_OnClick(object? sender, RoutedEventArgs e) => OpenUrl(FfmpegSourceUrl);

    private void OpenBuildSource_OnClick(object? sender, RoutedEventArgs e) => OpenUrl(BuildSourceUrl);

    private void Close_OnClick(object? sender, RoutedEventArgs e) => Close();

    private static void OpenLocalFile(string path)
    {
        if (File.Exists(path))
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
    }

    private static void OpenUrl(string url) =>
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
}
