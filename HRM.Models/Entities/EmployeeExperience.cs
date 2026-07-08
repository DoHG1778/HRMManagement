using System;
using System.Collections.Generic;

namespace HRM.Models.Entities;

public partial class EmployeeExperience
{
    public int ExperienceId { get; set; }

    public int EmployeeId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string? Position { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
