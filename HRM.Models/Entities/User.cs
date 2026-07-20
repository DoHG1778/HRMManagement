using System;
using System.Collections.Generic;

namespace HRM.Models.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual ICollection<Kpi> Kpis { get; set; } = new List<Kpi>();

    public virtual ICollection<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Payroll> PayrollConfirmedByUsers { get; set; } = new List<Payroll>();

    public virtual ICollection<Payroll> PayrollGeneratedByUsers { get; set; } = new List<Payroll>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
