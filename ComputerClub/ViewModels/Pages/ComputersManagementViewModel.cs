using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ComputerClub.Infrastructure;
using ComputerClub.Infrastructure.Entities;
using ComputerClub.Mappers;
using ComputerClub.Messages;
using ComputerClub.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ComputerClub.ViewModels.Pages;

public partial class ComputersManagementViewModel(
    ApplicationDbContext context,
    IServiceScopeFactory scopeFactory,
    ILogger<ComputersManagementViewModel> logger)
    : ObservableObject, IRecipient<SessionChangedMessage>, IDisposable
{
    public ObservableCollection<ComputerItem> Computers { get; } = [];

    public IReadOnlyList<ComputerTypeDefinition> ComputerTypes => ComputerClub.ComputerTypes.All;

    private readonly Dictionary<int, CancellationTokenSource> _saveTokens = new();
    
    private CancellationTokenSource? _timerCts;

    [RelayCommand]
    private async Task Loaded()
    {
        var computerEntities = await context.Computers.ToListAsync();
        
        foreach (var entity in computerEntities)
        {
            var item = entity.Map();
            Subscribe(item);
            Computers.Add(item);
        }

        await RefreshStatuses();
        WeakReferenceMessenger.Default.RegisterAll(this);
        StartTimer();
    }

    [RelayCommand]
    private void Unloaded() => StopTimer();
    
    [RelayCommand]
    private async Task RefreshStatuses()
    {
        var entities = await context.Computers.ToListAsync();

        var activeSessions = await context.Sessions
            .Where(s => s.Status == SessionStatus.Active)
            .ToListAsync();

        foreach (var entity in entities)
        {
            var item = Computers.FirstOrDefault(c => c.Id == entity.Id);
            if (item is null) continue;

            item.Status = entity.Status;
            item.SessionStartedAt = activeSessions
                .FirstOrDefault(s => s.ComputerId == entity.Id)?.StartedAt;
        }
    }

    private void StartTimer()
    {
        StopTimer();
        _timerCts = new CancellationTokenSource();
        _ = TickAsync(_timerCts.Token);
    }

    private void StopTimer()
    {
        _timerCts?.Cancel();
        _timerCts?.Dispose();
        _timerCts = null;
    }
    
    private async Task TickAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        try
        {
            while (await timer.WaitForNextTickAsync(ct))
            {
                foreach (var computer in Computers)
                {
                    computer.RefreshDuration();
                }
            }
        }
        catch (OperationCanceledException) { }
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
    private async Task RemoveComputer(ComputerItem item)
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
    private async Task SetComputerType((ComputerItem computerCanvasItem, ComputerTypeDefinition type) param)
    {
        var (computerCanvasItem, type) = param;

        computerCanvasItem.TypeId = type.Id;

        var entity = await context.Computers.FindAsync(computerCanvasItem.Id);
        if (entity is null) return;

        entity.TypeId = type.Id;
        await context.SaveChangesAsync();
    }

    private void Subscribe(ComputerItem item)
    {
        item.PropertyChanged += async (_, e) =>
        {
            if (e.PropertyName is not nameof(ComputerItem.X) and not nameof(ComputerItem.Y)) return;

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
                if (entity is null) return;

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

    public async void Receive(SessionChangedMessage message)
    {
        try
        {
            await RefreshStatusesCommand.ExecuteAsync(null);
        }
        catch (Exception e)
        {
            logger.LogError("SessionChangeMessage.Receive: {Message}", e.Message);
        }
    }
    
    public void Dispose() => StopTimer();
}