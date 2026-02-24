using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComputerClub.Infrastructure;
using ComputerClub.Infrastructure.Entities;
using ComputerClub.Mappers;
using ComputerClub.Models;
using ComputerClub.Services;
using Microsoft.EntityFrameworkCore;

namespace ComputerClub.ViewModels.Pages;

public partial class SessionsViewModel(ApplicationDbContext context, SessionService sessionService) : ObservableObject
{
    public ObservableCollection<SessionItem> ActiveSessions { get; } = [];

    public ObservableCollection<ClientItem> Clients { get; } = [];
    public ObservableCollection<TariffItem> Tariffs { get; } = [];
    public ObservableCollection<ComputerCanvasItem> AvailableComputers { get; } = [];

    [ObservableProperty] private ClientItem? _selectedClient;
    [ObservableProperty] private TariffItem? _selectedTariff;
    [ObservableProperty] private ComputerCanvasItem? _selectedComputer;
    [ObservableProperty] private string? _errorMessage;

    [RelayCommand]
    private async Task Loaded()
    {
        await RefreshAll();

        _ = TickDurationAsync();
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

        try
        {
            var session = await sessionService.OpenSession(
                SelectedClient.Id,
                SelectedComputer.Id,
                SelectedTariff.Id);

            var full = await context.Sessions
                .Include(s => s.Client)
                .Include(s => s.Tariff)
                .FirstAsync(s => s.Id == session.Id);

            ActiveSessions.Add(full.Map());

            AvailableComputers.Remove(SelectedComputer);
            SelectedComputer.Status = ComputerStatus.Occupied;

            SelectedClient = null;
            SelectedTariff = null;
            SelectedComputer = null;
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
    }
    
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

        var clients = await context.Clients.ToListAsync();
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
    
    private async Task TickDurationAsync()
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        while (await timer.WaitForNextTickAsync())
        {
            foreach (var sessionItem in ActiveSessions)
            {
                sessionItem.RefreshDuration();
            }

            if (ActiveSessions.Count == 0) break;
        }
    }
}