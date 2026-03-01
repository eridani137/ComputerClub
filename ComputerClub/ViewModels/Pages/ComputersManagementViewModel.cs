using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ComputerClub.Infrastructure;
using ComputerClub.Infrastructure.Entities;
using ComputerClub.Mappers;
using ComputerClub.Messages;
using ComputerClub.Models;
using ComputerClub.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace ComputerClub.ViewModels.Pages;

public partial class ComputersManagementViewModel(
    ApplicationDbContext context,
    IServiceScopeFactory scopeFactory,
    ISnackbarService snackbarService,
    SessionTickService tickService,
    ILogger<ComputersManagementViewModel> logger)
    : ObservableObject, IRecipient<SessionChangedMessage>, ISessionTick, IDisposable
{
    public ObservableCollection<ComputerItem> Computers { get; } = [];

    public IReadOnlyList<ComputerTypeDefinition> ComputerTypes => ComputerClub.ComputerTypes.All;

    private readonly Dictionary<int, CancellationTokenSource> _saveTokens = new();

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
        tickService.Register(this);
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    [RelayCommand]
    private async Task RefreshStatuses()
    {
        var entities = await context.Computers.ToListAsync();

        var activeSessions = await context.Sessions
            .Where(s => s.Status == SessionStatus.Active)
            .ToListAsync();

        var reservations = await context.Reservations
            .Where(r => r.Status == ReservationStatus.Pending)
            .ToListAsync();

        foreach (var entity in entities)
        {
            var item = Computers.FirstOrDefault(c => c.Id == entity.Id);
            if (item is null) continue;

            item.Status = entity.Status;

            var session = activeSessions.FirstOrDefault(s => s.ComputerId == entity.Id);
            item.SessionStartedAt = session?.StartedAt;
            item.SessionPlannedDuration = session?.PlannedDuration;

            var reservation = reservations
                .Where(r => r.ComputerId == entity.Id)
                .OrderBy(r => r.StartsAt)
                .FirstOrDefault();

            item.ReservationStartsAt = reservation?.StartsAt;
            item.ReservationEndsAt = reservation?.EndsAt;

            if (reservation is not null && entity.Status == ComputerStatus.Available)
            {
                item.Status = ComputerStatus.Reserved;
            }
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
    private async Task RemoveComputer(ComputerItem item)
    {
        if (item.Status != ComputerStatus.Available)
        {
            snackbarService.Show("Ошибка", "Попробуйте еще, когда компьютер освободится",
                ControlAppearance.Danger, new SymbolIcon(SymbolRegular.ErrorCircle24), TimeSpan.FromSeconds(3));
            return;
        }

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
        var (item, type) = param;

        if (item.Status is ComputerStatus.Occupied or ComputerStatus.Reserved)
        {
            snackbarService.Show("Ошибка", "Компьютер занят или зарезервирован",
                ControlAppearance.Danger, new SymbolIcon(SymbolRegular.ErrorCircle24), TimeSpan.FromSeconds(3));
            return;
        }

        item.TypeId = type.Id;

        var entity = await context.Computers.FindAsync(item.Id);
        if (entity is null) return;

        entity.TypeId = type.Id;
        await context.SaveChangesAsync();
    }
    
    [RelayCommand]
    private async Task ToggleOutOfService(ComputerItem item)
    {
        if (item.Status is ComputerStatus.Occupied or ComputerStatus.Reserved)
        {
            snackbarService.Show("Ошибка", "Компьютер занят или зарезервирован",
                ControlAppearance.Danger, new SymbolIcon(SymbolRegular.ErrorCircle24), TimeSpan.FromSeconds(3));
            return;
        }

        var entity = await context.Computers.FindAsync(item.Id);
        if (entity is null) return;

        if (entity.Status == ComputerStatus.OutOfService)
        {
            entity.Status = ComputerStatus.Available;
            item.Status = ComputerStatus.Available;
        }
        else
        {
            entity.Status = ComputerStatus.OutOfService;
            item.Status = ComputerStatus.OutOfService;
        }

        await context.SaveChangesAsync();
        WeakReferenceMessenger.Default.Send(new SessionChangedMessage(item.Id));
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

    public void Tick()
    {
        foreach (var computer in Computers)
        {
            computer.RefreshDuration();
        }
    }
    
    public void Dispose()
    {
        tickService.Unregister(this);
        foreach (var cts in _saveTokens.Values)
        {
            cts.Dispose();
        }
    }
}