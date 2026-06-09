using LeaveManagement.API.Models;
using LeaveManagement.API.Services;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.API.Data;

public static class DatabaseSeeder
{
    public static async Task SeedDemoUsersAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LeaveManagementDbContext>();
        var passwordService = scope.ServiceProvider.GetRequiredService<PasswordService>();

        if (!await context.Database.CanConnectAsync())
        {
            return;
        }

        await EnsureUserAsync(context, passwordService, "manager@leave.local", "Mia", "Manager", "Operations", UserRole.Manager);
        await EnsureUserAsync(context, passwordService, "employee@leave.local", "Evan", "Employee", "Engineering", UserRole.Employee);
        await context.SaveChangesAsync();
    }

    private static async Task EnsureUserAsync(
        LeaveManagementDbContext context,
        PasswordService passwordService,
        string email,
        string firstName,
        string lastName,
        string department,
        string role)
    {
        var user = await context.Employees.FirstOrDefaultAsync(e => e.Email == email);
        if (user is null)
        {
            var password = passwordService.HashPassword("Password123!");
            context.Employees.Add(new Employee
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Department = department,
                Role = role,
                PasswordHash = password.Hash,
                PasswordSalt = password.Salt,
                IsActive = true
            });
            return;
        }

        var changed = false;
        if (string.IsNullOrWhiteSpace(user.PasswordHash) || string.IsNullOrWhiteSpace(user.PasswordSalt))
        {
            var password = passwordService.HashPassword("Password123!");
            user.PasswordHash = password.Hash;
            user.PasswordSalt = password.Salt;
            changed = true;
        }

        if (user.Role != role)
        {
            user.Role = role;
            changed = true;
        }

        if (changed)
        {
            context.Employees.Update(user);
        }
    }
}
