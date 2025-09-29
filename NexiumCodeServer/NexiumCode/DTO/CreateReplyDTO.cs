using System.ComponentModel.DataAnnotations;

namespace NexiumCode.DTO
{
    public class CreateReplyDTO
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string Content { get; set; }
    }
}
