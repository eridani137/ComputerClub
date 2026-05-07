using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComputerClub.Infrastructure.Entities;
using ComputerClub.Mappers;
using ComputerClub.Models;
using ComputerClub.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace ComputerClub.ViewModels.Pages;

public partial class CurrentCashViewModel(
    PaymentService paymentService,
    ShiftReportService shiftReportService,
    ISnackbarService snackbarService,
    IContentDialogService dialogService)
    : ObservableObject
{
    public ObservableCollection<PaymentItem> Payments { get; } = [];

    [ObservableProperty] private decimal _topUpCash;
    [ObservableProperty] private decimal _topUpCard;
    [ObservableProperty] private decimal _totalTopUp;
    [ObservableProperty] private decimal _totalCharge;
    [ObservableProperty] private decimal _totalRefund;
    [ObservableProperty] private decimal _total;

    private DateTime _shiftStart = DateTime.UtcNow.Date;

    [RelayCommand]
    private async Task Loaded() => await Refresh();

    [RelayCommand]
    private async Task Refresh()
    {
        Payments.Clear();

        var todayUtc = DateTime.UtcNow.Date;
        var tomorrowUtc = todayUtc.AddDays(1);
        _shiftStart = todayUtc;

        var items = await paymentService.GetAll()
            .Where(p => p.CreatedAt >= todayUtc && p.CreatedAt < tomorrowUtc)
            .ToListAsync();

        foreach (var item in items)
            Payments.Add(item.Map());

        TopUpCash = Payments
            .Where(p => p.Type == PaymentType.TopUpCash)
            .Sum(p => p.Amount);

        TopUpCard = Payments
            .Where(p => p.Type == PaymentType.TopUpCard)
            .Sum(p => p.Amount);

        TotalTopUp = TopUpCash + TopUpCard;

        TotalCharge = Payments
            .Where(p => p.Type == PaymentType.Charge)
            .Sum(p => p.Amount);

        TotalRefund = Payments
            .Where(p => p.Type == PaymentType.Refund)
            .Sum(p => p.Amount);

        Total = TotalTopUp;
    }

    [RelayCommand]
    private async Task CloseShift()
    {
        var dialog = new ContentDialog
        {
            Title = "Завершить смену?",
            Content = BuildConfirmationText(),
            PrimaryButtonText = "Сохранить отчёт",
            CloseButtonText = "Отмена",
            DefaultButton = ContentDialogButton.Primary,
        };

        var result = await dialogService.ShowAsync(dialog, CancellationToken.None);

        if (result != ContentDialogResult.Primary) return;

        var dlg = new SaveFileDialog
        {
            Title = "Сохранить отчёт смены",
            Filter = "Excel файл (*.xlsx)|*.xlsx",
            FileName = $"Смена_{DateTime.Now:yyyy-MM-dd_HH-mm}.xlsx",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        if (dlg.ShowDialog() != true) return;

        try
        {
            var shiftEnd = DateTime.UtcNow;
            var bytes = await shiftReportService.GenerateReportAsync(_shiftStart, shiftEnd);
            await File.WriteAllBytesAsync(dlg.FileName, bytes);

            snackbarService.Show(
                "Смена завершена",
                $"Отчёт сохранён: {Path.GetFileName(dlg.FileName)}",
                ControlAppearance.Success,
                new SymbolIcon(SymbolRegular.CheckmarkCircle24),
                TimeSpan.FromSeconds(5));
        }
        catch (Exception e)
        {
            snackbarService.Show(
                "Ошибка",
                $"Не удалось сохранить отчёт: {e.Message}",
                ControlAppearance.Danger,
                new SymbolIcon(SymbolRegular.ErrorCircle24),
                TimeSpan.FromSeconds(6));
        }
    }

    private string BuildConfirmationText()
    {
        return $"""
                Итоги смены ({_shiftStart.ToLocalTime():dd.MM.yyyy}):

                  • Пополнения наличными:  {TopUpCash:N2} ₽
                  • Пополнения по карте:   {TopUpCard:N2} ₽
                  • Итого пополнений:      {TotalTopUp:N2} ₽
                  • Списания за сессии:    {TotalCharge:N2} ₽
                  • Возвраты:              {TotalRefund:N2} ₽

                  ▶ Чистая касса:          {Total:N2} ₽

                Будет сформирован Excel-отчёт.
                """;
    }
}