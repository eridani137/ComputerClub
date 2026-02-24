using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComputerClub.Infrastructure;
using ComputerClub.Infrastructure.Entities;
using ComputerClub.Mappers;
using ComputerClub.Models;
using ComputerClub.Services;
using Microsoft.EntityFrameworkCore;

namespace ComputerClub.ViewModels.Pages;

public partial class ClientsViewModel(ApplicationDbContext context, SessionService sessionService) : ObservableObject
{
    public ObservableCollection<ClientItem> Clients { get; } = [];

    [ObservableProperty] private ClientItem? _selectedClient;
    [ObservableProperty] private string _newFullName = string.Empty;
    [ObservableProperty] private string _newPhone = string.Empty;
    [ObservableProperty] private decimal _topUpAmount;
    [ObservableProperty] private string? _errorMessage;
    
    [RelayCommand]
    private async Task AddClient()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(NewFullName))
        {
            ErrorMessage = "Введите имя клиента";
            return;
        }

        var entity = new ClientEntity
        {
            FullName = NewFullName.Trim(),
            Phone = NewPhone.Trim(),
            Balance = 0
        };

        context.Clients.Add(entity);
        await context.SaveChangesAsync();

        Clients.Add(entity.Map());

        NewFullName = string.Empty;
        NewPhone = string.Empty;
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

        var entity = await context.Clients.FindAsync(item.Id);
        if (entity is null) return;

        context.Clients.Remove(entity);
        await context.SaveChangesAsync();

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
            TopUpAmount = 0;
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
    }
}