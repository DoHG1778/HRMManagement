using System;
using System.Collections.Generic;

namespace HRM.Models.Entities;

public partial class EmployeeBankAccount
{
    public int BankAccountId { get; set; }

    public int EmployeeId { get; set; }

    public string BankName { get; set; } = null!;

    public string AccountNumber { get; set; } = null!;

    public string AccountHolder { get; set; } = null!;

    public bool IsPrimary { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
