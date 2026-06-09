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

        await EnsureAuthenticationColumnsAsync(context);

        await EnsureUserAsync(context, passwordService, "manager@leave.local", "Mia", "Manager", "Operations", UserRole.Manager);
        await EnsureUserAsync(context, passwordService, "employee@leave.local", "Evan", "Employee", "Engineering", UserRole.Employee);
        await context.SaveChangesAsync();
    }

    private static async Task EnsureAuthenticationColumnsAsync(LeaveManagementDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'dbo.Employees', N'U') IS NOT NULL
                AND COL_LENGTH(N'dbo.Employees', N'Role') IS NULL
            BEGIN
                ALTER TABLE dbo.Employees ADD [Role] nvarchar(20) NOT NULL CONSTRAINT DF_Employees_Role DEFAULT N'Employee';
            END
            """);

        await context.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'dbo.Employees', N'U') IS NOT NULL
            BEGIN
                IF COL_LENGTH(N'dbo.Employees', N'PasswordHash') IS NULL
                    ALTER TABLE dbo.Employees ADD PasswordHash nvarchar(max) NOT NULL CONSTRAINT DF_Employees_PasswordHash DEFAULT N'';

                IF COL_LENGTH(N'dbo.Employees', N'PasswordSalt') IS NULL
                    ALTER TABLE dbo.Employees ADD PasswordSalt nvarchar(max) NOT NULL CONSTRAINT DF_Employees_PasswordSalt DEFAULT N'';
            END
            """);

        await context.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'dbo.Employees', N'U') IS NOT NULL
                AND COL_LENGTH(N'dbo.Employees', N'Role') IS NOT NULL
                AND NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Employees_Role' AND parent_object_id = OBJECT_ID(N'dbo.Employees'))
            BEGIN
                ALTER TABLE dbo.Employees ADD CONSTRAINT CK_Employees_Role CHECK ([Role] IN (N'Employee', N'Manager'));
            END
            """);
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
