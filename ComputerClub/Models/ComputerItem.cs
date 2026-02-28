using CommunityToolkit.Mvvm.ComponentModel;
using ComputerClub.Infrastructure.Entities;

namespace ComputerClub.Models;

public partial class ComputerItem : ObservableObject
{
    [ObservableProperty] private int _id;
    [ObservableProperty] private double _x;
    [ObservableProperty] private double _y;
    [ObservableProperty] private int _typeId;
    [ObservableProperty] private ComputerStatus _status;
    [ObservableProperty] private DateTime? _sessionStartedAt;
    [ObservableProperty] private TimeSpan? _sessionPlannedDuration;

    public string SessionTimeDisplay
    {
        get
        {
            if (SessionStartedAt is null || SessionPlannedDuration is null) return string.Empty;
            var remaining = SessionPlannedDuration.Value - (DateTime.UtcNow - SessionStartedAt.Value);
            return TimeSpan.FromTicks(Math.Max(0, remaining.Ticks)).ToString(@"hh\:mm\:ss");
        }
    }

    public void RefreshDuration() => OnPropertyChanged(nameof(SessionTimeDisplay));
}