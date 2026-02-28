using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComputerClub.Infrastructure;
using ComputerClub.Infrastructure.Entities;
using ComputerClub.Models;
using ComputerClub.Services;
using Microsoft.EntityFrameworkCore;

namespace ComputerClub.ViewModels.Pages;

public partial class CreateSessionViewModel(
    ApplicationDbContext context,
    SessionService sessionService
) : ObservableObject
{
    public ObservableCollection<ScheduleRow> Rows { get; } = [];

    [ObservableProperty] private DateTime _selectedDate = DateTime.Today;
    [ObservableProperty] private int _startHour;
    [ObservableProperty] private string? _errorMessage;

    private ScheduleRow? _selectedRow;
    private int _dragStartHour = -1;
    private int _dragEndHour = -1;

    public int HoursCount => 24;
    public IEnumerable<int> Hours => Enumerable.Range(0, HoursCount);

    public event Action? SessionCreated;
    
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

        if (_selectedRow is not null && !ReferenceEquals(row, _selectedRow))
        {
            ClearSelection();
        }

        _selectedRow = row;

        if (cell.IsSelected)
        {
            var start = Math.Min(_dragStartHour, _dragEndHour);
            var end = Math.Max(_dragStartHour, _dragEndHour);

            if (cell.Hour == start) _dragStartHour++;
            else if (cell.Hour == end)
            {
                _dragEndHour--;
            }
            else
            {
                ClearSelection(); return;
            }
        }
        else
        {
            if (_dragStartHour < 0)
            {
                _dragStartHour = cell.Hour;
                _dragEndHour = cell.Hour;
            }
            else
            {
                var start = Math.Min(_dragStartHour, _dragEndHour);
                var end = Math.Max(_dragStartHour, _dragEndHour);

                if (cell.Hour == start - 1) _dragStartHour = cell.Hour;
                else if (cell.Hour == end + 1) _dragEndHour = cell.Hour;
                else
                {
                    ClearSelection();
                    _dragStartHour = cell.Hour;
                    _dragEndHour = cell.Hour;
                }
            }
        }

        if (_dragStartHour > _dragEndHour)
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
        if (_selectedRow is null || _dragStartHour < 0)
        {
            SelectionSummary = string.Empty;
            return;
        }

        var start = Math.Min(_dragStartHour, _dragEndHour);
        var end = Math.Max(_dragStartHour, _dragEndHour);
        var hours = end - start + 1;

        SelectionSummary = $"ПК №{_selectedRow.ComputerId} · {start:D2}:00 – {end + 1:D2}:00 · {hours} ч.";
    }

    private bool CanConfirm()
    {
        return _selectedRow is not null && _dragStartHour >= 0;
    }
    
    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private async Task Confirm()
    {
        if (App.CurrentUser is null)
        {
            ErrorMessage = "Ошибка получения идентификатора";
            return;
        }
        
        ErrorMessage = null;

        if (_selectedRow is null || _dragStartHour < 0)
        {
            ErrorMessage = "Выберите время";
            return;
        }

        var start = Math.Min(_dragStartHour, _dragEndHour);
        var end = Math.Max(_dragStartHour, _dragEndHour);
        var hours = end - start + 1;

        var tariff = await context.Tariffs
            .FirstOrDefaultAsync(t => t.ComputerTypeId == _selectedRow.ComputerTypeId);

        if (tariff is null)
        {
            ErrorMessage = $"Тариф для типа «{_selectedRow.TypeName}» не найден";
            return;
        }

        try
        {
            await sessionService.OpenSession(
                App.CurrentUser.Id,
                _selectedRow.ComputerId,
                tariff.Id,
                TimeSpan.FromHours(hours));

            SessionCreated?.Invoke();
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
        _dragStartHour = -1;
        _dragEndHour = -1;
        _selectedRow = null;

        var computers = await context.Computers
            .Where(c => c.Status != ComputerStatus.OutOfService)
            .OrderBy(c => c.Id)
            .ToListAsync();

        var dayStart = SelectedDate.Date.ToUniversalTime();
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

        foreach (var computer in computers)
        {
            var type = ComputerTypes.GetById(computer.TypeId);
            var row = new ScheduleRow
            {
                ComputerId = computer.Id,
                ComputerTypeId = computer.TypeId,
                TypeName = type.Name
            };

            for (var h = 0; h < HoursCount; h++)
            {
                var slotStart = dayStart.AddHours(h);
                var slotEnd = slotStart.AddHours(1);

                var session = sessions.FirstOrDefault(s =>
                    s.ComputerId == computer.Id &&
                    s.StartedAt < slotEnd &&
                    s.StartedAt + s.PlannedDuration > slotStart);

                row.Cells.Add(new ScheduleCell
                {
                    Hour = h,
                    IsOccupied = session is not null,
                    SessionLabel = session is not null && h == (int)(session.StartedAt - dayStart).TotalHours
                        ? session.Client.FullName
                        : string.Empty
                });
            }

            Rows.Add(row);
        }
    }

    private void UpdateSelection()
    {
        if (_selectedRow is null) return;

        var start = Math.Min(_dragStartHour, _dragEndHour);
        var end = Math.Max(_dragStartHour, _dragEndHour);

        foreach (var cell in _selectedRow.Cells)
        {
            cell.IsSelected = cell.Hour >= start && cell.Hour <= end && !cell.IsOccupied;
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
        
        ConfirmCommand.NotifyCanExecuteChanged();
    }
}