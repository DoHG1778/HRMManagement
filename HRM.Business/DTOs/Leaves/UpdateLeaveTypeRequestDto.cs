using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Leaves
{
    public class UpdateLeaveTypeRequestDto
    {
        [MaxLength(100)]
        public string? LeaveTypeName { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Max days per year must be greater than or equal to 0.")]
        public int? MaxDaysPerYear { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool? IsActive { get; set; }
    }
}