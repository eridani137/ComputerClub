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
    
    public TimeSpan? SessionDuration =>
        SessionStartedAt.HasValue ? DateTime.UtcNow - SessionStartedAt.Value : null;

    public void RefreshDuration() => OnPropertyChanged(nameof(SessionDuration));
}