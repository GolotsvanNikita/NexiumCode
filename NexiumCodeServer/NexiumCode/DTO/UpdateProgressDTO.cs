using System.ComponentModel.DataAnnotations;

namespace NexiumCode.DTO
{
    public class UpdateProgressDTO
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        [Range(0, 100)]
        public int Progress { get; set; }
    }
}
