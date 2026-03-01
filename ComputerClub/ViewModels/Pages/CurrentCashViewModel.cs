using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComputerClub.Mappers;
using ComputerClub.Models;
using ComputerClub.Services;
using Microsoft.EntityFrameworkCore;

namespace ComputerClub.ViewModels.Pages;

public partial class CurrentCashViewModel(
    PaymentService paymentService)
    : ObservableObject
{
    public ObservableCollection<PaymentItem> Payments { get; } = [];

    [ObservableProperty] private decimal _totalTopUp;
    [ObservableProperty] private decimal _totalCharge;
    [ObservableProperty] private decimal _totalRefund;
    [ObservableProperty] private decimal _total;

    [RelayCommand]
    private async Task Loaded() => await Refresh();

    [RelayCommand]
    private async Task Refresh()
    {
        Payments.Clear();

        var todayUtc = DateTime.UtcNow.Date;
        var tomorrowUtc = todayUtc.AddDays(1);

        var items = await paymentService.GetAll()
            .Where(p => p.CreatedAt >= todayUtc && p.CreatedAt < tomorrowUtc)
            .ToListAsync();

        foreach (var item in items)
            Payments.Add(item.Map());

        TotalTopUp = Payments
            .Where(p => p.Type == Infrastructure.Entities.PaymentType.TopUp)
            .Sum(p => p.Amount);

        TotalCharge = Payments
            .Where(p => p.Type == Infrastructure.Entities.PaymentType.Charge)
            .Sum(p => p.Amount);

        TotalRefund = Payments
            .Where(p => p.Type == Infrastructure.Entities.PaymentType.Refund)
            .Sum(p => p.Amount);

        Total = TotalTopUp + TotalCharge + TotalRefund;
    }
}