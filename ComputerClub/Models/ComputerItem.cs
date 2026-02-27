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

    public TimeSpan? Remaining
    {
        get
        {
            if (SessionStartedAt is null || SessionPlannedDuration is null) return null;
            var elapsed = DateTime.UtcNow - SessionStartedAt.Value;
            return TimeSpan.FromTicks(Math.Max(0, (SessionPlannedDuration.Value - elapsed).Ticks));
        }
    }

    public bool IsOvertime =>
        SessionStartedAt.HasValue && SessionPlannedDuration.HasValue &&
        DateTime.UtcNow - SessionStartedAt.Value > SessionPlannedDuration.Value;

    public void RefreshDuration()
    {
        OnPropertyChanged(nameof(Remaining));
        OnPropertyChanged(nameof(IsOvertime));
    }
}