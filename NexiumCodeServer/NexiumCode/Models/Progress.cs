using System.ComponentModel.DataAnnotations;

namespace NexiumCode.Models
{
    public class Progress
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int CourseId { get; set; }

        public int TheoryProgress { get; set; } = 0;

        public int PracticeProgress { get; set; } = 0;

        public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;

        public User User { get; set; }

        public Course Course { get; set; }
        public ICollection<ProgressLesson> ProgressLessons { get; set; } = [];
    }
}