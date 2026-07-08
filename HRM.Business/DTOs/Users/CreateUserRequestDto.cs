using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Users
{
    public class CreateUserRequestDto
    {
        [Required(ErrorMessage = "Username is required.")]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email format is invalid.")]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        public int? EmployeeId { get; set; }

        public List<int> RoleIds { get; set; } = new();
    }
}