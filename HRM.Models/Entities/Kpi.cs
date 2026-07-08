using System;
using System.Collections.Generic;

namespace HRM.Models.Entities;

public partial class Kpi
{
    public int Kpiid { get; set; }

    public string Kpiname { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Weight { get; set; }

    public bool IsActive { get; set; }

    public int? CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? CreatedByUser { get; set; }

    public virtual ICollection<Kpiassignment> Kpiassignments { get; set; } = new List<Kpiassignment>();
}
