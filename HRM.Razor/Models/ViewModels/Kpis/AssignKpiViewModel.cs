using System.ComponentModel.DataAnnotations;

namespace HRM.Razor.Models.ViewModels.Kpis
{
    public class AssignKpiViewModel
    {
        [Required(ErrorMessage = "Please select a KPI.")]
        public int Kpiid { get; set; }

        [Required(ErrorMessage = "Please select an employee.")]
        public int EmployeeId { get; set; }

        public decimal? TargetValue { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }
    }
}