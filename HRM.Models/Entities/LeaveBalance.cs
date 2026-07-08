using System;
using System.Collections.Generic;

namespace HRM.Models.Entities;

public partial class LeaveBalance
{
    public int LeaveBalanceId { get; set; }

    public int EmployeeId { get; set; }

    public int LeaveTypeId { get; set; }

    public int Year { get; set; }

    public int TotalDays { get; set; }

    public int UsedDays { get; set; }

    public int? RemainingDays { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual LeaveType LeaveType { get; set; } = null!;
}
