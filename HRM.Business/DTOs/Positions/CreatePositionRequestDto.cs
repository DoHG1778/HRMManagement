using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Positions
{
    public class CreatePositionRequestDto
    {
        [Required(ErrorMessage = "Position name is required.")]
        [MaxLength(150)]
        public string PositionName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}