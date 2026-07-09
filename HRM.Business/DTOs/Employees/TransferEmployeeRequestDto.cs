using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Employees
{
    public class TransferEmployeeRequestDto
    {
        [Required(ErrorMessage = "New department is required.")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "New position is required.")]
        public int PositionId { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        public DateOnly StartDate { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
