using System;
using System.Collections.Generic;

namespace HRM.Models.Entities;

public partial class Kpiassignment
{
    public int AssignmentId { get; set; }

    public int Kpiid { get; set; }

    public int EmployeeId { get; set; }

    public int? AssignedBy { get; set; }

    public decimal? TargetValue { get; set; }

    public decimal? ActualValue { get; set; }

    public decimal ProgressPercent { get; set; }

    public decimal? EmployeeSelfScore { get; set; }

    public string? EmployeeComment { get; set; }

    public decimal? ManagerScore { get; set; }

    public string? ManagerComment { get; set; }

    public int? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Employee? AssignedByNavigation { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Kpi Kpi { get; set; } = null!;

    public virtual Employee? ReviewedByNavigation { get; set; }
}
