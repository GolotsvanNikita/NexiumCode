using System.ComponentModel.DataAnnotations;

namespace NexiumCode.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public ICollection<Lesson> Lessons { get; set; }
        public ICollection<Progress> Progresses { get; set; }
        public ICollection<Certificate> Certificates { get; set; }
    }
}
