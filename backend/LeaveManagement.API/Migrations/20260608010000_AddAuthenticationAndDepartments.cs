using LeaveManagement.API.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveManagement.API.Migrations;

[DbContext(typeof(LeaveManagementDbContext))]
[Migration("20260608010000_AddAuthenticationAndDepartments")]
public partial class AddAuthenticationAndDepartments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddCheckConstraint(
            name: "CK_Employees_Department",
            table: "Employees",
            sql: "[Department] IN ('IT', 'HR', 'Finance', 'Marketing', 'Sales', 'Operations', 'Engineering', 'Customer Support')");

        migrationBuilder.CreateTable(
            name: "AppUsers",
            columns: table => new
            {
                AppUserID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                PasswordHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                EmployeeID = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppUsers", x => x.AppUserID);
                table.CheckConstraint("CK_AppUsers_Role", "[Role] IN ('Admin', 'Manager', 'Employee')");
                table.ForeignKey(
                    name: "FK_AppUsers_Employees_EmployeeID",
                    column: x => x.EmployeeID,
                    principalTable: "Employees",
                    principalColumn: "EmployeeID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.InsertData(
            table: "Employees",
            columns: new[] { "EmployeeID", "Department", "Email", "FirstName", "IsActive", "LastName" },
            values: new object[,]
            {
                { -1, "IT", "admin@demo.com", "Admin", true, "User" },
                { -2, "Operations", "manager@demo.com", "Manager", true, "User" },
                { -3, "Engineering", "employee@demo.com", "Employee", true, "User" }
            });

        migrationBuilder.InsertData(
            table: "AppUsers",
            columns: new[] { "AppUserID", "Email", "EmployeeID", "PasswordHash", "Role" },
            values: new object[,]
            {
                { 1, "admin@demo.com", -1, "admin-demo-salt:0xJKYowDmRkZgbzycMBHGAWXJJKeGALbeBPiFWHmE8s=", "Admin" },
                { 2, "manager@demo.com", -2, "manager-demo-salt:d85aZJikAQ2wmK5/hJIAbAd43LGy/RzLiT2zfUYiBmc=", "Manager" },
                { 3, "employee@demo.com", -3, "employee-demo-salt:fP9vyvjXHJA08EsJkkieGeCV+OkcMSseLeG1uGkTNL0=", "Employee" }
            });

        migrationBuilder.CreateIndex(name: "IX_AppUsers_Email", table: "AppUsers", column: "Email", unique: true);
        migrationBuilder.CreateIndex(name: "IX_AppUsers_EmployeeID", table: "AppUsers", column: "EmployeeID", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AppUsers");

        migrationBuilder.DeleteData(table: "Employees", keyColumn: "EmployeeID", keyValue: -1);
        migrationBuilder.DeleteData(table: "Employees", keyColumn: "EmployeeID", keyValue: -2);
        migrationBuilder.DeleteData(table: "Employees", keyColumn: "EmployeeID", keyValue: -3);

        migrationBuilder.DropCheckConstraint(name: "CK_Employees_Department", table: "Employees");
    }
}
