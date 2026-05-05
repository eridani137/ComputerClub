using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComputerClub.Infrastructure.Entities;
using ComputerClub.Services;
using Microsoft.AspNetCore.Identity;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace ComputerClub.ViewModels.Pages;

public partial class TopUpViewModel(
    UserManager<ComputerClubIdentity> userManager,
    PaymentService paymentService,
    ISnackbarService snackbarService
) : ObservableObject
{
    [ObservableProperty] private decimal _balance;
    [ObservableProperty] private decimal _amount = 1000;
    [ObservableProperty] private PaymentType _selectedPaymentType = PaymentType.TopUpCash;
    [ObservableProperty] private string? _errorMessage;

    [RelayCommand]
    private async Task Loaded()
    {
        var userId = App.CurrentUser?.Id;
        if (userId is null) return;

        var user = await userManager.FindByIdAsync(userId.ToString()!);
        if (user is not null)
        {
            Balance = user.Balance;
        }
    }

    [RelayCommand]
    private async Task TopUp()
    {
        ErrorMessage = null;

        var userId = App.CurrentUser?.Id;
        if (userId is null) return;

        var topUpAmount = Amount;

        try
        {
            await paymentService.TopUp(userId.Value, topUpAmount, SelectedPaymentType);
            Balance += topUpAmount;
            Amount = 1000;
            snackbarService.Show(
                "Баланс пополнен",
                $"+{topUpAmount:N2} ₽",
                ControlAppearance.Success,
                new SymbolIcon(SymbolRegular.CheckmarkCircle24),
                TimeSpan.FromSeconds(3));
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
    }
}
