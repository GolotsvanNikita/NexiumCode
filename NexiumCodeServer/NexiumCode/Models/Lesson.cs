using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexiumCode.Models
{
    public class Lesson
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Content { get; set; }

        public bool IsTheory { get; set; } = true;

        public int Order { get; set; }

        public Course Course { get; set; }

        public ICollection<QuizQuestion> QuizQuestions { get; set; }
        public ICollection<PracticeTask> PracticeTasks { get; set; }
    }
}
