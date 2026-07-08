using System;
using System.Collections.Generic;

namespace HRM.Models.Entities;

public partial class EmployeeContact
{
    public int ContactId { get; set; }

    public int EmployeeId { get; set; }

    public string ContactName { get; set; } = null!;

    public string? Relationship { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public bool IsPrimary { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
