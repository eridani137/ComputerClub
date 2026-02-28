using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComputerClub.Infrastructure;
using ComputerClub.Infrastructure.Entities;
using ComputerClub.Services;
using Microsoft.EntityFrameworkCore;

namespace ComputerClub.ViewModels.Pages;

public partial class ClientSessionViewModel(
    ApplicationDbContext context,
    SessionTickService tickService
) : ObservableObject, ISessionTick, IDisposable
{
    [ObservableProperty] private bool _hasActiveSession;
    [ObservableProperty] private int _computerId;
    [ObservableProperty] private string _tariffName = string.Empty;
    [ObservableProperty] private string _timeDisplay = "00:00:00";
    [ObservableProperty] private decimal _totalCost;

    private DateTime _startedAt;
    private TimeSpan _plannedDuration;
    private decimal _pricePerHour;
    private SessionEntity? _activeSession;

    [RelayCommand]
    private async Task Loaded()
    {
        await RefreshSession();
        tickService.Register(this);
    }

    [RelayCommand]
    private void StartSession()
    {
        // TODO
    }

    private async Task RefreshSession()
    {
        var userId = App.CurrentUser?.Id;
        if (userId is null) return;

        _activeSession = await context.Sessions
            .Include(s => s.Tariff)
            .FirstOrDefaultAsync(s =>
                s.ClientId == userId &&
                s.Status == SessionStatus.Active);

        if (_activeSession is null)
        {
            HasActiveSession = false;
            return;
        }

        HasActiveSession = true;
        ComputerId = _activeSession.ComputerId;
        TariffName = _activeSession.Tariff.Name;
        _startedAt = _activeSession.StartedAt;
        _plannedDuration = _activeSession.PlannedDuration;
        _pricePerHour = _activeSession.Tariff.PricePerHour;

        Tick();
    }


    public void Tick()
    {
        if (!HasActiveSession) return;

        var elapsed = DateTime.UtcNow - _startedAt;
        var remaining = _plannedDuration - elapsed;
        TimeDisplay = TimeSpan.FromTicks(Math.Max(0, remaining.Ticks)).ToString(@"hh\:mm\:ss");

        var hours = (decimal)elapsed.TotalHours;
        TotalCost = Math.Round(hours * _pricePerHour, 2);
    }

    public void Dispose()
    {
        tickService.Unregister(this);
    }
}