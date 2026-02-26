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

namespace ComputerClub.ViewModels.Pages;

public partial class ClientsViewModel(
    ApplicationDbContext context,
    SessionService sessionService,
    UserManager<ComputerClubIdentity> userManager
    ) : ObservableObject
{
    public ObservableCollection<ClientItem> Clients { get; } = [];

    [ObservableProperty] private ClientItem? _selectedClient;
    [ObservableProperty] private decimal _topUpAmount = 1000;
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
            await sessionService.TopUpBalance(item.Id, TopUpAmount);
            item.Balance += TopUpAmount;
            TopUpAmount = 1000;
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
    }
}