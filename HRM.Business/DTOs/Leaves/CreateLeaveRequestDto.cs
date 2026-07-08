using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Leaves
{
    public class CreateLeaveRequestDto
    {
        [Required(ErrorMessage = "Leave type is required.")]
        public int LeaveTypeId { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        public DateOnly StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        public DateOnly EndDate { get; set; }

        [MaxLength(1000)]
        public string? Reason { get; set; }
    }
}