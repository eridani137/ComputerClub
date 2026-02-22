using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComputerClub.Infrastructure;
using ComputerClub.Infrastructure.Entities;
using ComputerClub.Mappers;
using ComputerClub.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ComputerClub.ViewModels;

public partial class ManagementViewModel(ApplicationDbContext context, IServiceScopeFactory scopeFactory) : ObservableObject
{
    public ObservableCollection<CanvasItem> PcItems { get; } = [];

    private readonly Dictionary<int, CancellationTokenSource> _saveTokens = new();
    
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

    [RelayCommand]
    private async Task SetPcType(TypeSelectionItem selection)
    {
        selection.Owner.TypeId = selection.TypeId;

        var entity = await context.Pcs.FindAsync(selection.Owner.Id);
        if (entity == null) return;

        entity.TypeId = selection.TypeId;
        await context.SaveChangesAsync();
    }

    private void Subscribe(CanvasItem item)
    {
        item.PropertyChanged += async (_, e) =>
        {
            if (e.PropertyName is not nameof(CanvasItem.X) and not nameof(CanvasItem.Y)) return;

            if (_saveTokens.TryGetValue(item.Id, out var oldToken))
            {
                await oldToken.CancelAsync();
                _saveTokens.Remove(item.Id);
                oldToken.Dispose();
            }

            var cts = new CancellationTokenSource();
            _saveTokens[item.Id] = cts;
            var token = cts.Token;

            try
            {
                await Task.Delay(700, token);
                if (token.IsCancellationRequested) return;

                await using var scope = scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var entity = await db.Pcs.FindAsync(item.Id);
                if (entity == null) return;

                entity.X = item.X;
                entity.Y = item.Y;

                await db.SaveChangesAsync(token);
            }
            catch (TaskCanceledException) { }
            finally
            {
                if (_saveTokens.TryGetValue(item.Id, out var t) && t == cts)
                {
                    _saveTokens.Remove(item.Id);
                    cts.Dispose();
                }
            }
        };
    }
}