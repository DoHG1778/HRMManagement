using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Positions
{
    public class UpdatePositionRequestDto
    {
        [MaxLength(150)]
        public string? PositionName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool? IsActive { get; set; }
    }
}