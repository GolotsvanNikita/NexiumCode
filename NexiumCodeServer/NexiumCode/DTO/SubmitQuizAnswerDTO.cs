using System.ComponentModel.DataAnnotations;

namespace NexiumCode.DTO
{
    public class SubmitQuizAnswerDTO
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string Answer { get; set; }
    }
}
