using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComputerClub.Models;

namespace ComputerClub.ViewModels;

public partial class ManagementViewModel : ObservableObject
{
    public ObservableCollection<CanvasItem> Items { get; } = [];

    [RelayCommand]
    private void Loaded()
    {
    }

    [RelayCommand]
    private void AddItem()
    {
        Items.Add(new CanvasItem { X = 50, Y = 50, Content = "ПК" });
    }

    [RelayCommand]
    private void RemoveItem(CanvasItem item)
    {
        Items.Remove(item);
    }
}