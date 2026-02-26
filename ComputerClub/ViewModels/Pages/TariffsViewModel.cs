using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComputerClub.Infrastructure;
using ComputerClub.Infrastructure.Entities;
using ComputerClub.Mappers;
using ComputerClub.Models;
using Microsoft.EntityFrameworkCore;

namespace ComputerClub.ViewModels.Pages;

public partial class TariffsViewModel(ApplicationDbContext context) : ObservableObject
{
    public ObservableCollection<TariffItem> Tariffs { get; } = [];

    [ObservableProperty] private ObservableCollection<ComputerTypeDefinition> _availableTypes = [];
    
    [ObservableProperty] private string _newName = string.Empty;
    [ObservableProperty] private decimal _newPricePerHour;
    [ObservableProperty] private ComputerTypeDefinition? _newComputerType;
    [ObservableProperty] private string? _errorMessage;

    [RelayCommand]
    private async Task Loaded()
    {
        var entities = await context.Tariffs.ToListAsync();
        foreach (var e in entities)
        {
            Tariffs.Add(e.Map());
        }
        
        RefreshAvailableTypes();
    }
    
    [RelayCommand]
    private async Task AddTariff()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(NewName))
        {
            ErrorMessage = "Введите название тарифа";
            return;
        }

        if (NewPricePerHour <= 0)
        {
            ErrorMessage = "Цена должна быть больше нуля";
            return;
        }

        if (NewComputerType is null)
        {
            ErrorMessage = "Выберите тип компьютера";
            return;
        }

        var exists = await context.Tariffs.AnyAsync(t => t.ComputerTypeId == NewComputerType.Id);
        if (exists)
        {
            ErrorMessage = $"Тариф для типа '{NewComputerType.Name}' уже существует";
            return;
        }

        var entity = new TariffEntity
        {
            Name = NewName.Trim(),
            PricePerHour = NewPricePerHour,
            ComputerTypeId = NewComputerType.Id
        };

        context.Tariffs.Add(entity);
        await context.SaveChangesAsync();

        Tariffs.Add(entity.Map());
        RefreshAvailableTypes();

        NewName = string.Empty;
        NewPricePerHour = 0;
        NewComputerType = null;
    }
    
    [RelayCommand]
    private async Task RemoveTariff(TariffItem item)
    {
        var hasActive = await context.Sessions
            .AnyAsync(s => s.TariffId == item.Id && s.Status == SessionStatus.Active);

        if (hasActive)
        {
            ErrorMessage = "Тариф используется в активной сессии";
            return;
        }

        var entity = await context.Tariffs.FindAsync(item.Id);
        if (entity is null) return;

        context.Tariffs.Remove(entity);
        await context.SaveChangesAsync();

        Tariffs.Remove(item);
        RefreshAvailableTypes();
    }
    
    [RelayCommand]
    private async Task SaveTariff(TariffItem item)
    {
        ErrorMessage = null;

        var entity = await context.Tariffs.FindAsync(item.Id);
        if (entity is null) return;

        entity.Name = item.Name;
        entity.PricePerHour = item.PricePerHour;

        await context.SaveChangesAsync();
    }
    
    private void RefreshAvailableTypes()
    {
        var usedTypeIds = Tariffs.Select(t => t.ComputerTypeId).ToHashSet();

        AvailableTypes = new ObservableCollection<ComputerTypeDefinition>(
            ComputerTypes.All.Where(t => t.Id != 0 && !usedTypeIds.Contains(t.Id))
        );
    }
}