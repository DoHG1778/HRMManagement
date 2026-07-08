using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Leaves
{
    public class SetLeaveBalanceRequestDto
    {
        [Required(ErrorMessage = "EmployeeId is required.")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "LeaveTypeId is required.")]
        public int LeaveTypeId { get; set; }

        [Range(2000, 2100, ErrorMessage = "Year must be between 2000 and 2100.")]
        public int Year { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Total days must be greater than or equal to 0.")]
        public int TotalDays { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Used days must be greater than or equal to 0.")]
        public int UsedDays { get; set; }
    }
}