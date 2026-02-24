using CommunityToolkit.Mvvm.ComponentModel;

namespace ComputerClub.Models;

public partial class ComputerCanvasItem : ObservableObject
{
    [ObservableProperty]
    private int _id;
    
    [ObservableProperty]
    private double _x;

    [ObservableProperty]
    private double _y;

    [ObservableProperty]
    private int _typeId;
    
    public IEnumerable<ComputerTypeSelectionItem> TypeOptions =>
        PcTypes.All.Select(t => new ComputerTypeSelectionItem(this, t));
}

public class ComputerTypeSelectionItem(ComputerCanvasItem owner, PcTypeDefinition typeDefinition)
{
    public ComputerCanvasItem Owner { get; } = owner;

    public string Label => typeDefinition.Name;
    public int TypeId => typeDefinition.Id;
}