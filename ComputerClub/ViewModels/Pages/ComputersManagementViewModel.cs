using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComputerClub.Infrastructure;
using ComputerClub.Infrastructure.Entities;
using ComputerClub.Mappers;
using ComputerClub.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ComputerClub.ViewModels.Pages;

public partial class ManagementViewModel(ApplicationDbContext context, IServiceScopeFactory scopeFactory)
    : ObservableObject
{
    public ObservableCollection<ComputerCanvasItem> Computers { get; } = [];

    public IReadOnlyList<ComputerTypeDefinition> ComputerTypes => ComputerClub.ComputerTypes.All;

    private readonly Dictionary<int, CancellationTokenSource> _saveTokens = new();

    [RelayCommand]
    private async Task Loaded()
    {
        Computers.Clear();

        var computerEntities = await context.Computers.ToListAsync();

        foreach (var entity in computerEntities)
        {
            var item = entity.Map();
            Subscribe(item);
            Computers.Add(item);
        }
    }

    [RelayCommand]
    private async Task AddComputer()
    {
        var entity = new ComputerEntity
        {
            X = 0,
            Y = 0,
            TypeId = Random.Shared.Next(0, 5)
        };

        context.Computers.Add(entity);
        await context.SaveChangesAsync();

        var item = entity.Map();
        Subscribe(item);
        Computers.Add(item);
    }

    [RelayCommand]
    private async Task RemoveComputer(ComputerCanvasItem item)
    {
        Computers.Remove(item);

        var entity = await context.Computers.FindAsync(item.Id);
        if (entity is not null)
        {
            context.Computers.Remove(entity);
            await context.SaveChangesAsync();
        }
    }

    [RelayCommand]
    private async Task SetComputerType((ComputerCanvasItem computerCanvasItem, ComputerTypeDefinition type) param)
    {
        var (computerCanvasItem, type) = param;

        computerCanvasItem.TypeId = type.Id;

        var entity = await context.Computers.FindAsync(computerCanvasItem.Id);
        if (entity == null) return;

        entity.TypeId = type.Id;
        await context.SaveChangesAsync();
    }

    private void Subscribe(ComputerCanvasItem item)
    {
        item.PropertyChanged += async (_, e) =>
        {
            if (e.PropertyName is not nameof(ComputerCanvasItem.X) and not nameof(ComputerCanvasItem.Y)) return;

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

                var entity = await db.Computers.FindAsync(item.Id);
                if (entity == null) return;

                entity.X = item.X;
                entity.Y = item.Y;

                await db.SaveChangesAsync(token);
            }
            catch (TaskCanceledException)
            {
            }
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