namespace HRM.Razor.Models.ViewModels.Payroll
{
    public class PayrollItemModel
    {
        public int PayrollId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string? PositionName { get; set; }
        public int PayrollMonth { get; set; }
        public int PayrollYear { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal TotalAllowance { get; set; }
        public decimal TotalBonus { get; set; }
        public decimal TotalOvertime { get; set; }
        public decimal TotalDeduction { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal NetSalary { get; set; }
        public string Status { get; set; } = "DRAFT";
        public string? GeneratedByName { get; set; }
        public string? ConfirmedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<PayrollDetailItemModel> PayrollDetails { get; set; } = new();
    }
}
