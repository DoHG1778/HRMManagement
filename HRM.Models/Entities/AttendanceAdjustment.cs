using System;
using System.Collections.Generic;

namespace HRM.Models.Entities;

public partial class AttendanceAdjustment
{
    public int AdjustmentId { get; set; }

    public int AttendanceId { get; set; }

    public int RequestedBy { get; set; }

    public DateTime? RequestedCheckInTime { get; set; }

    public DateTime? RequestedCheckOutTime { get; set; }

    public string Reason { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Employee? ApprovedByNavigation { get; set; }

    public virtual Attendance Attendance { get; set; } = null!;

    public virtual Employee RequestedByNavigation { get; set; } = null!;
}
