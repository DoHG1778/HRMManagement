using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Employees
{
    public class UpdateMyProfileRequestDto
    {
        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(500)]
        public string? AvatarUrl { get; set; }
    }
}