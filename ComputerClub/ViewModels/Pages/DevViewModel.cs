using Bogus;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComputerClub.Infrastructure.Entities;
using ComputerClub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ComputerClub.ViewModels.Pages;

public partial class DevViewModel(
    UserManager<ComputerClubIdentity> userManager,
    RoleManager<IdentityRole<int>> roleManager,
    ILogger<DevViewModel> logger
) : ObservableObject
{
    private readonly Faker<ClientItem> _faker = new Faker<ClientItem>("ru")
        .RuleFor(c => c.FullName, f => f.Name.FullName())
        .RuleFor(u => u.Login, f => f.Internet.UserName())
        .RuleFor(c => c.Password, f => "Qwerty123!")
        .RuleFor(c => c.PhoneNumber, f => f.Phone.PhoneNumber("+7 (###) ###-##-##"))
        .RuleFor(c => c.Balance, f => f.Finance.Amount(0, 5000));


    [RelayCommand]
    private async Task AddRandomClient()
    {
        var clientItem = _faker.Generate();

        var identityUser = new ComputerClubIdentity
        {
            UserName = clientItem.Login,
            FullName = clientItem.FullName,
            Balance = clientItem.Balance,
            PhoneNumber = clientItem.PhoneNumber,
        };

        var result = await userManager.CreateAsync(identityUser, clientItem.Password);

        if (!result.Succeeded)
        {
            logger.LogError("Ошибки при создании пользователя: {ErrorDescriptions}",
                string.Join(", ", result.Errors.Select(x => x.Description)));
            return;
        }
        
        var roleResult = await userManager.AddToRoleAsync(identityUser, "User");
        if (!roleResult.Succeeded)
        {
            logger.LogError("Ошибка добавления root в роль Admin: {Errors}",
                string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            return;
        }

        logger.LogInformation("Пользователь {ClientFullName} создан", clientItem.FullName);
    }
}