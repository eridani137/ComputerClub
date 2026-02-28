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
    UserManager<ComputerClubIdentity> userManager,
    SessionTickService tickService
) : ObservableObject, ISessionTick, IDisposable
{
    public ObservableCollection<SessionItem> Sessions { get; } = [];

    public ObservableCollection<ClientItem> Clients { get; } = [];
    public ObservableCollection<TariffItem> Tariffs { get; } = [];
    public ObservableCollection<ComputerItem> AvailableComputers { get; } = [];

    [ObservableProperty] private ClientItem? _selectedClient;
    [ObservableProperty] private TariffItem? _selectedTariff;
    [ObservableProperty] private ComputerItem? _selectedComputer;
    [ObservableProperty] private string? _errorMessage;

    [ObservableProperty] private int _plannedHours = 1;

    [RelayCommand]
    private async Task Loaded()
    {
        await RefreshAll();
        tickService.Register(this);
    }

    private TimeSpan GetPlannedDuration()
    {
        return TimeSpan.FromHours(PlannedHours);
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

        if (PlannedHours <= 0)
        {
            ErrorMessage = "Укажите время аренды";
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
                tariff.Id,
                GetPlannedDuration());

            var full = await context.Sessions
                .Include(s => s.Client)
                .Include(s => s.Tariff)
                .FirstAsync(s => s.Id == session.Id);

            Sessions.Add(full.Map());

            AvailableComputers.Remove(computer);
            computer.Status = ComputerStatus.Occupied;

            SelectedClient = null;
            SelectedTariff = null;
            SelectedComputer = null;
            PlannedHours = 1;

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
            item.OvertimeDuration = session.OvertimeDuration;

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
        Sessions.Clear();
        Clients.Clear();
        Tariffs.Clear();

        var sessions = await context.Sessions
            .Include(s => s.Client)
            .Include(s => s.Tariff)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync();

        foreach (var session in sessions)
        {
            Sessions.Add(session.Map());
        }

        var clients = await userManager.Users.ToListAsync();
        foreach (var client in clients)
        {
            Clients.Add(client.Map());
        }

        var tariffs = await context.Tariffs.ToListAsync();
        foreach (var tariff in tariffs)
        {
            Tariffs.Add(tariff.Map());
        }

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

    public void Tick()
    {
        foreach (var session in Sessions.Where(s => s.IsActive))
        {
            session.RefreshDuration();
        }

        var expired = Sessions.Where(s => s is { IsActive: true, IsOvertime: true }).ToList();
        foreach (var session in expired)
        {
            _ = CloseSessionCommand.ExecuteAsync(session);
        }
    }

    public void Dispose() => tickService.Unregister(this);
}