using System;
using System.Collections.Generic;

namespace HRM.Models.Entities;

public partial class Evaluation
{
    public int EvaluationId { get; set; }

    public int EmployeeId { get; set; }

    public int EvaluatorId { get; set; }

    public string EvaluationPeriod { get; set; } = null!;

    public decimal? TotalScore { get; set; }

    public string? Comment { get; set; }

    public string Status { get; set; } = null!;

    public DateTime EvaluationDate { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Employee Evaluator { get; set; } = null!;
}
