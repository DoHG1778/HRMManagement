using System.ComponentModel.DataAnnotations;

namespace HRM.Razor.Models.ViewModels.Payroll
{
    public class MyPayslipFilterViewModel
    {
        [Display(Name = "Tháng")]
        public int Month { get; set; } = DateTime.Today.Month;

        [Display(Name = "Năm")]
        public int Year { get; set; } = DateTime.Today.Year;
    }
}
