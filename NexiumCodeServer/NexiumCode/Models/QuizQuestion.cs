using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexiumCode.Models
{
    public class QuizQuestion
    {
        public int Id { get; set; }

        [Required]
        public int LessonId { get; set; }

        [Required]
        public string QuestionText { get; set; }

        public string Options { get; set; }

        public string CorrectAnswer { get; set; }

        public Lesson Lesson { get; set; }
    }
}
