using LeaveManagement.API.Constants;
using LeaveManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.API.Data;

public class LeaveManagementDbContext(DbContextOptions<LeaveManagementDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeID);
            entity.Property(e => e.EmployeeID).ValueGeneratedOnAdd();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Department).IsRequired().HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.ToTable(table => table.HasCheckConstraint("CK_Employees_Department", "[Department] IN ('IT', 'HR', 'Finance', 'Marketing', 'Sales', 'Operations', 'Engineering', 'Customer Support')"));
            entity.HasData(
                new Employee { EmployeeID = -1, FirstName = "Admin", LastName = "User", Email = "admin@demo.com", Department = "IT", IsActive = true },
                new Employee { EmployeeID = -2, FirstName = "Manager", LastName = "User", Email = "manager@demo.com", Department = "Operations", IsActive = true },
                new Employee { EmployeeID = -3, FirstName = "Employee", LastName = "User", Email = "employee@demo.com", Department = "Engineering", IsActive = true }
            );
        });

        modelBuilder.Entity<LeaveType>(entity =>
        {
            entity.HasKey(lt => lt.LeaveTypeID);
            entity.Property(lt => lt.LeaveTypeID).ValueGeneratedOnAdd();
            entity.Property(lt => lt.LeaveName).IsRequired().HasMaxLength(50);
            entity.HasIndex(lt => lt.LeaveName).IsUnique();
            entity.HasData(
                new LeaveType { LeaveTypeID = 1, LeaveName = "Annual Leave" },
                new LeaveType { LeaveTypeID = 2, LeaveName = "Sick Leave" },
                new LeaveType { LeaveTypeID = 3, LeaveName = "Unpaid Leave" }
            );
        });


        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(u => u.AppUserID);
            entity.Property(u => u.AppUserID).ValueGeneratedOnAdd();
            entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(128);
            entity.Property(u => u.Role).IsRequired().HasMaxLength(20);
            entity.ToTable(table => table.HasCheckConstraint("CK_AppUsers_Role", "[Role] IN ('Admin', 'Manager', 'Employee')"));

            entity.HasOne(u => u.Employee)
                .WithOne()
                .HasForeignKey<AppUser>(u => u.EmployeeID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasData(
                new AppUser { AppUserID = 1, Email = "admin@demo.com", PasswordHash = "admin-demo-salt:0xJKYowDmRkZgbzycMBHGAWXJJKeGALbeBPiFWHmE8s=", Role = AppRoles.Admin, EmployeeID = -1 },
                new AppUser { AppUserID = 2, Email = "manager@demo.com", PasswordHash = "manager-demo-salt:d85aZJikAQ2wmK5/hJIAbAd43LGy/RzLiT2zfUYiBmc=", Role = AppRoles.Manager, EmployeeID = -2 },
                new AppUser { AppUserID = 3, Email = "employee@demo.com", PasswordHash = "employee-demo-salt:fP9vyvjXHJA08EsJkkieGeCV+OkcMSseLeG1uGkTNL0=", Role = AppRoles.Employee, EmployeeID = -3 }
            );
        });

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(lr => lr.RequestID);
            entity.Property(lr => lr.RequestID).ValueGeneratedOnAdd();
            entity.Property(lr => lr.FromDate).HasColumnType("date");
            entity.Property(lr => lr.ToDate).HasColumnType("date");
            entity.Property(lr => lr.Reason).IsRequired().HasMaxLength(500);
            entity.Property(lr => lr.Status).IsRequired().HasMaxLength(20).HasDefaultValue(LeaveStatus.Pending);
            entity.Property(lr => lr.ManagerComments).HasMaxLength(500);
            entity.Property(lr => lr.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_LeaveRequests_DateRange", "[ToDate] >= [FromDate]");
                table.HasCheckConstraint("CK_LeaveRequests_Status", "[Status] IN ('Pending', 'Approved', 'Rejected')");
            });

            entity.HasOne(lr => lr.Employee)
                .WithMany(e => e.LeaveRequests)
                .HasForeignKey(lr => lr.EmployeeID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(lr => lr.LeaveType)
                .WithMany(lt => lt.LeaveRequests)
                .HasForeignKey(lr => lr.LeaveTypeID)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
