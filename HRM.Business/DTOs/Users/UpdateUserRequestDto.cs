using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Users
{
    public class UpdateUserRequestDto
    {
        [EmailAddress(ErrorMessage = "Email format is invalid.")]
        [MaxLength(100)]
        public string? Email { get; set; }

        public bool? IsActive { get; set; }

        public List<int>? RoleIds { get; set; }
    }
}