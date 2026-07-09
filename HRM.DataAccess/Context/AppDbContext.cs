using System;
using HRM.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRM.DataAccess.Contexts;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attendance> Attendances { get; set; } = null!;

    public virtual DbSet<AttendanceAdjustment> AttendanceAdjustments { get; set; } = null!;

    public virtual DbSet<Contract> Contracts { get; set; } = null!;

    public virtual DbSet<Department> Departments { get; set; } = null!;

    public virtual DbSet<Employee> Employees { get; set; } = null!;

    public virtual DbSet<EmployeeAssignment> EmployeeAssignments { get; set; } = null!;

    public virtual DbSet<EmployeeBankAccount> EmployeeBankAccounts { get; set; } = null!;

    public virtual DbSet<EmployeeCertificate> EmployeeCertificates { get; set; } = null!;

    public virtual DbSet<EmployeeContact> EmployeeContacts { get; set; } = null!;

    public virtual DbSet<EmployeeEducation> EmployeeEducations { get; set; } = null!;

    public virtual DbSet<EmployeeExperience> EmployeeExperiences { get; set; } = null!;

    public virtual DbSet<Evaluation> Evaluations { get; set; } = null!;

    public virtual DbSet<Kpi> Kpis { get; set; } = null!;

    public virtual DbSet<Kpiassignment> Kpiassignments { get; set; } = null!;

    public virtual DbSet<LeaveBalance> LeaveBalances { get; set; } = null!;

    public virtual DbSet<LeaveRequest> LeaveRequests { get; set; } = null!;

    public virtual DbSet<LeaveType> LeaveTypes { get; set; } = null!;

    public virtual DbSet<Notification> Notifications { get; set; } = null!;

    public virtual DbSet<NotificationRecipient> NotificationRecipients { get; set; } = null!;

    public virtual DbSet<OvertimeRequest> OvertimeRequests { get; set; } = null!;

    public virtual DbSet<Payroll> Payrolls { get; set; } = null!;

    public virtual DbSet<PayrollDetail> PayrollDetails { get; set; } = null!;

    public virtual DbSet<Position> Positions { get; set; } = null!;

    public virtual DbSet<Role> Roles { get; set; } = null!;

    public virtual DbSet<User> Users { get; set; } = null!;

    public virtual DbSet<UserRole> UserRoles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK__Attendan__8B69261CBA8FA015");

            entity.HasIndex(e => e.AttendanceDate, "IX_Attendances_Date");

            entity.HasIndex(e => e.EmployeeId, "IX_Attendances_Employee");

            entity.HasIndex(e => new { e.EmployeeId, e.AttendanceDate }, "UQ_Attendance_Daily").IsUnique();

            entity.Property(e => e.CheckInTime).HasColumnType("datetime");
            entity.Property(e => e.CheckOutTime).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("PRESENT");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.WorkingHours).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Employee).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attendances_Employee");
        });

        modelBuilder.Entity<AttendanceAdjustment>(entity =>
        {
            entity.HasKey(e => e.AdjustmentId).HasName("PK__Attendan__E60DB893DC133A7B");

            entity.HasIndex(e => e.Status, "IX_AttendanceAdjustments_Status");

            entity.Property(e => e.ApprovedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.RejectionReason).HasMaxLength(500);
            entity.Property(e => e.RequestedCheckInTime).HasColumnType("datetime");
            entity.Property(e => e.RequestedCheckOutTime).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("PENDING");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.AttendanceAdjustmentApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK_AttendanceAdjustments_ApprovedBy");

            entity.HasOne(d => d.Attendance).WithMany(p => p.AttendanceAdjustments)
                .HasForeignKey(d => d.AttendanceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AttendanceAdjustments_Attendance");

            entity.HasOne(d => d.RequestedByNavigation).WithMany(p => p.AttendanceAdjustmentRequestedByNavigations)
                .HasForeignKey(d => d.RequestedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AttendanceAdjustments_RequestedBy");
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.ContractId).HasName("PK__Contract__C90D3469FC7B21E3");

            entity.HasIndex(e => e.EmployeeId, "IX_Contracts_Employee");

            entity.HasIndex(e => e.EndDate, "IX_Contracts_EndDate");

            entity.HasIndex(e => e.EmployeeId, "UQ_Contracts_ActiveContract")
                .IsUnique()
                .HasFilter("([Status]='ACTIVE')");

            entity.Property(e => e.ContractFileUrl).HasMaxLength(500);
            entity.Property(e => e.ContractType).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.Salary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVE");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Employee).WithOne(p => p.Contract)
                .HasForeignKey<Contract>(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Contracts_Employee");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__B2079BED7A800761");

            entity.HasIndex(e => e.DepartmentName, "UQ_Departments_DepartmentName").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DepartmentName).HasMaxLength(150);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.ManagerEmployee).WithMany(p => p.Departments)
                .HasForeignKey(d => d.ManagerEmployeeId)
                .HasConstraintName("FK_Departments_ManagerEmployee");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__Employee__7AD04F1133B4119F");

            entity.HasIndex(e => e.FullName, "IX_Employees_FullName");

            entity.HasIndex(e => e.ManagerId, "IX_Employees_ManagerId");

            entity.HasIndex(e => e.EmploymentStatus, "IX_Employees_Status");

            entity.HasIndex(e => e.Cccd, "UQ_Employees_CCCD_NotNull")
                .IsUnique()
                .HasFilter("([CCCD] IS NOT NULL)");

            entity.HasIndex(e => e.Email, "UQ_Employees_Email").IsUnique();

            entity.HasIndex(e => e.EmployeeCode, "UQ_Employees_EmployeeCode").IsUnique();

            entity.HasIndex(e => e.UserId, "UQ_Employees_UserId_NotNull")
                .IsUnique()
                .HasFilter("([UserId] IS NOT NULL)");

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.Cccd)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("CCCD");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.EmployeeCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.EmploymentStatus)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVE");
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.Gender)
                .HasConversion(
                    v => v == "1"
                        || v.ToLower() == "true"
                        || v.ToLower() == "male"
                        || v.ToLower() == "m"
                        || v.ToLower() == "nam",
                    v => v ? "Male" : "Female");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Manager).WithMany(p => p.InverseManager)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK_Employees_Manager");

            entity.HasOne(d => d.User).WithOne(p => p.Employee)
                .HasForeignKey<Employee>(d => d.UserId)
                .HasConstraintName("FK_Employees_User");
        });

        modelBuilder.Entity<EmployeeAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__Employee__32499E77C30D820C");

            entity.HasIndex(e => e.DepartmentId, "IX_EmployeeAssignments_Department");

            entity.HasIndex(e => e.EmployeeId, "IX_EmployeeAssignments_Employee");

            entity.HasIndex(e => e.PositionId, "IX_EmployeeAssignments_Position");

            entity.HasIndex(e => e.EmployeeId, "UQ_EmployeeAssignments_Current")
                .IsUnique()
                .HasFilter("([EndDate] IS NULL)");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(500);

            entity.HasOne(d => d.Department).WithMany(p => p.EmployeeAssignments)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeAssignments_Department");

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeAssignments)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeAssignments_Employee");

            entity.HasOne(d => d.Position).WithMany(p => p.EmployeeAssignments)
                .HasForeignKey(d => d.PositionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeAssignments_Position");
        });

        modelBuilder.Entity<EmployeeBankAccount>(entity =>
        {
            entity.HasKey(e => e.BankAccountId).HasName("PK__Employee__4FC8E4A19E48EA99");

            entity.HasIndex(e => e.AccountNumber, "UQ_EmployeeBankAccounts_AccountNumber_NotNull")
                .IsUnique()
                .HasFilter("([AccountNumber] IS NOT NULL)");

            entity.Property(e => e.AccountHolder).HasMaxLength(150);
            entity.Property(e => e.AccountNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BankName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsPrimary).HasDefaultValue(true);

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeBankAccounts)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeBankAccounts_Employee");
        });

        modelBuilder.Entity<EmployeeCertificate>(entity =>
        {
            entity.HasKey(e => e.CertificateId).HasName("PK__Employee__BBF8A7C1B8D3A61B");

            entity.Property(e => e.CertificateName).HasMaxLength(200);
            entity.Property(e => e.CertificateNumber)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IssuedBy).HasMaxLength(200);

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeCertificates)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeCertificates_Employee");
        });

        modelBuilder.Entity<EmployeeContact>(entity =>
        {
            entity.HasKey(e => e.ContactId).HasName("PK__Employee__5C66259B2383F947");

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.ContactName).HasMaxLength(150);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Relationship).HasMaxLength(50);

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeContacts)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeContacts_Employee");
        });

        modelBuilder.Entity<EmployeeEducation>(entity =>
        {
            entity.HasKey(e => e.EducationId).HasName("PK__Employee__4BBE3805BB23A41F");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Degree).HasMaxLength(100);
            entity.Property(e => e.Gpa)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("GPA");
            entity.Property(e => e.Major).HasMaxLength(100);
            entity.Property(e => e.SchoolName).HasMaxLength(200);

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeEducations)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeEducations_Employee");
        });

        modelBuilder.Entity<EmployeeExperience>(entity =>
        {
            entity.HasKey(e => e.ExperienceId).HasName("PK__Employee__2F4E3449A081D665");

            entity.Property(e => e.CompanyName).HasMaxLength(200);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Position).HasMaxLength(150);

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeExperiences)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeExperiences_Employee");
        });

        modelBuilder.Entity<Evaluation>(entity =>
        {
            entity.HasKey(e => e.EvaluationId).HasName("PK__Evaluati__36AE68F351324F5E");

            entity.HasIndex(e => e.EmployeeId, "IX_Evaluations_Employee");

            entity.Property(e => e.Comment).HasMaxLength(1000);
            entity.Property(e => e.EvaluationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EvaluationPeriod)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("DRAFT");
            entity.Property(e => e.TotalScore).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Employee).WithMany(p => p.EvaluationEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Evaluations_Employee");

            entity.HasOne(d => d.Evaluator).WithMany(p => p.EvaluationEvaluators)
                .HasForeignKey(d => d.EvaluatorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Evaluations_Evaluator");
        });

        modelBuilder.Entity<Kpi>(entity =>
        {
            entity.HasKey(e => e.Kpiid).HasName("PK__KPIs__72E692A118A5955D");

            entity.ToTable("KPIs");

            entity.Property(e => e.Kpiid).HasColumnName("KPIId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Kpiname)
                .HasMaxLength(200)
                .HasColumnName("KPIName");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.Weight).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Kpis)
                .HasForeignKey(d => d.CreatedByUserId)
                .HasConstraintName("FK_KPIs_CreatedByUser");
        });

        modelBuilder.Entity<Kpiassignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__KPIAssig__32499E779C331F9C");

            entity.ToTable("KPIAssignments");

            entity.HasIndex(e => e.EmployeeId, "IX_KPIAssignments_Employee");

            entity.HasIndex(e => e.Status, "IX_KPIAssignments_Status");

            entity.Property(e => e.ActualValue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EmployeeComment).HasMaxLength(1000);
            entity.Property(e => e.EmployeeSelfScore).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Kpiid).HasColumnName("KPIId");
            entity.Property(e => e.ManagerComment).HasMaxLength(1000);
            entity.Property(e => e.ManagerScore).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ProgressPercent).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ReviewedAt).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("ASSIGNED");
            entity.Property(e => e.TargetValue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.KpiassignmentAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .HasConstraintName("FK_KPIAssignments_AssignedBy");

            entity.HasOne(d => d.Employee).WithMany(p => p.KpiassignmentEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KPIAssignments_Employee");

            entity.HasOne(d => d.Kpi).WithMany(p => p.Kpiassignments)
                .HasForeignKey(d => d.Kpiid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KPIAssignments_KPI");

            entity.HasOne(d => d.ReviewedByNavigation).WithMany(p => p.KpiassignmentReviewedByNavigations)
                .HasForeignKey(d => d.ReviewedBy)
                .HasConstraintName("FK_KPIAssignments_ReviewedBy");
        });

        modelBuilder.Entity<LeaveBalance>(entity =>
        {
            entity.HasKey(e => e.LeaveBalanceId).HasName("PK__LeaveBal__8A68C4A2A654BCBA");

            entity.HasIndex(e => new { e.EmployeeId, e.LeaveTypeId, e.Year }, "UQ_LeaveBalances_PerYear").IsUnique();

            entity.Property(e => e.RemainingDays).HasComputedColumnSql("([TotalDays]-[UsedDays])", true);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Employee).WithMany(p => p.LeaveBalances)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LeaveBalances_Employee");

            entity.HasOne(d => d.LeaveType).WithMany(p => p.LeaveBalances)
                .HasForeignKey(d => d.LeaveTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LeaveBalances_LeaveType");
        });

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(e => e.LeaveRequestId).HasName("PK__LeaveReq__609421EEF2B11527");

            entity.HasIndex(e => new { e.StartDate, e.EndDate }, "IX_LeaveRequests_Date");

            entity.HasIndex(e => e.EmployeeId, "IX_LeaveRequests_Employee");

            entity.HasIndex(e => e.Status, "IX_LeaveRequests_Status");

            entity.Property(e => e.ApprovedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Reason).HasMaxLength(1000);
            entity.Property(e => e.RejectionReason).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("PENDING");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.LeaveRequestApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK_LeaveRequests_ApprovedBy");

            entity.HasOne(d => d.Employee).WithMany(p => p.LeaveRequestEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LeaveRequests_Employee");

            entity.HasOne(d => d.LeaveType).WithMany(p => p.LeaveRequests)
                .HasForeignKey(d => d.LeaveTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LeaveRequests_LeaveType");
        });

        modelBuilder.Entity<LeaveType>(entity =>
        {
            entity.HasKey(e => e.LeaveTypeId).HasName("PK__LeaveTyp__43BE8F14CE515491");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LeaveTypeName).HasMaxLength(100);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E1205F3091A");

            entity.HasIndex(e => e.CreatedAt, "IX_Notifications_CreatedAt");

            entity.HasIndex(e => e.NotificationType, "IX_Notifications_Type");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NotificationType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("GENERAL");
            entity.Property(e => e.TargetType)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("USER");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.CreatedByUserId)
                .HasConstraintName("FK_Notifications_CreatedByUser");
        });

        modelBuilder.Entity<NotificationRecipient>(entity =>
        {
            entity.HasKey(e => e.RecipientId).HasName("PK__Notifica__F0A6024D9926DC3C");

            entity.HasIndex(e => new { e.UserId, e.IsRead }, "IX_NotificationRecipients_User_Read");

            entity.HasIndex(e => new { e.NotificationId, e.UserId }, "UQ_NotificationRecipients").IsUnique();

            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.ReadAt).HasColumnType("datetime");

            entity.HasOne(d => d.Notification).WithMany(p => p.NotificationRecipients)
                .HasForeignKey(d => d.NotificationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NotificationRecipients_Notification");

            entity.HasOne(d => d.User).WithMany(p => p.NotificationRecipients)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NotificationRecipients_User");
        });

        modelBuilder.Entity<OvertimeRequest>(entity =>
        {
            entity.HasKey(e => e.Otid).HasName("PK__Overtime__A934AEC48E16FE0E");

            entity.HasIndex(e => e.Otdate, "IX_OvertimeRequests_Date");

            entity.HasIndex(e => e.EmployeeId, "IX_OvertimeRequests_Employee");

            entity.HasIndex(e => e.Status, "IX_OvertimeRequests_Status");

            entity.Property(e => e.Otid).HasColumnName("OTId");
            entity.Property(e => e.ApprovedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.Otdate).HasColumnName("OTDate");
            entity.Property(e => e.Reason).HasMaxLength(1000);
            entity.Property(e => e.RejectionReason).HasMaxLength(500);
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("PENDING");
            entity.Property(e => e.TotalHours).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.OvertimeRequestApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK_OvertimeRequests_ApprovedBy");

            entity.HasOne(d => d.Employee).WithMany(p => p.OvertimeRequestEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OvertimeRequests_Employee");
        });

        modelBuilder.Entity<Payroll>(entity =>
        {
            entity.HasKey(e => e.PayrollId).HasName("PK__Payrolls__99DFC6722AEA2502");

            entity.HasIndex(e => e.EmployeeId, "IX_Payrolls_Employee");

            entity.HasIndex(e => new { e.PayrollYear, e.PayrollMonth }, "IX_Payrolls_Period");

            entity.HasIndex(e => new { e.EmployeeId, e.PayrollMonth, e.PayrollYear }, "UQ_Payroll_Period").IsUnique();

            entity.Property(e => e.BaseSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ConfirmedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.GrossSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NetSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("DRAFT");
            entity.Property(e => e.TotalAllowance).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalBonus).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalDeduction).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalOvertime).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.ConfirmedByUser).WithMany(p => p.PayrollConfirmedByUsers)
                .HasForeignKey(d => d.ConfirmedByUserId)
                .HasConstraintName("FK_Payrolls_ConfirmedByUser");

            entity.HasOne(d => d.Employee).WithMany(p => p.Payrolls)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payrolls_Employee");

            entity.HasOne(d => d.GeneratedByUser).WithMany(p => p.PayrollGeneratedByUsers)
                .HasForeignKey(d => d.GeneratedByUserId)
                .HasConstraintName("FK_Payrolls_GeneratedByUser");
        });

        modelBuilder.Entity<PayrollDetail>(entity =>
        {
            entity.HasKey(e => e.PayrollDetailId).HasName("PK__PayrollD__010127C9D4C2586E");

            entity.HasIndex(e => e.PayrollId, "IX_PayrollDetails_Payroll");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.ItemType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SourceType)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Payroll).WithMany(p => p.PayrollDetails)
                .HasForeignKey(d => d.PayrollId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PayrollDetails_Payroll");
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.PositionId).HasName("PK__Position__60BB9A79B94D2CF9");

            entity.HasIndex(e => e.PositionName, "UQ_Positions_PositionName").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PositionName).HasMaxLength(150);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A993DF1CB");

            entity.HasIndex(e => e.RoleName, "UQ_Roles_RoleName").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CCB13402E");

            entity.HasIndex(e => e.IsActive, "IX_Users_IsActive");

            entity.HasIndex(e => e.Email, "UQ_Users_Email").IsUnique();

            entity.HasIndex(e => e.Username, "UQ_Users_Username").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastLoginAt).HasColumnType("datetime");
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });

            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoles_Role");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoles_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
