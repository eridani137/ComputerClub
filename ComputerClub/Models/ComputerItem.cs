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

    public bool IsOvertime =>
        SessionStartedAt.HasValue && SessionPlannedDuration.HasValue &&
        DateTime.UtcNow - SessionStartedAt.Value > SessionPlannedDuration.Value;

    public string SessionTimeDisplay
    {
        get
        {
            if (SessionStartedAt is null || SessionPlannedDuration is null) return string.Empty;
            var elapsed = DateTime.UtcNow - SessionStartedAt.Value;
            var remaining = SessionPlannedDuration.Value - elapsed;

            return remaining.Ticks > 0
                ? remaining.ToString(@"hh\:mm\:ss")
                : $@"+{(-remaining):hh\:mm\:ss}";
        }
    }

    public void RefreshDuration()
    {
        OnPropertyChanged(nameof(SessionTimeDisplay));
        OnPropertyChanged(nameof(IsOvertime));
    }
}