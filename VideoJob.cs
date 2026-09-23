using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BoxroomMovieMaker;

public sealed class VideoJob : INotifyPropertyChanged
{
    private string _status = "Waiting";
    private string _statusColor = "#94A0B5";

    public required string InputPath { get; init; }
    public required string DisplayName { get; init; }
    public string Folder => Path.GetDirectoryName(InputPath) ?? string.Empty;
    public string OutputPath { get; set; } = string.Empty;

    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }

    public string StatusColor
    {
        get => _statusColor;
        set { _statusColor = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
