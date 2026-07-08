using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Users
{
    public class AssignRoleRequestDto
    {
        [Required(ErrorMessage = "UserId is required.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "RoleIds is required.")]
        public List<int> RoleIds { get; set; } = new();
    }
}