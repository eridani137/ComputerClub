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
    [ObservableProperty] private int _hour;
    [ObservableProperty] private bool _isOccupied;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private string _sessionLabel = string.Empty;
}