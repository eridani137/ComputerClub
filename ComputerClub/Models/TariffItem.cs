using CommunityToolkit.Mvvm.ComponentModel;

namespace ComputerClub.Models;

public partial class TariffItem : ObservableObject
{
    [ObservableProperty] private int _id;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private decimal _pricePerHour;
    [ObservableProperty] private int _computerTypeId;
    
    public string ComputerTypeName => ComputerTypes.GetById(ComputerTypeId).Name;
}