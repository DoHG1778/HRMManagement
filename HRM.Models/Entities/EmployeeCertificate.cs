using System;
using System.Collections.Generic;

namespace HRM.Models.Entities;

public partial class EmployeeCertificate
{
    public int CertificateId { get; set; }

    public int EmployeeId { get; set; }

    public string CertificateName { get; set; } = null!;

    public string? IssuedBy { get; set; }

    public DateOnly? IssueDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public string? CertificateNumber { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
