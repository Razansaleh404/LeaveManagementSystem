using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LeaveManagement.API.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Employees",
            columns: table => new
            {
                EmployeeID = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Department = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
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
                LeaveName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
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
                FromDate = table.Column<DateOnly>(type: "date", nullable: false),
                ToDate = table.Column<DateOnly>(type: "date", nullable: false),
                NumberOfDays = table.Column<int>(type: "integer", nullable: false),
                Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                ManagerComments = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
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

        migrationBuilder.CreateIndex(name: "IX_Employees_Email", table: "Employees", column: "Email", unique: true);
        migrationBuilder.CreateIndex(name: "IX_LeaveRequests_EmployeeID", table: "LeaveRequests", column: "EmployeeID");
        migrationBuilder.CreateIndex(name: "IX_LeaveRequests_LeaveTypeID", table: "LeaveRequests", column: "LeaveTypeID");
        migrationBuilder.CreateIndex(name: "IX_LeaveTypes_LeaveName", table: "LeaveTypes", column: "LeaveName", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "LeaveRequests");
        migrationBuilder.DropTable(name: "Employees");
        migrationBuilder.DropTable(name: "LeaveTypes");
    }
}
