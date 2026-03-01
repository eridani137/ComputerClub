using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComputerClub.Infrastructure;
using ComputerClub.Infrastructure.Entities;
using ComputerClub.Models;
using ComputerClub.Services;
using ComputerClub.Views.Pages;
using Microsoft.EntityFrameworkCore;
using Wpf.Ui;

namespace ComputerClub.ViewModels.Pages;

public partial class CreateSessionViewModel(
    ApplicationDbContext context,
    SessionService sessionService,
    INavigationService navigationService
) : ObservableObject
{
    public ObservableCollection<ScheduleRow> Rows { get; } = [];

    public int SlotsCount => 48;
    public IEnumerable<int> Slots => Enumerable.Range(0, SlotsCount);

    public IEnumerable<SlotHeader> SlotHeaders =>
        Slots.Select(i => new SlotHeader(i, i % 2 == 0 ? $"{i / 2:D2}:00" : string.Empty));
    
    [ObservableProperty] private DateTime _selectedDate = DateTime.Today;
    [ObservableProperty] private int _startHour;
    [ObservableProperty] private string? _errorMessage;

    private ScheduleRow? _selectedRow;
    private int _dragStartSlot = -1;
    private int _dragEndSlot = -1;
    
    [RelayCommand]
    private async Task Loaded() => await Refresh();

    [RelayCommand]
    private async Task PreviousDay()
    {
        SelectedDate = SelectedDate.AddDays(-1);
        await Refresh();
    }

    [RelayCommand]
    private async Task NextDay()
    {
        SelectedDate = SelectedDate.AddDays(1);
        await Refresh();
    }

    [RelayCommand]
    private async Task GoToToday()
    {
        SelectedDate = DateTime.Today;
        await Refresh();
    }
    
    public void OnCellClick(ScheduleRow row, ScheduleCell cell)
    {
        if (cell.IsOccupied) return;
        if (cell.IsPast) return;

        if (_selectedRow is not null && !ReferenceEquals(row, _selectedRow))
        {
            ClearSelection();
        }

        _selectedRow = row;

        if (cell.IsSelected)
        {
            var start = Math.Min(_dragStartSlot, _dragEndSlot);
            var end = Math.Max(_dragStartSlot, _dragEndSlot);

            if (cell.SlotIndex == start)
            {
                _dragStartSlot++;
            }
            else if (cell.SlotIndex == end)
            {
                _dragEndSlot--;
            }
            else
            {
                ClearSelection(); 
                return;
            }
        }
        else
        {
            if (_dragStartSlot < 0)
            {
                _dragStartSlot = cell.SlotIndex;
                _dragEndSlot = cell.SlotIndex;
            }
            else
            {
                var start = Math.Min(_dragStartSlot, _dragEndSlot);
                var end = Math.Max(_dragStartSlot, _dragEndSlot);

                if (cell.SlotIndex == start - 1) _dragStartSlot = cell.SlotIndex;
                else if (cell.SlotIndex == end + 1) _dragEndSlot = cell.SlotIndex;
                else
                {
                    ClearSelection();
                    _dragStartSlot = cell.SlotIndex;
                    _dragEndSlot = cell.SlotIndex;
                }
            }
        }

        if (_dragStartSlot > _dragEndSlot)
        {
            ClearSelection(); 
            return;
        }

        UpdateSelection();
        UpdateSummary();
    }

    [ObservableProperty] private string _selectionSummary = string.Empty;

    private void UpdateSummary()
    {
        if (_selectedRow is null || _dragStartSlot < 0)
        {
            SelectionSummary = string.Empty;
            return;
        }

        var start = Math.Min(_dragStartSlot, _dragEndSlot);
        var end = Math.Max(_dragStartSlot, _dragEndSlot);
        var duration = TimeSpan.FromMinutes((end - start + 1) * 30);

        var startTime = TimeSpan.FromMinutes(start * 30);
        var endTime = TimeSpan.FromMinutes((end + 1) * 30);

        SelectionSummary = $"ПК №{_selectedRow.ComputerId} · {startTime:hh\\:mm} – {endTime:hh\\:mm} · {duration.TotalHours:0.#} ч.";
    }

    private bool CanConfirm()
    {
        return _selectedRow is not null && _dragStartSlot >= 0;
    }
    
    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private async Task Confirm()
    {
        if (App.CurrentUser is null)
        {
            ErrorMessage = "Ошибка получения пользователя";
            return;
        }
        if (_selectedRow is null || _dragStartSlot < 0)
        {
            ErrorMessage = "Выберите время";
            return;
        }

        ErrorMessage = null;

        var startSlot = Math.Min(_dragStartSlot, _dragEndSlot);
        var endSlot = Math.Max(_dragStartSlot, _dragEndSlot);
        var duration = TimeSpan.FromMinutes((endSlot - startSlot + 1) * 30);

        var localStart = SelectedDate.Date.AddMinutes(startSlot * 30);
        var utcStart = DateTime.SpecifyKind(localStart, DateTimeKind.Local).ToUniversalTime();
        
        if (utcStart < DateTime.UtcNow)
        {
            ErrorMessage = "Нельзя зарезервировать сессию в прошлом";
            return;
        }

        var tariff = await context.Tariffs
            .FirstOrDefaultAsync(t => t.ComputerTypeId == _selectedRow.ComputerTypeId);

        if (tariff is null)
        {
            ErrorMessage = $"Тариф для типа '{_selectedRow.TypeName}' не найден";
            return;
        }

        try
        {
            await sessionService.ReserveSession(
                App.CurrentUser.Id,
                _selectedRow.ComputerId,
                tariff.Id,
                utcStart,
                duration);

            navigationService.Navigate(typeof(ClientSessionPage));
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private async Task Refresh()
    {
        Rows.Clear();
        ClearSelection();
        _dragStartSlot = -1;
        _dragEndSlot = -1;
        _selectedRow = null;

        var computers = await context.Computers
            .Where(c => c.Status != ComputerStatus.OutOfService)
            .OrderBy(c => c.Id)
            .ToListAsync();

        var nowUtc = DateTime.UtcNow;
        var dayStart = DateTime.SpecifyKind(SelectedDate.Date, DateTimeKind.Local).ToUniversalTime();
        var dayEnd = dayStart.AddDays(1);

        var sessions = await context.Sessions
            .Include(s => s.Client)
            .Where(s => s.Status == SessionStatus.Active &&
                        s.StartedAt < dayEnd &&
                        s.StartedAt > dayStart.AddDays(-1))
            .ToListAsync();
        
        sessions = sessions
            .Where(s => s.StartedAt + s.PlannedDuration > dayStart)
            .ToList();
        
        var reservations = await context.Reservations
            .Include(r => r.Client)
            .Where(r => r.Status == ReservationStatus.Pending &&
                        r.StartsAt < dayEnd &&
                        r.StartsAt > dayStart.AddDays(-1))
            .ToListAsync();
        
        reservations = reservations
            .Where(r => r.EndsAt > dayStart)
            .ToList();
        
        foreach (var computer in computers)
        {
            var type = ComputerTypes.GetById(computer.TypeId);
            var row = new ScheduleRow
            {
                ComputerId = computer.Id,
                ComputerTypeId = computer.TypeId,
                TypeName = type.Name
            };

            for (var slot = 0; slot < SlotsCount; slot++)
            {
                var slotStart = dayStart.AddMinutes(slot * 30);
                var slotEnd = slotStart.AddMinutes(30);

                var session = sessions.FirstOrDefault(s =>
                    s.ComputerId == computer.Id &&
                    s.StartedAt < slotEnd &&
                    s.StartedAt + s.PlannedDuration > slotStart);

                var reservation = reservations.FirstOrDefault(r =>
                    r.ComputerId == computer.Id &&
                    r.StartsAt < slotEnd &&
                    r.EndsAt > slotStart);
                
                var sessionSlot = (int)((session?.StartedAt - dayStart)?.TotalMinutes / 30 ?? -1);
                var reservationSlot = (int)((reservation?.StartsAt - dayStart)?.TotalMinutes / 30 ?? -1);
                
                row.Cells.Add(new ScheduleCell
                {
                    SlotIndex = slot,
                    IsOccupied = session is not null || reservation is not null,
                    IsReservation = reservation is not null && session is null,
                    IsPast = slotEnd <= nowUtc,
                    SessionLabel = session is not null && slot == sessionSlot
                        ? session.Client.UserName!
                        : reservation is not null && slot == reservationSlot
                            ? $"[Р] {reservation.Client.UserName}"
                            : string.Empty
                });
            }

            Rows.Add(row);
        }
    }

    private void UpdateSelection()
    {
        if (_selectedRow is null) return;
        var start = Math.Min(_dragStartSlot, _dragEndSlot);
        var end = Math.Max(_dragStartSlot, _dragEndSlot);
        foreach (var cell in _selectedRow.Cells)
        {
            cell.IsSelected = cell.SlotIndex >= start && cell.SlotIndex <= end && !cell.IsOccupied;
        }
        
        ConfirmCommand.NotifyCanExecuteChanged();
    }

    private void ClearSelection()
    {
        foreach (var row in Rows)
        {
            foreach (var cell in row.Cells)
            {
                cell.IsSelected = false;
            }
        }
        
        _dragStartSlot = -1;
        _dragEndSlot = -1;
        _selectedRow = null;

        SelectionSummary = string.Empty;
        
        ConfirmCommand.NotifyCanExecuteChanged();
    }
}