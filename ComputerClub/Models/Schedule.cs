using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ComputerClub.Models;

public partial class ScheduleRow : ObservableObject
{
    [ObservableProperty] private int _computerId;
    [ObservableProperty] private int _computerTypeId;
    [ObservableProperty] private string _typeName = string.Empty;

    public ObservableCollection<ScheduleCell> Cells { get; } = [];
}

public partial class ScheduleCell : ObservableObject
{
    [ObservableProperty] private int _slotIndex;
    [ObservableProperty] private bool _isOccupied;
    [ObservableProperty] private bool _isReservation;
    [ObservableProperty] private string _sessionLabel = string.Empty;
    [ObservableProperty] private bool _isPast;
    
    public bool IsSelected
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayBrush));
        }
    }

    public ScheduleCell DisplayBrush => this;
    
    public TimeSpan SlotTime => TimeSpan.FromMinutes(SlotIndex * 30);
}

public record SlotHeader(int Index, string Label);