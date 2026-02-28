using CommunityToolkit.Mvvm.ComponentModel;
using ComputerClub.Infrastructure.Entities;

namespace ComputerClub.Models;

public partial class PaymentItem : ObservableObject
{
    [ObservableProperty] private int _id;
    [ObservableProperty] private int _clientId;
    [ObservableProperty] private string _clientName = string.Empty;
    [ObservableProperty] private decimal _amount;
    [ObservableProperty] private PaymentType _type;
    [ObservableProperty] private DateTime _createdAt;
    [ObservableProperty] private int? _sessionId;

    public string TypeDisplay => Type switch
    {
        PaymentType.TopUp => "Пополнение",
        PaymentType.Charge => "Списание",
        PaymentType.Refund => "Возврат",
        _ => string.Empty
    };

    public string AmountDisplay => Amount > 0
        ? $"+{Amount:N2} ₽"
        : $"{Amount:N2} ₽";
}