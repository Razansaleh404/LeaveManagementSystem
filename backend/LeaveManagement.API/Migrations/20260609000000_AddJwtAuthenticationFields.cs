using LeaveManagement.API.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveManagement.API.Migrations;

[DbContext(typeof(LeaveManagementDbContext))]
[Migration("20260609000000_AddJwtAuthenticationFields")]
public partial class AddJwtAuthenticationFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PasswordHash",
            table: "Employees",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "PasswordSalt",
            table: "Employees",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "Role",
            table: "Employees",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "Employee");

        migrationBuilder.AddCheckConstraint(
            name: "CK_Employees_Role",
            table: "Employees",
            sql: "[Role] IN ('Employee', 'Manager')");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(name: "CK_Employees_Role", table: "Employees");
        migrationBuilder.DropColumn(name: "PasswordHash", table: "Employees");
        migrationBuilder.DropColumn(name: "PasswordSalt", table: "Employees");
        migrationBuilder.DropColumn(name: "Role", table: "Employees");
    }
}
