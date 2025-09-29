using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexiumCode.Models
{
    public class PracticeTask
    {
        public int Id { get; set; }

        [Required]
        public int LessonId { get; set; }

        [Required]
        public string TaskDescription { get; set; }

        public string StarterCode { get; set; }

        public string TestCases { get; set; }

        public int AverageTimeSeconds { get; set; }

        public Lesson Lesson { get; set; }
    }
}
