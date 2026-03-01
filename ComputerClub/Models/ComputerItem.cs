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
    [ObservableProperty] private DateTime? _reservationStartsAt;
    [ObservableProperty] private DateTime? _reservationEndsAt;

    public string SessionTimeDisplay
    {
        get
        {
            if (SessionStartedAt is not null && SessionPlannedDuration is not null)
            {
                var remaining = SessionPlannedDuration.Value - (DateTime.UtcNow - SessionStartedAt.Value);
                return TimeSpan.FromTicks(Math.Max(0, remaining.Ticks)).ToString(@"hh\:mm\:ss");
            }

            if (ReservationStartsAt is not null)
            {
                var timeUntil = ReservationStartsAt.Value - DateTime.UtcNow;
                return timeUntil.Ticks > 0
                    ? $@"через {timeUntil:hh\:mm\:ss}"
                    : "скоро";
            }

            return string.Empty;
        }
    }

    public void RefreshDuration() => OnPropertyChanged(nameof(SessionTimeDisplay));
}