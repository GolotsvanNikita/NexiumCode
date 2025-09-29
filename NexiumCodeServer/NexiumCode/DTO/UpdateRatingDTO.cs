using System.ComponentModel.DataAnnotations;

namespace NexiumCode.DTO
{
    public class UpdateRatingDTO
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Rating { get; set; }
    }
}
