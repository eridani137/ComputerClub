using CommunityToolkit.Mvvm.ComponentModel;

namespace ComputerClub.Models;

public partial class PaymentItem : ObservableObject
{
    [ObservableProperty] private int _id;
    [ObservableProperty] private int _clientId;
    [ObservableProperty] private string _clientName = string.Empty;
    [ObservableProperty] private decimal _amount;
    [ObservableProperty] private DateTime _createdAt;
    [ObservableProperty] private int? _sessionId;

    public bool IsTopUp => Amount > 0;
    public string AmountDisplay => Amount > 0 ? $"+{Amount:N2} ₽" : $"{Amount:N2} ₽";
}