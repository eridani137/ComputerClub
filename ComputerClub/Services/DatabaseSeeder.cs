using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ComputerClub.Services;

public class DatabaseSeeder(
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ILogger<DatabaseSeeder> logger
)
{
    public async Task Seed()
    {
        string[] roleNames = ["Admin", "User", "Guest"];
        
        foreach (var roleName in roleNames)
        {
            if (await roleManager.RoleExistsAsync(roleName)) continue;

            var role = new IdentityRole(roleName);
            var result = await roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                logger.LogInformation("Роль '{RoleName}' успешно создана", roleName);
            }
            else
            {
                logger.LogError("Ошибка создания роли '{RoleName}': {Errors}",
                    roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        const string rootUsername = "root";
        const string rootPassword = "Qwerty123_";

        var rootUser = await userManager.FindByNameAsync(rootUsername);
        if (rootUser is null)
        {
            rootUser = new IdentityUser()
            {
                UserName = rootUsername
            };

            var result = await userManager.CreateAsync(rootUser, rootPassword);
            if (!result.Succeeded)
            {
                logger.LogError("Ошибка создания root пользователя: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }

            var roleResult = await userManager.AddToRoleAsync(rootUser, "Admin");
            if (!roleResult.Succeeded)
            {
                logger.LogError("Ошибка добавления root в роль Admin: {Errors}",
                    string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }
            else
            {
                logger.LogInformation("root пользователь успешно создан с ролью Admin");
            }
        }
        else
        {
            logger.LogInformation("root пользователь уже существует");
        }
    }
}