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
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ComputerClub.ViewModels.Pages;

public partial class SessionsViewModel(
    ApplicationDbContext context,
    SessionService sessionService,
    UserManager<ComputerClubIdentity> userManager
) : ObservableObject, IDisposable
{
    public ObservableCollection<SessionItem> ActiveSessions { get; } = [];

    public ObservableCollection<ClientItem> Clients { get; } = [];
    public ObservableCollection<TariffItem> Tariffs { get; } = [];
    public ObservableCollection<ComputerItem> AvailableComputers { get; } = [];

    [ObservableProperty] private ClientItem? _selectedClient;
    [ObservableProperty] private TariffItem? _selectedTariff;
    [ObservableProperty] private ComputerItem? _selectedComputer;
    [ObservableProperty] private string? _errorMessage;

    private CancellationTokenSource? _timerCts;

    [RelayCommand]
    private async Task Loaded()
    {
        await RefreshAll();
        StartTimer();
    }
    
    [RelayCommand]
    private void Unloaded()
    {
        StopTimer();
    }

    private void StartTimer()
    {
        StopTimer();
        _timerCts = new CancellationTokenSource();
        _ = TickDurationAsync(_timerCts.Token);
    }

    private void StopTimer()
    {
        _timerCts?.Cancel();
        _timerCts?.Dispose();
        _timerCts = null;
    }

    [RelayCommand]
    private async Task OpenSession()
    {
        ErrorMessage = null;

        if (SelectedClient is null || SelectedTariff is null || SelectedComputer is null)
        {
            ErrorMessage = "Выберите клиента, компьютер и тариф";
            return;
        }

        var client = SelectedClient;
        var computer = SelectedComputer;
        var tariff = SelectedTariff;

        try
        {
            var session = await sessionService.OpenSession(
                client.Id,
                computer.Id,
                tariff.Id);

            var full = await context.Sessions
                .Include(s => s.Client)
                .Include(s => s.Tariff)
                .FirstAsync(s => s.Id == session.Id);

            ActiveSessions.Add(full.Map());

            AvailableComputers.Remove(computer);
            computer.Status = ComputerStatus.Occupied;

            SelectedClient = null;
            SelectedTariff = null;
            SelectedComputer = null;

            WeakReferenceMessenger.Default.Send(new SessionChangedMessage(computer.Id));
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
    }

    [RelayCommand]
    private async Task CloseSession(SessionItem item)
    {
        ErrorMessage = null;

        try
        {
            var session = await sessionService.CloseSession(item.Id);

            item.EndedAt = session.EndedAt;
            item.TotalCost = session.TotalCost;
            item.Status = session.Status;

            ActiveSessions.Remove(item);

            var client = Clients.FirstOrDefault(c => c.Id == item.ClientId);
            client?.Balance = session.Client.Balance;

            await RefreshAvailableComputers();

            WeakReferenceMessenger.Default.Send(new SessionChangedMessage(item.ComputerId));
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
    }

    private async Task RefreshAll()
    {
        ActiveSessions.Clear();
        Clients.Clear();
        Tariffs.Clear();

        var sessions = await sessionService.GetActiveSessions().ToListAsync();
        foreach (var s in sessions) ActiveSessions.Add(s.Map());

        var clients = await userManager.Users.ToListAsync();
        foreach (var c in clients) Clients.Add(c.Map());

        var tariffs = await context.Tariffs.ToListAsync();
        foreach (var t in tariffs) Tariffs.Add(t.Map());

        await RefreshAvailableComputers();
    }

    private async Task RefreshAvailableComputers()
    {
        AvailableComputers.Clear();

        var computers = await context.Computers
            .Where(c => c.Status == ComputerStatus.Available)
            .ToListAsync();
        foreach (var c in computers)
        {
            AvailableComputers.Add(c.Map());
        }
    }

    private async Task TickDurationAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        try
        {
            while (await timer.WaitForNextTickAsync(ct))
            {
                foreach (var session in ActiveSessions)
                {
                    session.RefreshDuration();
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    partial void OnSelectedComputerChanged(ComputerItem? value)
    {
        if (value is null)
        {
            SelectedTariff = null;
            return;
        }

        SelectedTariff = Tariffs.FirstOrDefault(t => t.ComputerTypeId == value.TypeId);

        ErrorMessage = SelectedTariff is null
            ? $"Для типа '{ComputerTypes.GetById(value.TypeId).Name}' тариф не найден"
            : null;
    }
    
    public void Dispose() => StopTimer();
}