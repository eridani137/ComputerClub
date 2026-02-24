using CommunityToolkit.Mvvm.ComponentModel;
using ComputerClub.Infrastructure.Entities;

namespace ComputerClub.Models;

public partial class ComputerCanvasItem : ObservableObject
{
    [ObservableProperty] private int _id;

    [ObservableProperty] private double _x;

    [ObservableProperty] private double _y;

    [ObservableProperty] private int _typeId;

    [ObservableProperty] private ComputerStatus _status;
}