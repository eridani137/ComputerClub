using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComputerClub.Infrastructure;
using ComputerClub.Infrastructure.Entities;
using ComputerClub.Mappers;
using ComputerClub.Models;
using ComputerClub.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace ComputerClub.ViewModels.Pages;

public partial class ClientsViewModel(
    ApplicationDbContext context,
    PaymentService paymentService,
    UserManager<ComputerClubIdentity> userManager,
    ISnackbarService  snackbarService
    ) : ObservableObject
{
    public ObservableCollection<ClientItem> Clients { get; } = [];

    [ObservableProperty] private ClientItem? _selectedClient;
    [ObservableProperty] private string? _errorMessage;
    
    [RelayCommand]
    private async Task Loaded()
    {
        var clients = await userManager.Users.ToListAsync();

        foreach (var client in clients)
        {
            Clients.Add(client.Map());
        }
    }
    
    [RelayCommand]
    private void CopyLogin(string login)
    {
        if (!string.IsNullOrWhiteSpace(login))
        {
            Clipboard.SetText(login);
            snackbarService.Show("Логин скопирован", "", ControlAppearance.Success,
                new SymbolIcon(SymbolRegular.Copy24), TimeSpan.FromSeconds(3));
        }
    }
    
    [RelayCommand]
    private async Task RemoveClient(ClientItem item)
    {
        var hasActive = await context.Sessions
            .AnyAsync(s => s.ClientId == item.Id && s.Status == SessionStatus.Active);

        if (hasActive)
        {
            ErrorMessage = "Нельзя удалить клиента с активной сессией";
            return;
        }

        var entity = await userManager.FindByIdAsync(item.Id.ToString());
        if (entity is null) return;

        if (entity.UserName is not null && entity.UserName.Equals("root", StringComparison.InvariantCultureIgnoreCase))
        {
            ErrorMessage = "Нельзя удалить root пользователя";
            return;
        }

        var result = await userManager.DeleteAsync(entity);
        if (!result.Succeeded)
        {
            ErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            return;
        }

        Clients.Remove(item);
    }
    
    [RelayCommand]
    private async Task TopUpBalance(ClientItem item)
    {
        ErrorMessage = null;

        try
        {
            await paymentService.TopUp(item.Id, item.TopUpAmount);
            item.Balance += item.TopUpAmount;
            item.TopUpAmount = 1000;
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
    }
}