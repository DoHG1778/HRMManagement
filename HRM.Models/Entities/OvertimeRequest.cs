using System;
using System.Collections.Generic;

namespace HRM.Models.Entities;

public partial class OvertimeRequest
{
    public int Otid { get; set; }

    public int EmployeeId { get; set; }

    public DateOnly Otdate { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public decimal TotalHours { get; set; }

    public string? Reason { get; set; }

    public string Status { get; set; } = null!;

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? RejectionReason { get; set; }

    public bool IsTransferredToPayroll { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Employee? ApprovedByNavigation { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
