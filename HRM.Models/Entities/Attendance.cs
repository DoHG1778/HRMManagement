using System;
using System.Collections.Generic;

namespace HRM.Models.Entities;

public partial class Attendance
{
    public int AttendanceId { get; set; }

    public int EmployeeId { get; set; }

    public DateOnly AttendanceDate { get; set; }

    public DateTime? CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }

    public decimal WorkingHours { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AttendanceAdjustment> AttendanceAdjustments { get; set; } = new List<AttendanceAdjustment>();

    public virtual Employee Employee { get; set; } = null!;
}
