using LeaveManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.API.Data;

public class LeaveManagementDbContext(DbContextOptions<LeaveManagementDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => e.EmployeeCode).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<LeaveType>().HasData(
            new LeaveType { LeaveTypeID = 1, LeaveName = "Annual Leave" },
            new LeaveType { LeaveTypeID = 2, LeaveName = "Sick Leave" },
            new LeaveType { LeaveTypeID = 3, LeaveName = "Unpaid Leave" }
        );

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasOne(r => r.Employee)
                .WithMany(e => e.LeaveRequests)
                .HasForeignKey(r => r.EmployeeID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.LeaveType)
                .WithMany(t => t.LeaveRequests)
                .HasForeignKey(r => r.LeaveTypeID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(r => r.Status).HasDefaultValue(LeaveRequestStatus.Pending);
            entity.Property(r => r.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
