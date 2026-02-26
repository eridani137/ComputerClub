using CommunityToolkit.Mvvm.ComponentModel;

namespace ComputerClub.Models;

public partial class ClientItem : ObservableObject
{
    [ObservableProperty] private int _id;
    [ObservableProperty] private string _login = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _fullName = string.Empty;
    [ObservableProperty] private string? _phoneNumber;
    [ObservableProperty] private decimal _balance;
}