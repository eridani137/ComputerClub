using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComputerClub.Models;

namespace ComputerClub.ViewModels;

public partial class ManagementViewModel : ObservableObject
{
    public ObservableCollection<CanvasItem> PcItems { get; } = [];

    [RelayCommand]
    private void Loaded()
    {
    }

    [RelayCommand]
    private void AddPc()
    {
        PcItems.Add(new CanvasItem()
        {
            Pc = new PcInfo()
            {
                Type = Random.Shared.Next(0, 6)
            }
        });
    }

    [RelayCommand]
    private void RemovePc(CanvasItem item)
    {
        PcItems.Remove(item);
    }
}