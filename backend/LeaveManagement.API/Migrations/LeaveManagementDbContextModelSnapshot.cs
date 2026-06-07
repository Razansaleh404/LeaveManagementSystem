using LeaveManagement.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LeaveManagement.API.Migrations;

[DbContext(typeof(LeaveManagementDbContext))]
partial class LeaveManagementDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.11")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        modelBuilder.UseIdentityByDefaultColumns();

        modelBuilder.Entity("LeaveManagement.API.Models.Employee", b =>
        {
            b.Property<int>("EmployeeID")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            b.Property<string>("Department")
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            b.Property<string>("Email")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            b.Property<string>("EmployeeCode")
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("varchar(20)");

            b.Property<string>("FullName")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            b.Property<bool>("IsActive")
                .HasColumnType("boolean");

            b.HasKey("EmployeeID");

            b.HasIndex("Email")
                .IsUnique();

            b.HasIndex("EmployeeCode")
                .IsUnique();

            b.ToTable("Employees");
        });

        modelBuilder.Entity("LeaveManagement.API.Models.LeaveRequest", b =>
        {
            b.Property<int>("RequestID")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            b.Property<DateTime>("CreatedDate")
                .ValueGeneratedOnAdd()
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            b.Property<int>("EmployeeID")
                .HasColumnType("integer");

            b.Property<DateTime>("FromDate")
                .HasColumnType("date");

            b.Property<int>("LeaveTypeID")
                .HasColumnType("integer");

            b.Property<string>("ManagerComments")
                .HasMaxLength(500)
                .HasColumnType("varchar(500)");

            b.Property<int>("NumberOfDays")
                .HasColumnType("integer");

            b.Property<string>("Reason")
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("varchar(500)");

            b.Property<string>("Status")
                .IsRequired()
                .ValueGeneratedOnAdd()
                .HasMaxLength(20)
                .HasColumnType("varchar(20)")
                .HasDefaultValue("Pending");

            b.Property<DateTime>("ToDate")
                .HasColumnType("date");

            b.HasKey("RequestID");

            b.HasIndex("EmployeeID");

            b.HasIndex("LeaveTypeID");

            b.ToTable("LeaveRequests");
        });

        modelBuilder.Entity("LeaveManagement.API.Models.LeaveType", b =>
        {
            b.Property<int>("LeaveTypeID")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            b.Property<string>("LeaveName")
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            b.HasKey("LeaveTypeID");

            b.ToTable("LeaveTypes");

            b.HasData(
                new { LeaveTypeID = 1, LeaveName = "Annual Leave" },
                new { LeaveTypeID = 2, LeaveName = "Sick Leave" },
                new { LeaveTypeID = 3, LeaveName = "Unpaid Leave" });
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
