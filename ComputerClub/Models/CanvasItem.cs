using CommunityToolkit.Mvvm.ComponentModel;

namespace ComputerClub.Models;

public partial class CanvasItem : ObservableObject
{
    [ObservableProperty]
    private int _id;
    
    [ObservableProperty]
    private double _x;

    [ObservableProperty]
    private double _y;

    [ObservableProperty]
    private int _typeId;
}