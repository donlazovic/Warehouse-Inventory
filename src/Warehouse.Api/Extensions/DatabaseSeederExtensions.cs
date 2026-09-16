using Microsoft.EntityFrameworkCore;
using Warehouse.DataLayer.Context;
using Warehouse.Domain.Entities.Identity;

namespace Warehouse.Api.Extensions;

public static class DatabaseSeederExtensions
{
    public static async Task SeedAdminUserAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();

        if (await context.Users.AnyAsync())
            return;

        var adminRole = await context.Roles.FirstAsync(x => x.Name == "Admin");

        context.Users.Add(new User
        {
            FirstName = "Sistem",
            LastName = "Administrator",
            Email = "admin@warehouse.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            IsActive = true,
            RoleId = adminRole.Id
        });

        await context.SaveChangesAsync();
    }
}
