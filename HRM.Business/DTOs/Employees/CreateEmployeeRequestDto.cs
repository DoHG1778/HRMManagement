using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Employees
{
    public class CreateEmployeeRequestDto
    {
        [Required(ErrorMessage = "Employee code is required.")]
        [MaxLength(20)]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required.")]
        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty;

        public DateOnly? DateOfBirth { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email format is invalid.")]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? Cccd { get; set; }

        [Required(ErrorMessage = "Hire date is required.")]
        public DateOnly HireDate { get; set; }

        public int? UserId { get; set; }

        public int? ManagerId { get; set; }

        [MaxLength(500)]
        public string? AvatarUrl { get; set; }
    }
}