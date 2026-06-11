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
        await EnsureExistingEmployeesCanSignInAsync(context, passwordService);

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
            BEGIN
                UPDATE dbo.Employees
                SET [Role] = CASE
                    WHEN LOWER(LTRIM(RTRIM([Role]))) = N'manager' THEN N'Manager'
                    ELSE N'Employee'
                END;

                IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Employees_Role' AND parent_object_id = OBJECT_ID(N'dbo.Employees'))
                    ALTER TABLE dbo.Employees ADD CONSTRAINT CK_Employees_Role CHECK ([Role] IN (N'Employee', N'Manager'));
            END
            """);
    }


    private static async Task EnsureExistingEmployeesCanSignInAsync(
        LeaveManagementDbContext context,
        PasswordService passwordService)
    {
        var employees = await context.Employees.ToListAsync();
        foreach (var employee in employees)
        {
            var changed = false;

            var normalizedRole = UserRole.Normalize(employee.Role);
            if (normalizedRole is null)
            {
                employee.Role = UserRole.Employee;
                changed = true;
            }
            else if (employee.Role != normalizedRole)
            {
                employee.Role = normalizedRole;
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(employee.PasswordHash) || string.IsNullOrWhiteSpace(employee.PasswordSalt))
            {
                var password = passwordService.HashPassword("Password123!");
                employee.PasswordHash = password.Hash;
                employee.PasswordSalt = password.Salt;
                changed = true;
            }

            if (changed)
            {
                context.Employees.Update(employee);
            }
        }
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
        var normalizedEmail = email.Trim().ToLower();
        var user = await context.Employees.FirstOrDefaultAsync(e => e.Email.ToLower() == normalizedEmail);
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

        if (!user.IsActive)
        {
            user.IsActive = true;
            changed = true;
        }

        if (changed)
        {
            context.Employees.Update(user);
        }
    }
}
