namespace HRM.Razor.Models.ViewModels.Payroll
{
    public class PayrollReportViewModel
    {
        public int PayrollMonth { get; set; }
        public int PayrollYear { get; set; }
        public int TotalEmployees { get; set; }
        public decimal TotalBaseSalary { get; set; }
        public decimal TotalAllowance { get; set; }
        public decimal TotalBonus { get; set; }
        public decimal TotalOvertime { get; set; }
        public decimal TotalDeduction { get; set; }
        public decimal TotalGrossSalary { get; set; }
        public decimal TotalNetSalary { get; set; }
        public List<PayrollItemModel> Payrolls { get; set; } = new();

        public List<DepartmentPayrollSummaryModel> DepartmentSummaries { get; set; } = new();
    }

    public class DepartmentPayrollSummaryModel
    {
        public string DepartmentName { get; set; } = "Chưa phân công";
        public int EmployeeCount { get; set; }
        public decimal TotalBaseSalary { get; set; }
        public decimal TotalGrossSalary { get; set; }
        public decimal TotalDeduction { get; set; }
        public decimal TotalNetSalary { get; set; }
    }
}
