using CommunityToolkit.Mvvm.ComponentModel;

namespace ComputerClub.Models;

public class CanvasItem : ObservableObject
{
    public double X
    {
        get;
        set => SetProperty(ref field, value);
    }

    public double Y
    {
        get;
        set => SetProperty(ref field, value);
    }
    
    public required PcInfo Pc
    {
        get;
        init => SetProperty(ref field, value);
    }
}