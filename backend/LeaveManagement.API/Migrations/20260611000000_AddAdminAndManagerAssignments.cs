using LeaveManagement.API.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveManagement.API.Migrations;

[DbContext(typeof(LeaveManagementDbContext))]
[Migration("20260611000000_AddAdminAndManagerAssignments")]
public partial class AddAdminAndManagerAssignments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(name: "CK_Employees_Role", table: "Employees");

        migrationBuilder.AddColumn<int>(
            name: "ManagerID",
            table: "Employees",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "ManagerID",
            table: "LeaveRequests",
            type: "int",
            nullable: true);

        migrationBuilder.Sql(
            """
            UPDATE lr
            SET ManagerID = mgr.EmployeeID
            FROM LeaveRequests lr
            CROSS APPLY (
                SELECT TOP 1 EmployeeID
                FROM Employees
                WHERE [Role] = N'Manager' AND IsActive = 1
                ORDER BY EmployeeID
            ) mgr
            WHERE lr.ManagerID IS NULL;
            """);

        migrationBuilder.AlterColumn<int>(
            name: "ManagerID",
            table: "LeaveRequests",
            type: "int",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.AddCheckConstraint(
            name: "CK_Employees_Role",
            table: "Employees",
            sql: "[Role] IN ('Employee', 'Manager', 'Admin')");

        migrationBuilder.CreateIndex(name: "IX_Employees_ManagerID", table: "Employees", column: "ManagerID");
        migrationBuilder.CreateIndex(name: "IX_LeaveRequests_ManagerID", table: "LeaveRequests", column: "ManagerID");

        migrationBuilder.AddForeignKey(
            name: "FK_Employees_Employees_ManagerID",
            table: "Employees",
            column: "ManagerID",
            principalTable: "Employees",
            principalColumn: "EmployeeID",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_LeaveRequests_Employees_ManagerID",
            table: "LeaveRequests",
            column: "ManagerID",
            principalTable: "Employees",
            principalColumn: "EmployeeID",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_Employees_Employees_ManagerID", table: "Employees");
        migrationBuilder.DropForeignKey(name: "FK_LeaveRequests_Employees_ManagerID", table: "LeaveRequests");
        migrationBuilder.DropIndex(name: "IX_Employees_ManagerID", table: "Employees");
        migrationBuilder.DropIndex(name: "IX_LeaveRequests_ManagerID", table: "LeaveRequests");
        migrationBuilder.DropCheckConstraint(name: "CK_Employees_Role", table: "Employees");
        migrationBuilder.DropColumn(name: "ManagerID", table: "Employees");
        migrationBuilder.DropColumn(name: "ManagerID", table: "LeaveRequests");
        migrationBuilder.AddCheckConstraint(name: "CK_Employees_Role", table: "Employees", sql: "[Role] IN ('Employee', 'Manager')");
    }
}
