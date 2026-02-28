using CommunityToolkit.Mvvm.ComponentModel;
using ComputerClub.Infrastructure.Entities;

namespace ComputerClub.Models;

public partial class SessionItem : ObservableObject
{
    [ObservableProperty] private int _id;
    [ObservableProperty] private int _clientId;
    [ObservableProperty] private string _clientName = string.Empty;
    [ObservableProperty] private int _computerId;
    [ObservableProperty] private int _tariffId;
    [ObservableProperty] private string _tariffName = string.Empty;
    [ObservableProperty] private decimal _pricePerHour;
    [ObservableProperty] private DateTime _startedAt;
    [ObservableProperty] private DateTime? _endedAt;
    [ObservableProperty] private decimal? _totalCost;
    [ObservableProperty] private SessionStatus _status;
    [ObservableProperty] private TimeSpan _plannedDuration;

    public bool IsActive => Status == SessionStatus.Active;

    public string TimeDisplay
    {
        get
        {
            if (!IsActive) return "—";
            var remaining = PlannedDuration - (DateTime.UtcNow - StartedAt);
            return TimeSpan.FromTicks(Math.Max(0, remaining.Ticks)).ToString(@"hh\:mm\:ss");
        }
    }

    public void RefreshDuration() => OnPropertyChanged(nameof(TimeDisplay));
}