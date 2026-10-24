using AutoHub.Models;
using AutoHub.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AutoHub.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in RoleNames.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var email = config["SuperAdmin:Email"];
        var password = config["SuperAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        email = email.Trim();
        var userName = email.Split('@')[0].Trim();

        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            if (!string.IsNullOrWhiteSpace(userName) &&
                !string.Equals(existing.UserName, userName, StringComparison.OrdinalIgnoreCase))
            {
                existing.UserName = userName;
                await userManager.UpdateAsync(existing);
            }
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            FullName = config["SuperAdmin:FullName"] ?? "Super Administrator",
            EmailConfirmed = true,
        };

        var created = await userManager.CreateAsync(admin, password);
        if (created.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, RoleNames.SuperAdmin);
            await userManager.AddToRoleAsync(admin, RoleNames.Administrator);
            await userManager.AddToRoleAsync(admin, RoleNames.Manager);
        }
    }
}
