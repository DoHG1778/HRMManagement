using System;
using System.Collections.Generic;

namespace HRM.Models.Entities;

public partial class Payroll
{
    public int PayrollId { get; set; }

    public int EmployeeId { get; set; }

    public int PayrollMonth { get; set; }

    public int PayrollYear { get; set; }

    public decimal BaseSalary { get; set; }

    public decimal TotalAllowance { get; set; }

    public decimal TotalBonus { get; set; }

    public decimal TotalOvertime { get; set; }

    public decimal TotalDeduction { get; set; }

    public decimal GrossSalary { get; set; }

    public decimal NetSalary { get; set; }

    public string Status { get; set; } = null!;

    public int? GeneratedByUserId { get; set; }

    public int? ConfirmedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? ConfirmedByUser { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual User? GeneratedByUser { get; set; }

    public virtual ICollection<PayrollDetail> PayrollDetails { get; set; } = new List<PayrollDetail>();
}
