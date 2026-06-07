using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LeaveManagement.API.Migrations;

public partial class InitialPostgreSql : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Employees",
            columns: table => new
            {
                EmployeeID = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                EmployeeCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                FullName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                Department = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Employees", x => x.EmployeeID);
            });

        migrationBuilder.CreateTable(
            name: "LeaveTypes",
            columns: table => new
            {
                LeaveTypeID = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                LeaveName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LeaveTypes", x => x.LeaveTypeID);
            });

        migrationBuilder.CreateTable(
            name: "LeaveRequests",
            columns: table => new
            {
                RequestID = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                EmployeeID = table.Column<int>(type: "integer", nullable: false),
                LeaveTypeID = table.Column<int>(type: "integer", nullable: false),
                FromDate = table.Column<DateTime>(type: "date", nullable: false),
                ToDate = table.Column<DateTime>(type: "date", nullable: false),
                NumberOfDays = table.Column<int>(type: "integer", nullable: false),
                Reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                ManagerComments = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LeaveRequests", x => x.RequestID);
                table.ForeignKey(
                    name: "FK_LeaveRequests_Employees_EmployeeID",
                    column: x => x.EmployeeID,
                    principalTable: "Employees",
                    principalColumn: "EmployeeID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_LeaveRequests_LeaveTypes_LeaveTypeID",
                    column: x => x.LeaveTypeID,
                    principalTable: "LeaveTypes",
                    principalColumn: "LeaveTypeID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.InsertData(
            table: "LeaveTypes",
            columns: new[] { "LeaveTypeID", "LeaveName" },
            values: new object[,]
            {
                { 1, "Annual Leave" },
                { 2, "Sick Leave" },
                { 3, "Unpaid Leave" }
            });

        migrationBuilder.CreateIndex(
            name: "IX_Employees_Email",
            table: "Employees",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Employees_EmployeeCode",
            table: "Employees",
            column: "EmployeeCode",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_LeaveRequests_EmployeeID",
            table: "LeaveRequests",
            column: "EmployeeID");

        migrationBuilder.CreateIndex(
            name: "IX_LeaveRequests_LeaveTypeID",
            table: "LeaveRequests",
            column: "LeaveTypeID");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "LeaveRequests");
        migrationBuilder.DropTable(name: "Employees");
        migrationBuilder.DropTable(name: "LeaveTypes");
    }
}
