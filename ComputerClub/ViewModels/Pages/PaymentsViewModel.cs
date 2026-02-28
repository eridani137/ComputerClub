using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComputerClub.Infrastructure.Entities;
using ComputerClub.Mappers;
using ComputerClub.Models;
using ComputerClub.Services;
using Microsoft.EntityFrameworkCore;

namespace ComputerClub.ViewModels.Pages;

public partial class PaymentsViewModel(PaymentService paymentService)
    : ObservableObject
{
    public ObservableCollection<PaymentItem> Payments { get; } = [];

    [ObservableProperty] private PaymentItem? _selectedPayment;
    [ObservableProperty] private PaymentType? _filterType;
    [ObservableProperty] private string? _filterClient;

    public IReadOnlyList<PaymentType?> PaymentTypes =>
    [
        null,
        PaymentType.TopUp,
        PaymentType.Charge,
        PaymentType.Refund
    ];
    
    [RelayCommand]
    private async Task Loaded()
    {
        await Refresh();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        Payments.Clear();

        var query = paymentService.GetAll();

        if (FilterType.HasValue)
        {
            query = query.Where(p => p.Type == FilterType);
        }

        if (!string.IsNullOrWhiteSpace(FilterClient))
        {
            query = query.Where(p => p.Client.UserName!.Contains(FilterClient));
        }

        var items = await query.ToListAsync();
        foreach (var item in items)
        {
            Payments.Add(item.Map());
        }
    }

    partial void OnFilterTypeChanged(PaymentType? value)
    {
        _ = Refresh();
    }

    partial void OnFilterClientChanged(string? value)
    {
        _ = Refresh();
    }
}