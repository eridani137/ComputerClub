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
    
    public IEnumerable<TypeSelectionItem> TypeOptions =>
        PcTypes.All.Select(t => new TypeSelectionItem(this, t));
}

public class TypeSelectionItem(CanvasItem owner, PcTypeDefinition typeDefinition)
{
    public CanvasItem Owner { get; } = owner;

    public string Label => typeDefinition.Name;
    public int TypeId => typeDefinition.Id;
}