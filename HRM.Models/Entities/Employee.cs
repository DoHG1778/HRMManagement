using System;
using System.Collections.Generic;

namespace HRM.Models.Entities;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string EmployeeCode { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public bool Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Phone { get; set; }

    public string Email { get; set; } = null!;

    public string? Address { get; set; }

    public string? Cccd { get; set; }

    public DateOnly HireDate { get; set; }

    public string EmploymentStatus { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public int? UserId { get; set; }

    public int? ManagerId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AttendanceAdjustment> AttendanceAdjustmentApprovedByNavigations { get; set; } = new List<AttendanceAdjustment>();

    public virtual ICollection<AttendanceAdjustment> AttendanceAdjustmentRequestedByNavigations { get; set; } = new List<AttendanceAdjustment>();

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Contract? Contract { get; set; }

    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

    public virtual EmployeeAssignment? EmployeeAssignment { get; set; }

    public virtual ICollection<EmployeeBankAccount> EmployeeBankAccounts { get; set; } = new List<EmployeeBankAccount>();

    public virtual ICollection<EmployeeCertificate> EmployeeCertificates { get; set; } = new List<EmployeeCertificate>();

    public virtual ICollection<EmployeeContact> EmployeeContacts { get; set; } = new List<EmployeeContact>();

    public virtual ICollection<EmployeeEducation> EmployeeEducations { get; set; } = new List<EmployeeEducation>();

    public virtual ICollection<EmployeeExperience> EmployeeExperiences { get; set; } = new List<EmployeeExperience>();

    public virtual ICollection<Evaluation> EvaluationEmployees { get; set; } = new List<Evaluation>();

    public virtual ICollection<Evaluation> EvaluationEvaluators { get; set; } = new List<Evaluation>();

    public virtual ICollection<Employee> InverseManager { get; set; } = new List<Employee>();

    public virtual ICollection<Kpiassignment> KpiassignmentAssignedByNavigations { get; set; } = new List<Kpiassignment>();

    public virtual ICollection<Kpiassignment> KpiassignmentEmployees { get; set; } = new List<Kpiassignment>();

    public virtual ICollection<Kpiassignment> KpiassignmentReviewedByNavigations { get; set; } = new List<Kpiassignment>();

    public virtual ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();

    public virtual ICollection<LeaveRequest> LeaveRequestApprovedByNavigations { get; set; } = new List<LeaveRequest>();

    public virtual ICollection<LeaveRequest> LeaveRequestEmployees { get; set; } = new List<LeaveRequest>();

    public virtual Employee? Manager { get; set; }

    public virtual ICollection<OvertimeRequest> OvertimeRequestApprovedByNavigations { get; set; } = new List<OvertimeRequest>();

    public virtual ICollection<OvertimeRequest> OvertimeRequestEmployees { get; set; } = new List<OvertimeRequest>();

    public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();

    public virtual User? User { get; set; }
}
