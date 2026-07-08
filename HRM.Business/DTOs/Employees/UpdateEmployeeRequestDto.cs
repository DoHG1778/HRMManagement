using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Employees
{
    public class UpdateEmployeeRequestDto
    {
        [MaxLength(150)]
        public string? FullName { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Email format is invalid.")]
        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? Cccd { get; set; }

        public DateOnly? HireDate { get; set; }

        [MaxLength(30)]
        public string? EmploymentStatus { get; set; }

        public int? UserId { get; set; }

        public int? ManagerId { get; set; }

        [MaxLength(500)]
        public string? AvatarUrl { get; set; }
    }
}