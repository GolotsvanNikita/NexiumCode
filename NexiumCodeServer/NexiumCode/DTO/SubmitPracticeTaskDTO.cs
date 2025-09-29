using System.ComponentModel.DataAnnotations;

namespace NexiumCode.DTO
{
    public class SubmitPracticeTaskDTO
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string Code { get; set; }
    }
}
