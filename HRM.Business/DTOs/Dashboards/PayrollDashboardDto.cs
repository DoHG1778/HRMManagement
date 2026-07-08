namespace HRM.Business.DTOs.Dashboards
{
    public class PayrollDashboardDto
    {
        public int PayrollMonth { get; set; }

        public int PayrollYear { get; set; }

        public int TotalPayrolls { get; set; }

        public int DraftPayrolls { get; set; }

        public int ConfirmedPayrolls { get; set; }

        public int PaidPayrolls { get; set; }

        public decimal TotalGrossSalary { get; set; }

        public decimal TotalNetSalary { get; set; }

        public decimal TotalOvertime { get; set; }

        public decimal TotalDeduction { get; set; }
    }
}