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
    private readonly Dictionary<int, CancellationTokenSource> _saveTokens = new();
    
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
            Type = Random.Shared.Next(0, 6)
        };
        
        context.Pcs.Add(entity);
        await context.SaveChangesAsync();
        
        var item = entity.Map();
        Subscribe(item);
        PcItems.Add(item);
    }

    [RelayCommand]
    private void RemovePc(CanvasItem item)
    {
        PcItems.Remove(item);
    }

    private void Subscribe(CanvasItem item)
    {
        item.PropertyChanged += async (_, e) =>
        {
            if (e.PropertyName is not nameof(CanvasItem.X) and not nameof(CanvasItem.Y)) return;

            if (_saveTokens.TryGetValue(item.Id, out var oldCts)) await oldCts.CancelAsync();

            var cts = new CancellationTokenSource();
            _saveTokens[item.Id] = cts;

            try
            {
                await Task.Delay(1000, cts.Token);

                var entity = await context.Pcs.FindAsync(item.Id);
                if (entity == null) return;

                entity.X = item.X;
                entity.Y = item.Y;

                await context.SaveChangesAsync(cts.Token);
            }
            catch (TaskCanceledException)
            {
            }
        };
    }
}