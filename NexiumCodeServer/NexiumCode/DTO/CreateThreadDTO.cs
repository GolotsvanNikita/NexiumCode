using System.ComponentModel.DataAnnotations;

namespace NexiumCode.DTO
{
    public class CreateThreadDTO
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public string Category { get; set; }
    }
}
