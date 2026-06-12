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
        await EnsureAssignmentColumnsAsync(context);
        await EnsureExistingEmployeesCanSignInAsync(context, passwordService);

        await EnsureUserAsync(context, passwordService, "admin@leave.local", "Ari", "Admin", "People Ops", UserRole.Admin);
        var manager = await EnsureUserAsync(context, passwordService, "manager@leave.local", "Mia", "Manager", "Operations", UserRole.Manager);
        var employee = await EnsureUserAsync(context, passwordService, "employee@leave.local", "Evan", "Employee", "Engineering", UserRole.Employee);
        await context.SaveChangesAsync();

        employee.ManagerID ??= manager.EmployeeID;
        await AssignExistingEmployeesToDefaultManagerAsync(context, manager.EmployeeID);
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
                IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Employees_Role' AND parent_object_id = OBJECT_ID(N'dbo.Employees'))
                    ALTER TABLE dbo.Employees DROP CONSTRAINT CK_Employees_Role;

                UPDATE dbo.Employees
                SET [Role] = CASE
                    WHEN LOWER(LTRIM(RTRIM([Role]))) = N'manager' THEN N'Manager'
                    WHEN LOWER(LTRIM(RTRIM([Role]))) = N'admin' THEN N'Admin'
                    ELSE N'Employee'
                END;

                IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Employees_Role' AND parent_object_id = OBJECT_ID(N'dbo.Employees'))
                    ALTER TABLE dbo.Employees ADD CONSTRAINT CK_Employees_Role CHECK ([Role] IN (N'Employee', N'Manager', N'Admin'));
            END
            """);
    }

    private static async Task EnsureAssignmentColumnsAsync(LeaveManagementDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'dbo.Employees', N'U') IS NOT NULL
                AND COL_LENGTH(N'dbo.Employees', N'ManagerID') IS NULL
            BEGIN
                ALTER TABLE dbo.Employees ADD ManagerID int NULL;
            END
            """);

        await context.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'dbo.LeaveRequests', N'U') IS NOT NULL
                AND COL_LENGTH(N'dbo.LeaveRequests', N'ManagerID') IS NULL
            BEGIN
                ALTER TABLE dbo.LeaveRequests ADD ManagerID int NULL;
            END
            """);
    }

    private static async Task AssignExistingEmployeesToDefaultManagerAsync(LeaveManagementDbContext context, int managerId)
    {
        var employees = await context.Employees
            .Where(e => e.Role == UserRole.Employee && e.ManagerID == null)
            .ToListAsync();
        foreach (var employee in employees)
        {
            employee.ManagerID = managerId;
        }

        await context.SaveChangesAsync();

        await context.Database.ExecuteSqlRawAsync(
            """
            UPDATE lr
            SET ManagerID = e.ManagerID
            FROM dbo.LeaveRequests lr
            INNER JOIN dbo.Employees e ON lr.EmployeeID = e.EmployeeID
            WHERE lr.ManagerID IS NULL AND e.ManagerID IS NOT NULL;
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

    private static async Task<Employee> EnsureUserAsync(
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
            var employee = new Employee
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Department = department,
                Role = role,
                PasswordHash = password.Hash,
                PasswordSalt = password.Salt,
                IsActive = true
            };
            context.Employees.Add(employee);
            return employee;
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

        return user;
    }
}
