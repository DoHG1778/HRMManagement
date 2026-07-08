using System;
using System.Collections.Generic;

namespace HRM.Models.Entities;

public partial class EmployeeEducation
{
    public int EducationId { get; set; }

    public int EmployeeId { get; set; }

    public string SchoolName { get; set; } = null!;

    public string? Major { get; set; }

    public string? Degree { get; set; }

    public int? GraduationYear { get; set; }

    public decimal? Gpa { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
