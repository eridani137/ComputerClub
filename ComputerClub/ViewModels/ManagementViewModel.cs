using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComputerClub.Infrastructure;
using ComputerClub.Infrastructure.Entities;
using ComputerClub.Mappers;
using ComputerClub.Models;
using Microsoft.EntityFrameworkCore;

namespace ComputerClub.ViewModels;

public partial class ManagementViewModel(ApplicationDbContext context) : ObservableObject
{
    public ObservableCollection<CanvasItem> PcItems { get; } = [];

    [RelayCommand]
    private async Task Loaded()
    {
        PcItems.Clear();
        
        var pcs = await context.Pcs.ToListAsync();

        foreach (var pc in pcs)
        {
            var item = pc.Map();
            Subscribe(item);
            PcItems.Add(item);
        }
    }

    [RelayCommand]
    private async Task AddPc()
    {
        var entity = new PcEntity
        {
            X = 0,
            Y = 0,
            TypeId = Random.Shared.Next(0, 5)
        };
        
        context.Pcs.Add(entity);
        await context.SaveChangesAsync();
        
        var item = entity.Map();
        Subscribe(item);
        PcItems.Add(item);
    }

    [RelayCommand]
    private async Task RemovePc(CanvasItem item)
    {
        PcItems.Remove(item);
        
        var entity = await context.Pcs.FindAsync(item.Id);
        if (entity is not null)
        {
            context.Pcs.Remove(entity);
            await context.SaveChangesAsync();
        }
    }

    private void Subscribe(CanvasItem item)
    {
        item.PropertyChanged += async (_, e) =>
        {
            if (e.PropertyName is not nameof(CanvasItem.X) and not nameof(CanvasItem.Y)) return;

            var entity = await context.Pcs.FindAsync(item.Id);
            if (entity == null) return;

            entity.X = item.X;
            entity.Y = item.Y;

            await context.SaveChangesAsync();
        };
    }
}