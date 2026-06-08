using System;
using LeaveManagement.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace LeaveManagement.API.Migrations;

[DbContext(typeof(LeaveManagementDbContext))]
partial class LeaveManagementDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.11")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

        modelBuilder.Entity("LeaveManagement.API.Models.Employee", b =>
        {
            b.Property<int>("EmployeeID")
                .ValueGeneratedOnAdd()
                .HasColumnType("int");

            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("EmployeeID"));

            b.Property<string>("Department")
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            b.Property<string>("Email")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            b.Property<string>("FirstName")
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            b.Property<bool>("IsActive")
                .ValueGeneratedOnAdd()
                .HasColumnType("bit")
                .HasDefaultValue(true);

            b.Property<string>("LastName")
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            b.HasKey("EmployeeID");
            b.HasIndex("Email").IsUnique();
            b.ToTable("Employees");
        });

        modelBuilder.Entity("LeaveManagement.API.Models.LeaveType", b =>
        {
            b.Property<int>("LeaveTypeID")
                .ValueGeneratedOnAdd()
                .HasColumnType("int");

            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("LeaveTypeID"));

            b.Property<string>("LeaveName")
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            b.HasKey("LeaveTypeID");
            b.HasIndex("LeaveName").IsUnique();
            b.ToTable("LeaveTypes");

            b.HasData(
                new { LeaveTypeID = 1, LeaveName = "Annual Leave" },
                new { LeaveTypeID = 2, LeaveName = "Sick Leave" },
                new { LeaveTypeID = 3, LeaveName = "Unpaid Leave" });
        });

        modelBuilder.Entity("LeaveManagement.API.Models.LeaveRequest", b =>
        {
            b.Property<int>("RequestID")
                .ValueGeneratedOnAdd()
                .HasColumnType("int");

            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RequestID"));

            b.Property<DateTime>("CreatedDate")
                .ValueGeneratedOnAdd()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            b.Property<int>("EmployeeID").HasColumnType("int");
            b.Property<DateOnly>("FromDate").HasColumnType("date");
            b.Property<int>("LeaveTypeID").HasColumnType("int");
            b.Property<string>("ManagerComments").HasMaxLength(500).HasColumnType("nvarchar(500)");
            b.Property<int>("NumberOfDays").HasColumnType("int");
            b.Property<string>("Reason").IsRequired().HasMaxLength(500).HasColumnType("nvarchar(500)");
            b.Property<string>("Status").IsRequired().ValueGeneratedOnAdd().HasMaxLength(20).HasColumnType("nvarchar(20)").HasDefaultValue("Pending");
            b.Property<DateOnly>("ToDate").HasColumnType("date");

            b.HasKey("RequestID");
            b.HasIndex("EmployeeID");
            b.HasIndex("LeaveTypeID");
            b.ToTable("LeaveRequests", t =>
            {
                t.HasCheckConstraint("CK_LeaveRequests_DateRange", "[ToDate] >= [FromDate]");
                t.HasCheckConstraint("CK_LeaveRequests_Status", "[Status] IN ('Pending', 'Approved', 'Rejected')");
            });
        });

        modelBuilder.Entity("LeaveManagement.API.Models.LeaveRequest", b =>
        {
            b.HasOne("LeaveManagement.API.Models.Employee", "Employee")
                .WithMany("LeaveRequests")
                .HasForeignKey("EmployeeID")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.HasOne("LeaveManagement.API.Models.LeaveType", "LeaveType")
                .WithMany("LeaveRequests")
                .HasForeignKey("LeaveTypeID")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.Navigation("Employee");
            b.Navigation("LeaveType");
        });

        modelBuilder.Entity("LeaveManagement.API.Models.Employee", b =>
        {
            b.Navigation("LeaveRequests");
        });

        modelBuilder.Entity("LeaveManagement.API.Models.LeaveType", b =>
        {
            b.Navigation("LeaveRequests");
        });
    }
}
