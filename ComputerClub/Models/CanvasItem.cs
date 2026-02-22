using CommunityToolkit.Mvvm.ComponentModel;
using ComputerClub.Infrastructure.Entities;

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
    
    public required PcEntity Pc
    {
        get;
        set;
    }
}