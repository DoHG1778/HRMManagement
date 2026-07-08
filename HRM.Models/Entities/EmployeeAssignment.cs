using System;
using System.Collections.Generic;

namespace HRM.Models.Entities;

public partial class EmployeeAssignment
{
    public int AssignmentId { get; set; }

    public int EmployeeId { get; set; }

    public int DepartmentId { get; set; }

    public int PositionId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;

    public virtual Position Position { get; set; } = null!;
}
