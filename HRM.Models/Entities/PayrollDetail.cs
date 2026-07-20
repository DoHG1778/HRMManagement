using System;
using System.Collections.Generic;

namespace HRM.Models.Entities;

public partial class PayrollDetail
{
    public int PayrollDetailId { get; set; }

    public int PayrollId { get; set; }

    public string ItemType { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Amount { get; set; }

    public string? SourceType { get; set; }

    public int? SourceId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Payroll Payroll { get; set; } = null!;
}
