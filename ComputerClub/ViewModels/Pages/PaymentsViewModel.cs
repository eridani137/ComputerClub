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

    public const string AllTypes = "Все";

    public IReadOnlyList<object> PaymentTypes =>
    [
        AllTypes,
        PaymentType.TopUp,
        PaymentType.Charge,
        PaymentType.Refund
    ];

    [ObservableProperty] private object _selectedPaymentType = AllTypes;

    private PaymentType? _activeFilterType;

    partial void OnSelectedPaymentTypeChanged(object value)
    {
        _activeFilterType = value is PaymentType type ? type : null;
        _ = Refresh();
    }
    
    [RelayCommand]
    private async Task Loaded()
    {
        FilterType = null;
        await Refresh();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        Payments.Clear();

        var query = paymentService.GetAll();

        if (_activeFilterType.HasValue)
        {
            query = query.Where(p => p.Type == _activeFilterType);
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